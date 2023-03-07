using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dtos.NovelaBackground
{
    public record NovelaBackgroundDto
    {
        public Guid NovelaId { get; set; }
        public Guid BackgroundId { get; set; }
    }
}