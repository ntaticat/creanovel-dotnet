FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
#
COPY *.sln .
COPY Domain/*.csproj ./Domain/
COPY Persistence/*.csproj ./Persistence/
COPY Application/*.csproj ./Application/
COPY WebAPI/*.csproj ./WebAPI/
COPY Tests/*.csproj ./Tests/
#
RUN dotnet restore
#
COPY Domain/. ./Domain/
COPY Persistence/. ./Persistence/
COPY Application/. ./Application/
COPY WebAPI/. ./WebAPI/
#
WORKDIR /app/WebAPI
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
#
COPY --from=build /app/WebAPI/out ./
ENTRYPOINT [ "dotnet", "WebAPI.dll" ]