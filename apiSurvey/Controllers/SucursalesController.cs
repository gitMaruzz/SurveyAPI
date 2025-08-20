using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace apiSurvey.Controllers
{
    [RoutePrefix("api/sucursales")]
    public class SucursalesController : ApiController
    {
        private readonly Logic.SucursalService _sucursalService;

        public SucursalesController()
        {
            _sucursalService = new Logic.SucursalService();
        }

        // GET /api/sucursales - Obtener todas las sucursales activas
        [HttpGet]
        [Route("")]
        [JwtAuthorization]
        public IHttpActionResult ObtenerSucursales()
        {
            try
            {
                var sucursales = _sucursalService.ObtenerSucursalesActivas();

                return Ok(new Logic.DataAPI.ApiResponse<List<Logic.DataAPI.SucursalDto>>
                {
                    Success = true,
                    Message = "Sucursales obtenidas exitosamente",
                    Data = sucursales
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET /api/sucursales/{id} - Obtener sucursal específica
        [HttpGet]
        [Route("{id:int}")]
        [JwtAuthorization]
        public IHttpActionResult ObtenerSucursal(int id)
        {
            try
            {
                var sucursal = _sucursalService.ObtenerSucursalPorId(id);

                if (sucursal == null)
                {
                    return NotFound();
                }

                return Ok(new Logic.DataAPI.ApiResponse<Logic.DataAPI.SucursalDto>
                {
                    Success = true,
                    Message = "Sucursal obtenida exitosamente",
                    Data = sucursal
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST /api/sucursales - Crear nueva sucursal (solo admin)
        [HttpPost]
        [Route("")]
        [JwtAuthorization("administrador")]
        public IHttpActionResult CrearSucursal(Logic.DataAPI.CrearSucursalRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var sucursalId = _sucursalService.CrearSucursal(request);

                return Ok(new Logic.DataAPI.ApiResponse<int>
                {
                    Success = true,
                    Message = "Sucursal creada exitosamente",
                    Data = sucursalId
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
                _sucursalService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
