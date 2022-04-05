using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Entities.BackgroundSprite.Dtos
{
    public class BackgroundSpriteDto
    {
        public Guid BackgroundSpriteId { get; set; }
        public string Nombre { get; set; }
        public string DireccionImagen { get; set; }
    }
}