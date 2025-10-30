FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base-env

WORKDIR /app
EXPOSE 8080



FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

WORKDIR /src

COPY ./src/*.csproj .
RUN dotnet restore ./*.csproj


COPY ./src/ .
RUN dotnet build ./*.csproj -c Release -o /publish
RUN dotnet publish ./*.csproj -c Release -o /publish /p:UseAppHost=false


FROM base-env AS run-env
COPY --from=build-env /publish .
ENTRYPOINT ["dotnet", "Demo.dll"]
