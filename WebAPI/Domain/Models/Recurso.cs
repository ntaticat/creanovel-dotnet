using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Recurso
    {
        public Guid RecursoId { get; set; }

        public Guid EscenaId { get; set; }
        public Escena Escena { get; set; }

        public bool PrimerRecurso { get; set; }
        public bool UltimoRecurso { get; set; }
        public string TipoRecurso { get; set; }

        // Los personajes que aparecen en el escenario de este nodo (jsonb): lista de `PersonajeEnEscena` en orden de apilado.
        // Guarda ids de PersonajeSprite sin FK; al borrar un sprite se limpian (ver DeletePersonajeSprite).
        public string Personajes { get; set; } = "[]";

        public Guid? BackgroundSpriteId { get; set; }
        public BackgroundSprite BackgroundSprite { get; set; }

        public Guid? SiguienteRecursoId { get; set; }
        public Recurso SiguienteRecurso { get; set; }

        public string Contenido { get; set; } = "{}";

        public ICollection<RecursoDecisionOpcion> Opciones { get; set; }

        public ICollection<LecturaRecursos> Lecturas { get; set; }
    }
}
