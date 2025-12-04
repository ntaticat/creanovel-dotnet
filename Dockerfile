# =====================
# STAGE 0: Imagen SDK base (para cache, restore y build)
# =====================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /src

# Copiamos solo los csproj para aprovechar el cache
COPY Domain/*.csproj Domain/
COPY Application/*.csproj Application/
COPY Persistence/*.csproj Persistence/
COPY WebAPI/*.csproj WebAPI/

RUN dotnet restore WebAPI/WebAPI.csproj

# Copiamos todo el código
COPY . .
WORKDIR /src/WebAPI

# =====================
# STAGE 1: Imagen de desarrollo (dotnet watch, VS Code, hot reload)
# =====================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS dev

ARG UID=1000
ARG GID=1000

RUN groupadd -g $GID devgroup \
    && useradd -m -u $UID -g $GID devuser
USER devuser

ENV HOME=/home/devuser
ENV DOTNET_CLI_HOME=/home/devuser

USER devuser
WORKDIR /src

# =====================
# STAGE 2: Publicación (solo para producción)
# =====================
FROM builder AS publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# =====================
# STAGE 3: Runtime de producción
# =====================
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAPI.dll"]