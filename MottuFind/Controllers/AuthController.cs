using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using MottuFind_C_.Application.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace MottuFind.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiversion}/[controller]")]
    [SwaggerTag("Controlador de autenticação.")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [SwaggerOperation(Summary = "Autentica um usuário e retorna o token JWT")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.AutenticarAsync(request.Email, request.Senha);
            if (token == null)
                return Unauthorized(new { mensagem = "Credenciais inválidas" });

            return Ok(new { token });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}
