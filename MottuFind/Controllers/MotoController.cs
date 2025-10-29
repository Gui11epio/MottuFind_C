using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MottuFind_C_.Domain.Entities;
using Sprint1_C_.Application.DTOs.Requests;
using Sprint1_C_.Application.DTOs.Response;
using Sprint1_C_.Application.Services;
using Sprint1_C_.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Sprint1_C_.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiversion}/[controller]")]
    [SwaggerTag("Gerencia operações relacionadas às motos.")]
    public class MotoController : ControllerBase
    {
        private readonly MotoService _motoService;

        public MotoController(MotoService motoService)
        {
            _motoService = motoService;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MotoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(
            Summary = "Obtém todas as motos",
            Description = "Retorna uma lista de todas as motos cadastradas."
        )]
        public async Task<IActionResult> GetAll()
        {
            var motos = await _motoService.ObterTodos();
            if (motos == null || !motos.Any()) return NoContent();
            return Ok(motos);
        }

        [Authorize]
        [HttpGet("placa")]
        [ProducesResponseType(typeof(Resource<MotoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Obtém uma moto por placa",
            Description = "Retorna os detalhes de uma moto específica com links HATEOAS para ações relacionadas."
        )]
        public async Task<IActionResult> GetByPlaca([FromQuery] string valor)
        {
            var moto = await _motoService.ObterPorPlaca(valor);
            if (moto == null)
                return NotFound();

            var resource = new Resource<MotoResponse>
            {
                Data = moto,
                Links =
        {
                    new Link { Href = Url.Action(nameof(GetByPlaca), new { valor }), Rel = "self", Method = "GET" },
                    new Link { Href = Url.Action(nameof(Update), new { valor }), Rel = "update", Method = "PUT" },
                    new Link { Href = Url.Action(nameof(Delete), new { valor }), Rel = "delete", Method = "DELETE" },
                    new Link { Href = Url.Action(nameof(GetAll)), Rel = "all", Method = "GET" }
        }
            };

            return Ok(resource);
        }

        [Authorize]
        [HttpGet("pagina")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Resource<PagedResult<MotoResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(
            Summary = "Obtém motos paginadas",
            Description = "Retorna uma lista paginada de motos com links HATEOAS para navegação entre páginas."
        )]
        public async Task<ActionResult<Resource<PagedResult<MotoResponse>>>> GetPaged(int numeroPag = 1, int tamanhoPag = 10)
        {
            var result = await _motoService.ObterPorPagina(numeroPag, tamanhoPag);

            var resource = new Resource<PagedResult<MotoResponse>>
            {
                Data = result,
                Links =
                {
                    new Link { Href = Url.Action(nameof(GetPaged), new { numeroPag, tamanhoPag }), Rel = "self", Method = "GET" }
                }
            };

            // adiciona links de próxima e anterior se aplicável
            if ((numeroPag * tamanhoPag) < result.Total)
                resource.Links.Add(new Link
                {
                    Href = Url.Action(nameof(GetPaged), new { numeroPag = numeroPag + 1, tamanhoPag }),
                    Rel = "next",
                    Method = "GET"
                });

            if (numeroPag > 1)
                resource.Links.Add(new Link
                {
                    Href = Url.Action(nameof(GetPaged), new { numeroPag = numeroPag - 1, tamanhoPag }),
                    Rel = "prev",
                    Method = "GET"
                });


            return Ok(resource);
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(MotoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Cria uma nova moto",
            Description = "Adiciona uma nova moto ao sistema."
        )]
        public async Task<IActionResult> Create([FromBody] MotoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var motoCriada = await _motoService.Criar(request);
            return CreatedAtAction(nameof(GetByPlaca), new { valor = motoCriada.Placa }, motoCriada);
        }

        [Authorize]
        [HttpPut("placa")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Atualiza uma moto existente",
            Description = "Atualiza os detalhes de uma moto específica."
        )]
        public async Task<IActionResult> Update(string placa, [FromBody] MotoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var atualizada = await _motoService.Atualizar(placa, request);
            if (!atualizada)
                return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("placa")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Remove uma moto",
            Description = "Remove uma moto específica do sistema."
        )]
        public async Task<IActionResult> Delete(string placa)
        {
            var removida = await _motoService.Remover(placa);
            if (!removida)
                return NotFound();

            return NoContent();
        }
    }
}
