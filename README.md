# Docker installation

Create a docker-compose.yml file (change the volume and port as needed):

```
version: '3.8'

services:
    sharpitpm:
        container_name: sharpitpm_live
        image: "necksus/sharpitpm:latest"
        volumes:
          - /opt/data:/data"
        ports:
          - "1337:80"
```

Execute the command `docker-compose up -d` to create the container.

Navigate to [http://hostname:1337/swagger](http://hostname:1337/swagger).
