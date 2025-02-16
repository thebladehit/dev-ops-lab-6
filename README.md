# Hive Emulator

- [Hive Emulator](#hive-emulator)
  - [About](#about)
  - [Installation](#installation)
    - [Redis](#redis)
    - [Map Component](#map-component)
    - [Communiction Control](#communiction-control)
    - [Hive Mind](#hive-mind)
  - [Usage](#usage)

## About
This is a demo project used in the Uni DevOps course

## Installation

### Redis
```bash
docker run --name redis -d -p 6379:6379 redis
```

### Map Component
```bash
cd src/MapClient

npm install

npm run dev
```

### Communiction Control
```bash
cd src/CommunicationControl

dotnet run  --project DevOpsProject/DevOpsProject.CommunicationControl.API.csproj
```

### Hive Mind
```bash
cd src/CommunicationControl

dotnet run  --project DevOpsProject/DevOpsProject.HiveMind.API.csproj
```


## Usage

1. Map Control is available at http://localhost:3000
2. Redis - Get available keys:
   ```bash
        docker exec -it redis redis-cli
        keys *
        get [hiveKey]
    ```

3. Communication Control Swagger: http://localhost:8080