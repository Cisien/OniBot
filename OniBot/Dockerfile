FROM microsoft/dotnet:1.1.2-runtime-nanoserver
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "OniBot.dll", "-environment", "production"]
