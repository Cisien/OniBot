FROM mcr.microsoft.com/dotnet/core/sdk:5.0 as build
WORKDIR /src
COPY . .
RUN dotnet publish OniBot/OniBot.csproj -o /app -r linux-x64
#COPY OniBot/config /app/config/

FROM mcr.microsoft.com/dotnet/core/runtime:5.0
RUN apt-get update && apt-get install -y libsodium-dev libopus0 ffmpeg ca-certificates;
RUN ln -s /usr/lib/x86_64-linux-gnu/libopus.so.0 /usr/lib/x86_64-linux-gnu/opus.so
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT dotnet OniBot.dll --environment production
