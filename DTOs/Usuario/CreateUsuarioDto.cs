namespace CreaNovelNETCore.DTOs.Usuario
{
    public class CreateUsuarioDto
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
    }
}