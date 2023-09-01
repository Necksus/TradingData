#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
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

#####################
#PUPPETEER RECIPE based on https://github.com/hardkoded/puppeteer-sharp/issues/1180#issuecomment-1015532968
#####################
RUN apt-get update && apt-get -f install && apt-get -y install wget gnupg2 apt-utils
RUN wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add -
RUN echo 'deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main' >> /etc/apt/sources.list
RUN apt-get update \
&& apt-get install -y google-chrome-stable --no-install-recommends --allow-downgrades fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf
######################
#END PUPPETEER RECIPE
######################
ENV PUPPETEER_EXECUTABLE_PATH "/usr/bin/google-chrome-stable"