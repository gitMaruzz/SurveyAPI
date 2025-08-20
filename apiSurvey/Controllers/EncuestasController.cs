using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.IdentityModel.Tokens.Jwt;
using apiSurvey.Logic;

namespace apiSurvey.Controllers
{
    [RoutePrefix("api/encuestas")]
    public class EncuestasController : ApiController
    {
        private readonly Logic.Services.EncuestaService _encuestaService;

        public EncuestasController()
        {
            _encuestaService = new Logic.Services.EncuestaService();
        }

        [HttpGet]
        [Route("")]
        [JwtAuthorization]
        public IHttpActionResult ObtenerEncuestas()
        {
            try
            {
                var encuestas = _encuestaService.ObtenerEncuestasActivas();

                return Ok(new Logic.DataAPI.ApiResponse<List<Logic.DataAPI.EncuestaDto>>
                {
                    Success = true,
                    Message = "Encuestas obtenidas exitosamente",
                    Data = encuestas
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("{id:int}")]
        [JwtAuthorization]
        public IHttpActionResult ObtenerEncuesta(int id)
        {
            try
            {
                var encuesta = _encuestaService.ObtenerEncuestaCompleta(id);

                if (encuesta == null)
                {
                    return NotFound();
                }

                return Ok(new Logic.DataAPI.ApiResponse<Logic.DataAPI.EncuestaDto>
                {
                    Success = true,
                    Message = "Encuesta obtenida exitosamente",
                    Data = encuesta
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("")]
        [JwtAuthorization("administrador")]
        public IHttpActionResult CrearEncuesta(Logic.DataAPI.CrearEncuestaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Obtener el ClaimsPrincipal y buscar el claim
                var claimsPrincipal = User as ClaimsPrincipal;
                if (claimsPrincipal == null)
                {
                    return BadRequest("Token inválido: no se pudo obtener información del usuario");
                }

                //var userIdClaim = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub);
                //if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                //{
                //    return BadRequest("Token inválido: no se pudo obtener ID de usuario");
                //}

                var userIdClaim = JwtDebugHelper.GetUserIdSafe(claimsPrincipal);
                if (!userIdClaim.HasValue)
                {
                    return BadRequest("No se pudo obtener ID de usuario");
                }
                var encuestaId = _encuestaService.CrearEncuesta(request, userIdClaim.Value);
                return Ok(new Logic.DataAPI.ApiResponse<int>
                {
                    Success = true,
                    Message = "Encuesta creada exitosamente",
                    Data = encuestaId
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("{id:int}/responder")]
        [JwtAuthorization("asesor")]
        public IHttpActionResult ResponderEncuesta(int id, Logic.DataAPI.ResponderEncuestaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Obtener el ClaimsPrincipal y buscar el claim
                var claimsPrincipal = User as ClaimsPrincipal;
                if (claimsPrincipal == null)
                {
                    return BadRequest("Token inválido: no se pudo obtener información del usuario");
                }

                //var userIdClaim = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub);
                //if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
                //{
                //    return BadRequest("Token inválido: no se pudo obtener ID de usuario");
                //}

                var userIdClaim = JwtDebugHelper.GetUserIdSafe(claimsPrincipal);
                if (!userIdClaim.HasValue)
                {
                    return BadRequest("No se pudo obtener ID de usuario");
                }


                //if (!int.TryParse(userIdClaim.Value, out int usuarioId))
                //{
                //    return BadRequest("Token inválido: ID de usuario no válido");
                //}

                // Verificar si ya respondió
                //if (_encuestaService.UsuarioYaRespondio(id, userIdClaim.Value))
                //{
                //    return BadRequest("Ya ha respondido esta encuesta");
                //}

                var resultado = _encuestaService.ResponderEncuesta(id, userIdClaim.Value, request);

                if (!resultado)
                {
                    return BadRequest("Error al guardar la respuesta");
                }

                return Ok(new Logic.DataAPI.ApiResponse<bool>
                {
                    Success = true,
                    Message = "Respuesta guardada exitosamente",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // ENDPOINT TEMPORAL PARA DEBUG
        //[HttpGet]
        //[Route("debug-token")]
        //[JwtAuthorization]
        //public IHttpActionResult DebugToken()
        //{
        //    try
        //    {
        //        var claimsPrincipal = User as ClaimsPrincipal;

        //        if (claimsPrincipal == null)
        //        {
        //            return Ok(new
        //            {
        //                message = "No se pudo obtener ClaimsPrincipal",
        //                userType = User?.GetType()?.Name
        //            });
        //        }

        //        // Debuggear claims
        //        JwtDebugHelper.DebugClaims(claimsPrincipal);

        //        // Obtener información del usuario
        //        var userId = JwtDebugHelper.GetUserIdSafe(claimsPrincipal);
        //        var userRole = JwtDebugHelper.GetUserRoleSafe(claimsPrincipal);

        //        var allClaims = claimsPrincipal.Claims.Select(c => new {
        //            Type = c.Type,
        //            Value = c.Value,
        //            ValueType = c.ValueType
        //        }).ToList();

        //        return Ok(new
        //        {
        //            message = "Debug info del token",
        //            userId = userId,
        //            userRole = userRole,
        //            totalClaims = allClaims.Count,
        //            claims = allClaims
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return InternalServerError(ex);
        //    }
        //}

        // MÉTODO 5: Debug endpoint (GET /api/encuestas/debug)
        [HttpGet]
        [Route("debug")]
        [JwtAuthorization]
        public IHttpActionResult Debug()
        {
            try
            {
                return Ok(new
                {
                    message = "Controlador Encuestas funcionando",
                    timestamp = DateTime.Now,
                    user = User?.Identity?.Name ?? "No identificado"
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
                _encuestaService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
