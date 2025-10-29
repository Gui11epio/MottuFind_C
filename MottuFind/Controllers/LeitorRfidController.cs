using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MottuFind_C_.Application.Services;
using Sprint1_C_.Application.DTOs.Requests;
using Sprint1_C_.Application.DTOs.Response;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuFind.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeitorRfidController : ControllerBase
    {
        private readonly LeitorRfidService _leitorService;

        public LeitorRfidController(LeitorRfidService service)
        {
            _leitorService = service;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LeitorRfidResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(
            Summary = "Obtém todos os leitores RFID",
            Description = "Retorna uma lista de todos os leitores RFID cadastrados."
        )]
        public async Task<IActionResult> GetAll()
        {
            var leitors = await _leitorService.ObterTodos();
            if (leitors == null || !leitors.Any()) return NoContent();
            return Ok(leitors);
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LeitorRfidResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Obtém um leitor RFID por ID",
            Description = "Retorna os detalhes de um leitor RFID específico."
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var leitor = await _leitorService.ObterPorId(id);
            if (leitor == null) return NotFound();
            return Ok(leitor);
        }

        [Authorize]
        [HttpGet("pagina")]
        [ProducesResponseType(typeof(PagedResult<LeitorRfidResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(
            Summary = "Obtém leitores RFID paginados",
            Description = "Retorna uma lista paginada de leitores RFID."
        )]
        public async Task<ActionResult<PagedResult<LeitorRfidResponse>>> GetPaged(int numeroPag = 1, int tamanhoPag = 10)
        {
            var result = await _leitorService.ObterPorPagina(numeroPag, tamanhoPag);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(LeitorRfidResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Cria um novo leitor RFID",
            Description = "Adiciona um novo leitor RFID ao sistema."
        )]
        public async Task<IActionResult> Create([FromBody] LeitorRfidRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _leitorService.Criar(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Atualiza um leitor RFID existente",
            Description = "Atualiza os detalhes de um leitor RFID específico."
        )]
        public async Task<IActionResult> Update(int id, [FromBody] LeitorRfidRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _leitorService.Atualizar(id, request);
            if (!updated) return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Remove um leitor RFID",
            Description = "Remove um leitor RFID específico do sistema."
        )]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _leitorService.Remover(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
