# Crear migracion para actualizar cambios en Dominio
dotnet ef migrations add NOMBRE_MIGRACION -p Persistence/ -s WebAPI/

# Actualizar la base de datos
dotnet ef database update --project WebAPI/