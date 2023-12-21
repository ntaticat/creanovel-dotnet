using System;
using System.Collections.Generic;

namespace Domain.Models;

public class NovelaVersion
{
    public Guid NovelaVersionId { get; set; }
    public string NumeroVersion { get; set; }
    public bool Disponible { get; set; }
    
    public Guid NovelaId { get; set; }
    public Novela Novela { get; set; }
    
    public ICollection<Escena> Escenas { get; set; }
}