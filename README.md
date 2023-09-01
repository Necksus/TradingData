# Docker installation

1) Create a /opt/data folder, then download content from https://github.com/Necksus/TradingData/tree/main/Data inside
2) Create a docker-compose.yml file (change the volume and port as needed):

```
version: '3.8'

services:
    sharpitpm:
        container_name: sharpitpm_live
        image: "necksus/sharpitpm:latest"
        volumes:
          - "/opt/data:/data"
        ports:
          - "1337:80"
```

**Important:** use necksus/sharpitpm.arm for arm host (such as Raspberry Pi)

3) Execute the command `docker-compose up -d` to create the container.
4) Navigate to [http://hostname:1337/swagger](http://hostname:1337/swagger).
