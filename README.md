# lighthouse

## Architecture
- **Lighthouse Runtime**
    - Examples: Windows CLI app, Windows Service, linux container
    - **Lighthouse Server** (*ILighthouseServiceContainer*)
        - **Configuration Provider** 
        - **Resources**
            - File System
            - Network Interfaces
        - **Lighthouse Service**
            - f
            - d
        - **Event Bus** (*IEventContext*)
            - d
        - **Service Discovery**
            - Local/Remote Services
        - **System Logging**
            - Provides debug logs for running processes
        - **Monitor**
            - A service which *monitors* the operations of the server application. Ensuring all services are running, handling errors, and surfacing metrics

## Tools
### lighthouse CLI
#### Building 
- Run: dotnet build -r win-x64 to get windows binaries

#### Usage
- lighthouse run app.json
- lighthouse deploy app.json
- lighthouse test app.json

### lighthouse Server
Represents a common implementation of a lighthouse service that will run many smaller processes. Using _lighthouse run_ will create a Lighthouse server which will load the app configuration data, hydrate a service, and then start the app. Ideally, an app shouldn't have to specifically create a lighthouse server. If another system wants to create a lighthouse app, it can do so any a separate process.

#### Startup
1) Monitor Starts
2) Load Configuration Providers
3) Load Service Repositories
4) Launch Services
    - Load service metadata
    - Inform Monitor of service
    - Launch the service
5) Monitor Services

### Service Repository
A provider of service descriptors. A service descriptor is enough information for a LighthouseServer to start the service, and manage it over time (e.g. versioning).