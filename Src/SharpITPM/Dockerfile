#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG DOTNET_VERSION

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Ajout de l'instruction VOLUME
VOLUME /data

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SharpITPM/SharpITPM.csproj", "SharpITPM/"]
RUN dotnet restore "SharpITPM/SharpITPM.csproj"
COPY . .
WORKDIR "/src/SharpITPM"
RUN dotnet build "SharpITPM.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SharpITPM.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SharpITPM.dll"]

# Inspired from https://github.com/hardkoded/puppeteer-sharp/issues/1180#issuecomment-1015532968
RUN  apt update && apt install -y chromium
ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/chromium"