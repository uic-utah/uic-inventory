FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /project

COPY . ./
WORKDIR /project/src/api
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app -r linux-x64
# -p:PublishSingleFile=true --self-contained false -p:DebugType=embedded -p:PublishReadyToRun=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
# RUN apt update -y && apt install -y libc-dev
ENTRYPOINT ["dotnet", "api.dll"]
