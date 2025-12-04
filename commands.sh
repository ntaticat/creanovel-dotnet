# Crear migracion para actualizar base de datos
dotnet ef migrations add NOMBRE_MIGRACION -p Persistence/ -s WebAPI/

# Actualizar la base de datos después de migración
# !!! En desarrollo no necesitas ejecutar manualmente
# !!! este comando, Program.cs ya lo hace por ti.
dotnet ef database update --project WebAPI/

# Usar Docker compose para desarrollo
docker compose up -d

# User-Secrets solo para Development !
dotnet user-secrets init --project WebAPI
# Usar secrets para cadena de conexión a la db
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=dbName;User Id=sa;Password=YourSecureSecret" --project WebAPI
# Usar secrets para cadena de conexión a la db
dotnet user-secrets set "JWTKey" "YourSecureSecret" --project WebAPI
# ver user secrets
dotnet user-secrets list --project WebAPI