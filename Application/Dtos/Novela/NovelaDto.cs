using System;

namespace Application.Dtos.Novela;

public class NovelaDto
{
    public Guid NovelaId { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public bool Disponible { get; set; }
    public Guid? UsuarioCreadorId { get; set; }
}