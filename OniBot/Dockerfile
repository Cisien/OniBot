FROM microsoft/dotnet:2.1-sdk as build
WORKDIR /src
COPY . .
RUN dotnet publish OniBot/OniBot.csproj -r linux-x64 -o /app

FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT dotnet OniBot.dll --environment production
