pipeline {
    agent any

    options {
        skipDefaultCheckout(true)
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '20'))
    }

    parameters {
        string(name: 'GITHUB_REPOSITORY', defaultValue: 'louis3588/Catalystic', description: 'GitHub repository in owner/repository format')
        booleanParam(name: 'FORCE_PUBLISH', defaultValue: false, description: 'Republish the current run after changing report configuration')
    }

    triggers { cron('H/15 * * * *') }

    stages {
        stage('Fetch GitHub Actions reports') {
            steps {
                dir('reports') { deleteDir() }
                withEnv(["GITHUB_REPOSITORY=${params.GITHUB_REPOSITORY ?: 'louis3588/Catalystic'}"]) {
                withCredentials([string(credentialsId: 'github-actions-read-token', variable: 'GITHUB_TOKEN')]) {
                    sh '''#!/bin/sh
                        set -eu
                        python3 - <<'PY'
import io, json, os, pathlib, re, urllib.parse, urllib.request, zipfile
import xml.etree.ElementTree as ET

repository = os.environ['GITHUB_REPOSITORY']
if not re.fullmatch(r'[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+', repository):
    raise SystemExit('Set GITHUB_REPOSITORY to owner/repository.')
base = 'https://api.github.com/repos/' + repository
headers = {'Authorization': 'Bearer ' + os.environ['GITHUB_TOKEN'], 'Accept': 'application/vnd.github+json', 'X-GitHub-Api-Version': '2022-11-28'}
class SafeRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, response_headers, newurl):
        redirected = super().redirect_request(req, fp, code, msg, response_headers, newurl)
        if redirected and urllib.parse.urlparse(newurl).netloc != urllib.parse.urlparse(req.full_url).netloc:
            redirected.remove_header('Authorization')
            redirected.remove_header('X-github-api-version')
        return redirected

opener = urllib.request.build_opener(SafeRedirect())
def get(url):
    with opener.open(urllib.request.Request(url, headers=headers), timeout=30) as response:
        return response.read()

runs = json.loads(get(base + '/actions/workflows/main.yml/runs?branch=master&event=push&status=success&per_page=1'))['workflow_runs']
if not runs:
    raise SystemExit('No successful master push workflow run is available yet.')
run = runs[0]
artifacts = json.loads(get(base + '/actions/runs/' + str(run['id']) + '/artifacts?per_page=100'))['artifacts']
root = pathlib.Path('reports').resolve()
for name in ('backend-reports', 'frontend-reports', 'coverage-sources'):
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
for pattern in ('**/coverage.cobertura.xml', '**/cobertura-coverage.xml'):
    reports = list(root.glob(pattern))
    if len(reports) != 1:
        raise SystemExit('Expected one coverage report for ' + pattern)
    tree = ET.parse(reports[0])
    sources = tree.getroot().find('sources')
    if sources is None:
        raise SystemExit('Coverage report has no sources.')
    for source in sources:
        source.text = '.'
    tree.write(reports[0], encoding='utf-8', xml_declaration=True)
print('Imported successful GitHub Actions run', run['id'], 'commit', run['head_sha'])
pathlib.Path('reports/run-id.txt').write_text(str(run['id']))
PY'''
                }
                }
                script {
                    def runId = readFile('reports/run-id.txt').trim()
                    def previous = currentBuild.previousSuccessfulBuild
                    env.REPORT_ALREADY_PUBLISHED = (!params.FORCE_PUBLISH && previous?.description == "GitHub Actions run ${runId}").toString()
                    currentBuild.description = "GitHub Actions run ${runId}"
                }
            }
        }
        stage('Publish reports') {
            when { expression { env.REPORT_ALREADY_PUBLISHED != 'true' } }
            steps {
                junit testResults: 'reports/**/junit.xml', allowEmptyResults: false
                recordCoverage id: 'backend-coverage', name: 'Backend coverage', tools: [[parser: 'COBERTURA', pattern: 'reports/**/coverage.cobertura.xml']], sourceCodeRetention: 'LAST_BUILD', sourceDirectories: [[path: 'reports/coverage-sources/CatalysticBackend']]
                recordCoverage id: 'frontend-coverage', name: 'Frontend coverage', tools: [[parser: 'COBERTURA', pattern: 'reports/**/cobertura-coverage.xml']], sourceCodeRetention: 'LAST_BUILD', sourceDirectories: [[path: 'reports/coverage-sources/catalysticfrontend']]
            }
        }
    }
}
