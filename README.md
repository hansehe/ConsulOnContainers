# ConsulOnContainers

## Get Started
1. Install Docker
2. Install python and pip
    - Windows:  https://matthewhorne.me/how-to-install-python-and-pip-on-windows-10/
    - Ubuntu: Python is installed by default
        - Install pip: sudo apt-get install python-pip
3. Install python dependencies:
    - -> pip install -r requirements.txt
4. See available commands:
    - -> python DockerBuild.py help

## Build & Run
1. Start domain development by deploying service dependencies:
    - python DockerBuild.py start-dev
2. Open a new cmd window and build solution as container images:
    - python DockerBuild.py build
3. Run solution as containers:
    - python DockerBuild.py run
4. Open browser and login to consul to see results!
    - Consul: http://localhost:8500