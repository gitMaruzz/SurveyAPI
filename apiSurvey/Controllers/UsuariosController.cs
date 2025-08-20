using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace apiSurvey.Controllers
{
    [RoutePrefix("api/usuarios")]
    public class UsuariosController : ApiController
    {
        private readonly Logic.Services.UsuarioService _usuarioService;

        public UsuariosController()
        {
            _usuarioService = new Logic.Services.UsuarioService();
        }

        [HttpGet]
        [Route("")]
        [JwtAuthorization("administrador")]
        public IHttpActionResult ObtenerUsuarios()
        {
            try
            {
                var usuarios = _usuarioService.ObtenerUsuarios();

                return Ok(new Logic.DataAPI.ApiResponse<List<Logic.DataAPI.UsuarioDto>>
                {
                    Success = true,
                    Message = "Usuarios obtenidos exitosamente",
                    Data = usuarios
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
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
