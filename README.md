# ConsulOnContainers

## Introduction
ConsulOnContainers is a concept library around [Consul](https://www.consul.io/), and it shows how to use Consul as a service registration orchestrator.  
In summary, each service registers with Consul during bootup with information on which IP address to locate the service, and how to do health checks on the respective service.

The solution includes an API service which registers itself with Consul, and a client service which locates the API service using Consul.
- [APIService](/src/Services/APIService/)
- [ClientService](/src/Services/ClientService/)

## Get Started
1. Install Docker:
   - https://www.docker.com/ 

## Build & Run
```bash
# Build dotnet core api and client service
docker-compose -f src/docker-compose.yml build

# Deploy consul cluster and dotnet core services
docker-compose -f src/docker-compose.consul.cluster.yml -f src/docker-compose.yml up

# Open browser and login to consul to see results!
# - Consul: http://localhost:8500
```

## Nice To Know
Consul is deployed in a cluster of Consul services, and there should only be one Consul service on each node in the cluster, thus it is not possible to run this concept in Swarm mode. Each Consul service will be in conflict if they are running on a single node if the cluster is running in Swarm mode. The concept is therefore deployed with bridge networks running on a single node.
- Wanna know more about consul? 
    - https://www.consul.io/docs/internals/architecture.html
