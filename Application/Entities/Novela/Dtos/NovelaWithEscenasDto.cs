using System;
using System.Collections.Generic;
using Application.Entities.Background.Dtos;
using Application.Entities.Escena.Dtos;
using Application.Entities.Personaje.Dtos;

namespace Application.Entities.Novela.Dtos
{
    public class NovelaWithEscenasDto
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