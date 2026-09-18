# ============================================================================
# Imagem da API do Nimbus ERP. O contexto de build e a RAIZ do repositorio
# (ver docker-compose.yml), porque o build depende dos arquivos de
# Central Package Management na raiz.
# ============================================================================

# --- Estagio 1: restore + publish -------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /origem

# Copia primeiro apenas os manifestos: assim o cache do restore so e
# invalidado quando uma dependencia muda, nao a cada alteracao de codigo.
COPY Directory.Build.props Directory.Packages.props ./
COPY src/Nimbus.Domain/Nimbus.Domain.csproj src/Nimbus.Domain/
COPY src/Nimbus.Application/Nimbus.Application.csproj src/Nimbus.Application/
COPY src/Nimbus.Infrastructure/Nimbus.Infrastructure.csproj src/Nimbus.Infrastructure/
COPY src/Nimbus.Api/Nimbus.Api.csproj src/Nimbus.Api/

RUN dotnet restore src/Nimbus.Api/Nimbus.Api.csproj

# O .editorconfig e necessario no build: o projeto compila com
# TreatWarningsAsErrors e as regras de estilo vivem nele.
COPY .editorconfig ./
COPY src/ src/

RUN dotnet publish src/Nimbus.Api/Nimbus.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publicado

# --- Estagio 2: runtime -----------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# A imagem aspnet e baseada em Debian slim e nao traz curl; o healthcheck do
# Docker Compose depende dele para consultar /health/pronto.
RUN apt-get update \
 && apt-get install --yes --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# Usuario sem privilegios (a imagem aspnet ja traz o usuario "app").
USER app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_gcServer=1

EXPOSE 8080

COPY --from=build /app/publicado ./

ENTRYPOINT ["dotnet", "Nimbus.Api.dll"]
