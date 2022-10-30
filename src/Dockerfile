FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY **/*.csproj ./
COPY **/*.db ./
RUN dotnet restore Moneyman.Api.csproj

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out Moneyman.Api

# Build runtime image
EXPOSE 80
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Moneyman.Api.dll"]
