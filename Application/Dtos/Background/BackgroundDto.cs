using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos.BackgroundSprite;

namespace Application.Dtos.Background
{
    public record BackgroundDto
    {
        public Guid BackgroundId { get; set; }
        public string Descripcion { get; set; }
        public ICollection<BackgroundSpriteDto> Sprites { get; set; }
    }
}