using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Entities.BackgroundSprite.Dtos;

namespace Application.Entities.Background.Dtos
{
    public class BackgroundDto
    {
        public Guid BackgroundId { get; set; }
        public string Descripcion { get; set; }
        public ICollection<BackgroundSpriteDto> Sprites { get; set; }
    }
}