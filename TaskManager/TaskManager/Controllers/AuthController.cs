using Microsoft.AspNetCore.Mvc;
using ApplicationLayer.Services;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public class LoginRequest
        {
            public string Username { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Usuario hardcodeado para pruebas
            if (request.Username == "admin" && request.Password == "admin")
            {
                var token = _jwtService.GenerateToken(request.Username);
                return Ok(new { Token = token });
            }

            return Unauthorized("Usuario o contraseña incorrecta");
        }

       
        [HttpGet("secret")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult Secret()
        {
            return Ok("¡Felicidades! Estás autenticado con JWT.");
        }
    }
}
