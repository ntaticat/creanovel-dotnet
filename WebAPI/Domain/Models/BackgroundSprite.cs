using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class BackgroundSprite
    {
        public Guid BackgroundSpriteId { get; set; }
        public string Nombre { get; set; }
        public string DireccionImagen { get; set; }
        public Guid BackgroundId { get; set; }
        public Background Background { get; set; }
    }
}