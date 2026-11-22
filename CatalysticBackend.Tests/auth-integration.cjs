// Runs only against a disposable PostgreSQL cluster created by this script.
const { spawn, spawnSync } = require('node:child_process');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const https = require('node:https');
const assert = require('node:assert/strict');

const repo = path.resolve(__dirname, '..');
const backend = path.join(repo, 'CatalysticBackend');
const pgBin = process.env.CATALYSTIC_TEST_PG_BIN || 'D:\\Program Files\\Postgres\\bin';
const dotnet = process.env.CATALYSTIC_TEST_DOTNET || path.join(os.homedir(), '.dotnet', 'dotnet.exe');
const dataDir = fs.mkdtempSync(path.join(os.tmpdir(), 'catalystic-auth-test-'));
const dbPort = 55439;
const webPort = 55440;
const firstEmail = 'test-admin@example.invalid';
const secondEmail = 'other-admin@example.invalid';
const dbEnv = {
    ...process.env,
    DOTNET_ROOT: path.dirname(dotnet),
    PATH: `${path.dirname(dotnet)}${path.delimiter}${process.env.PATH}`,
    PGHOST: `127.0.0.1:${dbPort}`,
    PGDATABASE: 'catalystic_auth_test',
    PGUSER: 'catalystic_test',
    PGPASSWORD: 'local-test-only',
    PGSSLMODE: 'disable',
    PGCHANNELBINDING: 'disable',
    ASPNETCORE_ENVIRONMENT: 'Development',
    ASPNETCORE_URLS: `https://localhost:${webPort}`,
};

function run(command, args, options = {}) {
    const result = spawnSync(command, args, { cwd: repo, env: dbEnv, encoding: 'utf8', ...options });
    if (result.error || result.status !== 0) {
        throw new Error(`${path.basename(command)} failed: ${result.error || result.stderr || result.stdout}`);
    }
    return result.stdout;
}

function pg(name) {
    return path.join(pgBin, `${name}.exe`);
}

function waitForOutput(child, needle) {
    return new Promise((resolve, reject) => {
        let output = '';
        const timer = setTimeout(() => reject(new Error(`Timed out waiting for ${needle}: ${output}`)), 30000);
        const collect = chunk => {
            output += chunk.toString();
            if (output.includes(needle)) {
                clearTimeout(timer);
                resolve();
            }
        };
        child.stdout.on('data', collect);
        child.stderr.on('data', collect);
        child.once('exit', code => {
            clearTimeout(timer);
            reject(new Error(`Process exited ${code}: ${output}`));
        });
    });
}

function bootstrap(email) {
    return new Promise(resolve => {
        const child = spawn(dotnet, [path.join(backend, 'bin', 'Debug', 'net10.0', 'CatalysticBackend.dll'), 'bootstrap-admin'],
            { cwd: backend, env: dbEnv, stdio: ['pipe', 'pipe', 'pipe'] });
        let stdout = '';
        child.stdout.on('data', chunk => { stdout += chunk; });
        child.stderr.resume();
        child.on('exit', code => resolve({ code, email, secret: stdout.match(/\b[A-F0-9]{48}\b/)?.[0] }));
        child.stdin.end(`${email}\n`);
    });
}

function client() {
    const cookies = new Map();
    return {
        async request(method, route, body, token) {
            const payload = body === undefined ? undefined : JSON.stringify(body);
            return new Promise((resolve, reject) => {
                const headers = {};
                if (payload) headers['Content-Type'] = 'application/json';
                if (token) headers['X-CSRF-TOKEN'] = token;
                if (cookies.size) headers.Cookie = [...cookies].map(([key, value]) => `${key}=${value}`).join('; ');
                const request = https.request({ hostname: 'localhost', port: webPort, path: route, method,
                    rejectUnauthorized: false, headers }, response => {
                    for (const cookie of response.headers['set-cookie'] || []) {
                        const pair = cookie.split(';', 1)[0];
                        const split = pair.indexOf('=');
                        cookies.set(pair.slice(0, split), pair.slice(split + 1));
                    }
                    let result = '';
                    response.on('data', chunk => { result += chunk; });
                    response.on('end', () => resolve({ status: response.statusCode, body: result }));
                });
                request.on('error', reject);
                if (payload) request.write(payload);
                request.end();
            });
        },
        async token() {
            const response = await this.request('GET', '/api/auth/csrf');
            assert.equal(response.status, 200);
            return JSON.parse(response.body).token;
        },
    };
}

(async () => {
    let postgres;
    let server;
    try {
        run(pg('initdb'), ['-D', dataDir, '-U', 'catalystic_test', '--auth=trust', '--no-locale', '--encoding=UTF8']);
        postgres = spawn(pg('postgres'), ['-D', dataDir, '-h', '127.0.0.1', '-p', String(dbPort)],
            { cwd: repo, stdio: ['ignore', 'pipe', 'pipe'] });
        await waitForOutput(postgres, 'ready to accept connections');
        run(pg('createdb'), ['-h', '127.0.0.1', '-p', String(dbPort), '-U', 'catalystic_test', 'catalystic_auth_test']);
        run(dotnet, ['ef', 'database', 'update', '--project', path.join(backend, 'CatalysticBackend.csproj')]);
        const sql = `INSERT INTO "Users" ("FirstName", "Email", "PasswordHash", "PhoneNumber", "City", "DateOfBirth", "ProfileImageUrl", "IsActive", "IsEmailVerified", "IsPhoneNumberVerified", "MustChangePassword", "SessionVersion") VALUES ('Test', '${firstEmail}', '', '', '', '2000-01-01Z', '', false, false, false, false, '00000000-0000-0000-0000-000000000000'), ('Test', '${secondEmail}', '', '', '', '2000-01-01Z', '', false, false, false, false, '00000000-0000-0000-0000-000000000000')`;
        run(pg('psql'), ['-h', '127.0.0.1', '-p', String(dbPort), '-U', 'catalystic_test', '-d', 'catalystic_auth_test', '-v', 'ON_ERROR_STOP=1', '-c', sql]);
        const attempts = await Promise.all([bootstrap(firstEmail), bootstrap(secondEmail)]);
        assert.equal(attempts.filter(attempt => attempt.code === 0).length, 1, 'only one bootstrap may succeed');
        const winner = attempts.find(attempt => attempt.code === 0);
        const { email, secret } = winner;
        assert.ok(secret, 'bootstrap must emit a test credential');
        const bootstrapState = run(pg('psql'), [
            '-h', '127.0.0.1', '-p', String(dbPort), '-U', 'catalystic_test',
            '-d', 'catalystic_auth_test', '-tAc',
            'SELECT "IsActive" AND "IsEmailVerified" AND "IsPhoneNumberVerified" AND "MustChangePassword" AND "Role" = \'Admin\' AND "BootstrapExpiresAt" > now() AND "PasswordHash" <> \'\' FROM "Users" WHERE "Role" = \'Admin\'',
        ]).trim();
        assert.equal(bootstrapState, 't', 'bootstrap must set administrator state');
        server = spawn(dotnet, [path.join(backend, 'bin', 'Debug', 'net10.0', 'CatalysticBackend.dll')],
            { cwd: backend, env: dbEnv, stdio: ['ignore', 'pipe', 'pipe'] });
        await waitForOutput(server, 'Application started');
        const first = client();
        let token = await first.token();
        const credentials = { email, password: secret };
        assert.equal((await first.request('POST', '/api/auth/login', credentials)).status, 400);
        assert.equal((await first.request('POST', '/api/auth/login', credentials, 'wrong')).status, 400);
        let response = await first.request('POST', '/api/auth/login', credentials, token);
        assert.equal(response.status, 200);
        assert.equal(JSON.parse(response.body).passwordChangeRequired, true);
        assert.equal((await first.request('POST', '/api/auth/logout', {}, token)).status, 401);
        const newPassword = 'Initial test password 123!';
        response = await first.request('POST', '/api/auth/bootstrap-password', { ...credentials, newPassword }, token);
        assert.equal(response.status, 204, `bootstrap password update: ${response.body}`);
        assert.equal((await first.request('POST', '/api/auth/bootstrap-password', { ...credentials, newPassword }, token)).status, 401);
        response = await first.request('POST', '/api/auth/login', { email, password: newPassword }, token);
        assert.equal(response.status, 200);
        assert.equal(JSON.parse(response.body).passwordChangeRequired, false);
        const second = client();
        let secondToken = await second.token();
        assert.equal((await second.request('POST', '/api/auth/login', { email, password: newPassword }, secondToken)).status, 200);
        token = await first.token();
        secondToken = await second.token();
        assert.equal((await first.request('POST', '/api/auth/logout', {}, 'wrong')).status, 400);
        assert.equal((await first.request('POST', '/api/auth/logout', {}, token)).status, 204);
        assert.equal((await second.request('POST', '/api/auth/logout', {}, secondToken)).status, 401);

        token = await first.token();
        assert.equal((await first.request('POST', '/api/auth/login', { email, password: newPassword }, token)).status, 200);
        const third = client();
        let thirdToken = await third.token();
        assert.equal((await third.request('POST', '/api/auth/login', { email, password: newPassword }, thirdToken)).status, 200);
        token = await first.token();
        thirdToken = await third.token();
        const changedPassword = 'Updated test password 456!';
        assert.equal((await first.request('POST', '/api/auth/password', {
            currentPassword: newPassword,
            newPassword: changedPassword,
        }, token)).status, 204);
        assert.equal((await third.request('POST', '/api/auth/logout', {}, thirdToken)).status, 401);

        // Restart to reset the process-local login rate limiter for the remaining checks.
        await new Promise(resolve => {
            server.once('exit', resolve);
            server.kill();
        });
        server = spawn(dotnet, [path.join(backend, 'bin', 'Debug', 'net10.0', 'CatalysticBackend.dll')],
            { cwd: backend, env: dbEnv, stdio: ['ignore', 'pipe', 'pipe'] });
        await waitForOutput(server, 'Application started');
        const afterChange = client();
        const afterChangeToken = await afterChange.token();
        assert.equal((await afterChange.request('POST', '/api/auth/login', { email, password: newPassword }, afterChangeToken)).status, 401);
        assert.equal((await afterChange.request('POST', '/api/auth/login', { email, password: changedPassword }, afterChangeToken)).status, 200);
        console.log('PostgreSQL bootstrap concurrency, HTTPS, CSRF, password change and session revocation checks passed.');
    } finally {
        if (server) server.kill();
        if (postgres) {
            spawnSync(pg('pg_ctl'), ['-D', dataDir, '-m', 'fast', '-w', 'stop'], { encoding: 'utf8' });
        }
        fs.rmSync(dataDir, { recursive: true, force: true, maxRetries: 10, retryDelay: 500 });
    }
})().catch(error => {
    console.error(error);
    process.exitCode = 1;
});
