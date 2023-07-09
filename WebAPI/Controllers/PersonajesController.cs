using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Commands.Personaje;
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
        public async Task<ActionResult<Unit>> PostPersonaje([FromBody] CreatePersonajeCommand.CreatePersonajeCommandRequest data)
        {
            return await _mediator.Send(data);
        }
    }
}