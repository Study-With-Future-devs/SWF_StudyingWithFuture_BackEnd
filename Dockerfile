# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar a solução
COPY *.sln ./

# Copiar a pasta do projeto inteiro
COPY Studying-With-Future/ ./Studying-With-Future/

# Restaurar dependências usando o caminho correto do projeto
RUN dotnet restore ./Studying-With-Future/Studying-With-Future.csproj

# Build/publish
WORKDIR /src/Studying-With-Future
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5201
ENTRYPOINT ["dotnet", "Studying-With-Future.dll"]