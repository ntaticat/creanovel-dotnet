# Crear migracion para actualizar cambios en Dominio
dotnet ef migrations add NOMBRE_MIGRACION -p Persistence/ -s WebAPI/

# Actualizar la base de datos
dotnet ef database update --project WebAPI/

# Crear imagen y ejecutar contenedor
docker build -f WebAPI/Dockerfile -t creanovel-api .   
docker run -d -p 5000:80 --name creanovel-api creanovel-api