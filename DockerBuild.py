import subprocess
import sys
import os
import yaml
from DockerBuildSystem import TerminalTools, DockerComposeTools, VersionTools, DockerSwarmTools
from SwarmManagement import SwarmManager

AvailableCommands = [
    ['run', 'Run solution.'],
    ['build', 'Build solution.'],
    ['start-dev', 'Start development session by deploying service dependencies'],
    ['help', 'Print available argument commands.']
]

def BuildDocker(buildSelection):
    srcFolder = ['src', '..']
    serviceDependenciesComposeFiles = ['docker-compose.consul.cluster.yml']
    generalComposeFiles = [
        'docker-compose.yml'
    ]

    if buildSelection == 'run':
        os.chdir(srcFolder[0])
        DockerComposeTools.DockerComposeUp(generalComposeFiles, False)
        os.chdir(srcFolder[1])
    
    elif buildSelection == 'build':
        os.chdir(srcFolder[0])
        DockerComposeTools.DockerComposeBuild(generalComposeFiles)
        os.chdir(srcFolder[1])

    elif buildSelection == 'start-dev':
        os.chdir(srcFolder[0])
        DockerComposeTools.CreateLocalNetwork('backend_network')
        DockerComposeTools.DockerComposeUp(serviceDependenciesComposeFiles)
        os.chdir(srcFolder[1])

    elif buildSelection == 'help':
        TerminalTools.PrintAvailableCommands(AvailableCommands)

    else:
        print('Please provide a valid build argument: ')
        BuildDocker('help')

if __name__ == '__main__':
    buildSelections = sys.argv[1:]
    if len(buildSelections) == 0:
        buildSelections = ['no_argument']
    for buildSelection in buildSelections:
        BuildDocker(buildSelection)
