# lighthouse

### lighthouse CLI
#### Building 
- Run: dotnet build -r win-x64 to get windows binaries

#### Usage
- lighthouse run app.json
- lighthouse deploy app.json
- lighthouse test app.json

### lighthouse Server
Represents a common implementation of a lighthouse service that will run many smaller processes. Using _lighthouse run_ will create a Lighthouse server which will load the app configuration data, hydrate a service, and then start the app. Ideally, an app shouldn't have to specifically create a lighthouse server. If another system wants to create a lighthouse app, it can do so any a separate process.