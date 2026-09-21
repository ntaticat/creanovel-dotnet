# Crear migracion para actualizar base de datos
dotnet ef migrations add NOMBRE_MIGRACION --project WebAPI/ --output-dir Persistence/Migrations --namespace Persistence.Migrations

# Actualizar la base de datos después de migración
# !!! En desarrollo no necesitas ejecutar manualmente
# !!! este comando, Program.cs ya lo hace por ti.
dotnet ef database update --project WebAPI/

# User-Secrets solo para Development !
dotnet user-secrets init --project WebAPI
dotnet user-secrets list --project WebAPI
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dbName;Username=postgres;Password=YourSecureSecret" --project WebAPI
dotnet user-secrets set "JWTKey" "YourSecureSecret" --project WebAPI