﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MottuFind_C_.Application.Services;
using Sprint1_C_.Application.DTOs.Requests;
using Sprint1_C_.Application.DTOs.Response;
using Sprint1_C_.Application.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuFind.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeituraRfidController : ControllerBase
    {
        private readonly LeituraRfidService _leituraService;

        public LeituraRfidController(LeituraRfidService service)
        {
            _leituraService = service;
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LeituraRfidResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(
            Summary = "Obtém todas as leituras RFID",
            Description = "Retorna uma lista de todas as leituras RFID cadastradas."
        )]
        public async Task<IActionResult> GetAll()
        {
            var leituras = await _leituraService.ObterTodos();
            if (leituras == null || !leituras.Any()) return NoContent();
            return Ok(leituras);
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LeituraRfidResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Obtém uma leitura RFID por ID",
            Description = "Retorna os detalhes de uma leitura RFID específica."
        )]
        public async Task<IActionResult> GetById(int id)
        {
            var leitura = await _leituraService.ObterPorId(id);
            if (leitura == null) return NotFound();
            return Ok(leitura);
        }

        [Authorize]
        [HttpGet("pagina")]
        [ProducesResponseType(typeof(PagedResult<LeituraRfidResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(
            Summary = "Obtém leituras RFID paginadas",
            Description = "Retorna uma lista paginada de leituras RFID."
        )]
        public async Task<ActionResult<PagedResult<LeituraRfidResponse>>> GetPaged(int numeroPag = 1, int tamanhoPag = 10)
        {
            var result = await _leituraService.ObterPorPagina(numeroPag, tamanhoPag);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(LeituraRfidResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(
            Summary = "Cria uma nova leitura RFID",
            Description = "Adiciona uma nova leitura RFID ao sistema."
        )]
        public async Task<IActionResult> Create([FromBody] LeituraRfidRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _leituraService.Criar(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Atualiza uma leitura RFID existente",
            Description = "Atualiza os detalhes de uma leitura RFID específica."
        )]
        public async Task<IActionResult> Update(int id, [FromBody] LeituraRfidRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var updated = await _leituraService.Atualizar(id, request);
            if (!updated) return NotFound();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(
            Summary = "Remove uma leitura RFID",
            Description = "Remove uma leitura RFID específica com base no ID fornecido."
        )]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _leituraService.Remover(id);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
