using System;
using System.Collections.Generic;
using Application.Dtos.Background;
using Application.Dtos.Escena;
using Application.Dtos.Personaje;

namespace Application.Dtos.Novela
{
    public record NovelaWithEscenasDto
    {
        public Guid NovelaId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Disponible { get; set; }
        public Guid? UsuarioCreadorId { get; set; }
        public ICollection<EscenaDto> Escenas { get; set; }
        public ICollection<PersonajeDto> Personajes { get; set; }
        public ICollection<BackgroundDto> Backgrounds { get; set; }
    }
}