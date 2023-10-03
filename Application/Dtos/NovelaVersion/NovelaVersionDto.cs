using System;
using System.Collections.Generic;

namespace Application.Dtos.NovelaVersion;

public class NovelaVersionDto
{
    public Guid NovelaVersionId { get; set; }
    public string NumeroVersion { get; set; }
    public Guid NovelaId { get; set; }
}