using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Commands.NovelaVersion;
using Application.Dtos.NovelaVersion;
using Application.Queries.NovelaVersion;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NovelaVersionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NovelaVersionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<NovelaVersionDto>>> GetNovelas()
        {
        return await _mediator.Send(new GetNovelaVersionesQuery.GetNovelaVersionesQueryRequest());
        }

        [HttpPost]
        public async Task<ActionResult<Unit>> PostNovelaVersion([FromBody] CreateNovelaVersionCommand.CreateNovelaVersionCommandRequest data)
        {
        return await _mediator.Send(data);
        }
    }
}