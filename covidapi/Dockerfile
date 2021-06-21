#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build-env
# Use native linux file polling for better performance
ENV DOTNET_USE_POLLING_FILE_WATCHER 1
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENTRYPOINT dotnet watch run --urls=http://+:5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["covidapi.csproj", "covidapi/"]
RUN dotnet restore "covidapi/covidapi.csproj"
COPY . .
WORKDIR "/src/covidapi"
RUN dotnet build "covidapi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "covidapi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "covidapi.dll"]