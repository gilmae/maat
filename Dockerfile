FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY Events/Events.csproj ./Events/
COPY StrangeVanilla.Blogging.Events/StrangeVanilla.Blogging.Events.csproj ./StrangeVanilla.Blogging.Events/
COPY Migrations/Migrations.csproj ./Migrations/
COPY Users/Users.csproj ./Users/
COPY SimpleRepo/SimpleRepo.csproj ./SimpleRepo/
COPY SV.Maat/SV.Maat.csproj ./SV.Maat/
RUN dotnet restore

# copy everything else and build app
COPY Events/. ./Events/
COPY StrangeVanilla.Blogging.Events/. ./StrangeVanilla.Blogging.Events/
COPY Migrations/. ./Migrations/
COPY Users/. ./Users/
COPY SimpleRepo/. ./SimpleRepo/
COPY SV.Maat/. ./SV.Maat/
WORKDIR /app
RUN dotnet publish SV.Maat -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "SV.Maat.dll"]