
# Overview
FileSync is an application, that allows you to define storage policy for a local file system
# Installation Steps
## Install Lighthouse Server
`run lighthouse-install.ps1 C:\lighthouse `
#### This will install the server as a windows service, into the provided directory. It will also register

## Install FileSync app
`lighthouse install filesync`

Note: No server installation is necessary, as storage servers will expose warehouses directly.

## Inform FileSync of the locations it should track
`lighthouse configure app:filesync config_file:file_sync_config.yaml`

### Content of the file_sync_config.yaml file
``` yaml
    autorun: true
    type: app
    
    folders:
        - folder: 'C:\Development'
        - folder: 'C:\Photos'
```
lighthouse run filesync