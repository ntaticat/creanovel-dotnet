using System;
using System.Collections.Generic;

namespace Domain.Models;

public class NovelaVersion
{
    public Guid NovelaVersionId { get; set; }
    public string NumeroVersion { get; set; }
    public bool Disponible { get; set; }
    public bool EsBorrador { get; set; }

    public Guid NovelaId { get; set; }
    public Novela Novela { get; set; }

    // Catálogo del motor de juego (variables, ...) como JSON. Vive en la versión para que cada
    // versión publicada sea autocontenida y el borrador la clone tal cual. Ver Application/Motor.
    public string Definiciones { get; set; } = "{\"variables\":[]}";

    public ICollection<Escena> Escenas { get; set; }
}