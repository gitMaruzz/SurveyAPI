using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace apiSurvey.Controllers
{
    [RoutePrefix("api/reportes")]
    public class ReportesController : ApiController
    {
        private readonly Logic.ReporteService _reporteService;

        public ReportesController()
        {
            _reporteService = new Logic.ReporteService();
        }

        // =====================================================
        // 1. ESTADÍSTICAS DE UNA ENCUESTA ESPECÍFICA
        // =====================================================
        // GET /api/reportes/encuesta/{id}
        // Solo administradores pueden ver estadísticas completas

        [HttpGet]
        [Route("encuesta/{id:int}")]
        [JwtAuthorization("administrador")]
        public IHttpActionResult ObtenerEstadisticasEncuesta(int id)
        {
            try
            {
                var estadisticas = _reporteService.ObtenerEstadisticasEncuesta(id);

                if (estadisticas == null)
                {
                    return NotFound();
                }

                return Ok(new Logic.DataAPI.ApiResponse<object>
                {
                    Success = true,
                    Message = "Estadísticas obtenidas exitosamente",
                    Data = estadisticas
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // =====================================================
        // 2. MIS RESPUESTAS (Del usuario actual)
        // =====================================================
        // GET /api/reportes/mis-respuestas
        // Cualquier usuario autenticado puede ver sus propias respuestas

        [HttpGet]
        [Route("mis-respuestas")]
        [JwtAuthorization]
        public IHttpActionResult ObtenerMisRespuestas()
        {
            try
            {
                var claimsPrincipal = User as ClaimsPrincipal;
                var usuarioId = Logic.JwtDebugHelper.GetUserIdSafe(claimsPrincipal);

                if (!usuarioId.HasValue)
                {
                    return BadRequest("Token inválido: no se pudo obtener ID de usuario");
                }

                var respuestas = _reporteService.ObtenerRespuestasUsuario(usuarioId.Value);

                return Ok(new Logic.DataAPI.ApiResponse<List<object>>
                {
                    Success = true,
                    Message = "Respuestas obtenidas exitosamente",
                    Data = respuestas
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // =====================================================
        // ENDPOINT PARA CONSULTAR POR NÚMERO DE SOLICITUD
        // =====================================================

        [HttpGet]
        [Route("solicitud/{numeroSolicitud}")]
        [JwtAuthorization]
        public IHttpActionResult ObtenerRespuestaPorSolicitud(string numeroSolicitud)
        {
            try
            {
                var respuesta = _reporteService.ObtenerRespuestaPorSolicitud(numeroSolicitud);

                if (respuesta == null)
                {
                    return NotFound();
                }

                return Ok(new Logic.DataAPI.ApiResponse<object>
                {
                    Success = true,
                    Message = "Respuesta obtenida exitosamente",
                    Data = respuesta
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
