using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonajesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PersonajesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Unit>> PostPersonaje([FromBody] Application.Entities.Personaje.Crear.Execute data)
        {
            return await _mediator.Send(data);
        }
    }
}