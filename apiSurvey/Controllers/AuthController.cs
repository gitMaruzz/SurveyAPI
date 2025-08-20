using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace apiSurvey.Controllers
{
    public class AuthController : ApiController
    {
        private readonly Logic.Services.UsuarioService _usuarioService;
        private readonly Logic.TokenGenerator _jwtService;


        public AuthController()
        {
            _usuarioService = new Logic.Services.UsuarioService();
            _jwtService = new Logic.TokenGenerator();
        }
        // GET: api/Auth

        [HttpPost]
        [Route("api/login")]
        public IHttpActionResult Login(Logic.DataAPI.LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuario = _usuarioService.ValidarUsuarioEF(request.Email, request.Password);
                //Models.Model.Usuario usuario = new Models.Model.Usuario();

                if (usuario == null)
                {
                    return BadRequest("Credenciales inválidas");
                }

                var token = _jwtService.GenerateTokenJwt(usuario);
                var expiracion = DateTime.UtcNow.AddHours(24);

                var response = new Logic.DataAPI.LoginResponse
                {
                    Token = token,
                    TipoUsuario = usuario.TipoUsuario,
                    Nombre = usuario.Nombre,
                    ID = usuario.Id,
                    Expiracion = expiracion
                };

                //var response = new Logic.DataAPI.LoginResponse
                //{
                //    Token = "toekn",
                //    TipoUsuario = "tipo",
                //    Nombre = "nombre",
                //    Expiracion = expiracion
                //};

                return Ok(new Logic.DataAPI.ApiResponse<Logic.DataAPI.LoginResponse>
                {
                    Success = true,
                    Message = "Login exitoso",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/register")]
        [JwtAuthorization("administrador")]
        public IHttpActionResult Register(Logic.DataAPI.RegistrarUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var usuario = _usuarioService.CrearUsuario(
                    request.Nombre,
                    request.Email,
                    request.Password,
                    request.TipoUsuario
                );

                return Ok(new Logic.DataAPI.ApiResponse<Logic.DataAPI.UsuarioDto>
                {
                    Success = true,
                    Message = "Usuario creado exitosamente",
                    Data = new Logic.DataAPI.UsuarioDto
                    {
                        Id = usuario.Id,
                        Nombre = usuario.Nombre,
                        Email = usuario.Email,
                        TipoUsuario = usuario.TipoUsuario,
                        FechaCreacion = usuario.FechaCreacion,
                        Activo = usuario.Activo
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // Endpoint para verificar que la API esté funcionando
        [HttpGet]
        [Route("health")]
        public IHttpActionResult Health()
        {
            try
            {
                return Ok(new
                {
                    status = "OK",
                    timestamp = DateTime.UtcNow,
                    server = Environment.MachineName,
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // Endpoint para verificar base de datos
        [HttpGet]
        [Route("db-check")]
        public IHttpActionResult DatabaseCheck()
        {
            try
            {
                _usuarioService.VerificarBaseDatos();
                return Ok(new { status = "BD verificada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error en BD: {ex.Message}");
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _usuarioService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
