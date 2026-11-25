pipeline {
    agent any

    options {
        skipDefaultCheckout(true)
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '20'))
    }

    parameters {
        string(name: 'GITHUB_REPOSITORY', defaultValue: 'louis3588/Catalystic', description: 'GitHub repository in owner/repository format')
    }

    triggers { cron('H/15 * * * *') }

    stages {
        stage('Fetch GitHub Actions reports') {
            steps {
                dir('reports') { deleteDir() }
                withCredentials([string(credentialsId: 'github-actions-read-token', variable: 'GITHUB_TOKEN')]) {
                    sh '''#!/bin/sh
                        set -eu
                        python3 - <<'PY'
import io, json, os, pathlib, re, urllib.request, zipfile

repository = os.environ['GITHUB_REPOSITORY']
if not re.fullmatch(r'[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+', repository):
    raise SystemExit('Set GITHUB_REPOSITORY to owner/repository.')
base = 'https://api.github.com/repos/' + repository
headers = {'Authorization': 'Bearer ' + os.environ['GITHUB_TOKEN'], 'Accept': 'application/vnd.github+json', 'X-GitHub-Api-Version': '2022-11-28'}
def get(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=headers), timeout=30) as response:
        return response.read()

runs = json.loads(get(base + '/actions/workflows/main.yml/runs?branch=master&event=push&status=success&per_page=1'))['workflow_runs']
if not runs:
    raise SystemExit('No successful master push workflow run is available yet.')
run = runs[0]
artifacts = json.loads(get(base + '/actions/runs/' + str(run['id']) + '/artifacts?per_page=100'))['artifacts']
root = pathlib.Path('reports').resolve()
for name in ('backend-reports', 'frontend-reports'):
    matches = [item for item in artifacts if item['name'] == name and not item['expired']]
    if len(matches) != 1:
        raise SystemExit('Required artifact missing or expired: ' + name)
    archive = zipfile.ZipFile(io.BytesIO(get(base + '/actions/artifacts/' + str(matches[0]['id']) + '/zip')))
    destination = root / name
    destination.mkdir(parents=True, exist_ok=True)
    for member in archive.infolist():
        target = (destination / member.filename).resolve()
        if not target.is_relative_to(destination):
            raise SystemExit('Unsafe artifact path.')
        if member.is_dir():
            target.mkdir(parents=True, exist_ok=True)
        else:
            target.parent.mkdir(parents=True, exist_ok=True)
            with archive.open(member) as source, target.open('wb') as output:
                output.write(source.read())
print('Imported successful GitHub Actions run', run['id'], 'commit', run['head_sha'])
pathlib.Path('reports/run-id.txt').write_text(str(run['id']))
PY'''
                }
                script {
                    def runId = readFile('reports/run-id.txt').trim()
                    def previous = currentBuild.previousSuccessfulBuild
                    env.REPORT_ALREADY_PUBLISHED = (previous?.description == "GitHub Actions run ${runId}").toString()
                    currentBuild.description = "GitHub Actions run ${runId}"
                }
            }
        }
        stage('Publish reports') {
            when { expression { env.REPORT_ALREADY_PUBLISHED != 'true' } }
            steps {
                junit testResults: 'reports/**/junit.xml', allowEmptyResults: false
                recordCoverage tools: [[parser: 'COBERTURA', pattern: 'reports/**/coverage.cobertura.xml'], [parser: 'COBERTURA', pattern: 'reports/**/cobertura-coverage.xml']], sourceCodeRetention: 'NEVER'
            }
        }
    }
}
