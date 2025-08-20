using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static apiSurvey.Models.Data;

namespace apiSurvey.Logic
{
    public class SucursalService 
    {
        private readonly EncuestasContext _context;

        public SucursalService()
        {
            _context = new EncuestasContext();
        }

        // =====================================================
        // OBTENER TODAS LAS SUCURSALES ACTIVAS
        // =====================================================
        public List<DataAPI.SucursalDto> ObtenerSucursalesActivas()
        {
            try
            {
                var sucursales = _context.Sucursales
                    .Where(s => s.Activa)
                    .OrderBy(s => s.Nombre)
                    .ToList();

                return sucursales.Select(s => new DataAPI.SucursalDto
                {
                    Id = s.Id,
                    CodigoSucursal = s.CodigoSucursal,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Estado = s.Estado,
                    Telefono = s.Telefono,
                    Email = s.Email,
                    Gerente = s.Gerente,
                    Activa = s.Activa
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo sucursales: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // OBTENER SUCURSAL POR ID
        // =====================================================
        public DataAPI.SucursalDto ObtenerSucursalPorId(int id)
        {
            try
            {
                var sucursal = _context.Sucursales
                    .FirstOrDefault(s => s.Id == id);

                if (sucursal == null)
                {
                    return null;
                }

                return new DataAPI.SucursalDto
                {
                    Id = sucursal.Id,
                    CodigoSucursal = sucursal.CodigoSucursal,
                    Nombre = sucursal.Nombre,
                    Direccion = sucursal.Direccion,
                    Ciudad = sucursal.Ciudad,
                    Estado = sucursal.Estado,
                    Telefono = sucursal.Telefono,
                    Email = sucursal.Email,
                    Gerente = sucursal.Gerente,
                    Activa = sucursal.Activa
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo sucursal {id}: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // OBTENER SUCURSAL POR CÓDIGO
        // =====================================================
        public DataAPI.SucursalDto ObtenerSucursalPorCodigo(string codigo)
        {
            try
            {
                var sucursal = _context.Sucursales
                    .FirstOrDefault(s => s.CodigoSucursal == codigo && s.Activa);

                if (sucursal == null)
                {
                    return null;
                }

                return new DataAPI.SucursalDto
                {
                    Id = sucursal.Id,
                    CodigoSucursal = sucursal.CodigoSucursal,
                    Nombre = sucursal.Nombre,
                    Direccion = sucursal.Direccion,
                    Ciudad = sucursal.Ciudad,
                    Estado = sucursal.Estado,
                    Telefono = sucursal.Telefono,
                    Email = sucursal.Email,
                    Gerente = sucursal.Gerente,
                    Activa = sucursal.Activa
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo sucursal por código {codigo}: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // CREAR NUEVA SUCURSAL
        // =====================================================
        public int CrearSucursal(DataAPI.CrearSucursalRequest request)
        {
            try
            {
                // Verificar que el código no exista
                var existe = _context.Sucursales
                    .Any(s => s.CodigoSucursal == request.CodigoSucursal);

                if (existe)
                {
                    throw new InvalidOperationException($"Ya existe una sucursal con el código '{request.CodigoSucursal}'");
                }

                var sucursal = new Models.Model.Sucursal
                {
                    CodigoSucursal = request.CodigoSucursal.Trim().ToUpper(),
                    Nombre = request.Nombre.Trim(),
                    Direccion = request.Direccion?.Trim(),
                    Ciudad = request.Ciudad?.Trim(),
                    Estado = request.Estado?.Trim(),
                    Telefono = request.Telefono?.Trim(),
                    Email = request.Email?.Trim(),
                    Gerente = request.Gerente?.Trim(),
                    Activa = true,
                    FechaCreacion = DateTime.Now
                };

                _context.Sucursales.Add(sucursal);
                _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"Sucursal creada: {sucursal.Nombre} ({sucursal.CodigoSucursal})");

                return sucursal.Id;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creando sucursal: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // ACTUALIZAR SUCURSAL
        // =====================================================
        public bool ActualizarSucursal(int id, DataAPI.ActualizarSucursalRequest request)
        {
            try
            {
                var sucursal = _context.Sucursales.FirstOrDefault(s => s.Id == id);

                if (sucursal == null)
                {
                    return false;
                }

                // Verificar código único (si cambió)
                if (sucursal.CodigoSucursal != request.CodigoSucursal)
                {
                    var existeOtroCodigo = _context.Sucursales
                        .Any(s => s.CodigoSucursal == request.CodigoSucursal && s.Id != id);

                    if (existeOtroCodigo)
                    {
                        throw new InvalidOperationException($"Ya existe otra sucursal con el código '{request.CodigoSucursal}'");
                    }
                }

                sucursal.CodigoSucursal = request.CodigoSucursal.Trim().ToUpper();
                sucursal.Nombre = request.Nombre.Trim();
                sucursal.Direccion = request.Direccion?.Trim();
                sucursal.Ciudad = request.Ciudad?.Trim();
                sucursal.Estado = request.Estado?.Trim();
                sucursal.Telefono = request.Telefono?.Trim();
                sucursal.Email = request.Email?.Trim();
                sucursal.Gerente = request.Gerente?.Trim();

                _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"Sucursal actualizada: {sucursal.Nombre}");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando sucursal {id}: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // ACTIVAR/DESACTIVAR SUCURSAL
        // =====================================================
        public bool CambiarEstadoSucursal(int id, bool activa)
        {
            try
            {
                var sucursal = _context.Sucursales.FirstOrDefault(s => s.Id == id);

                if (sucursal == null)
                {
                    return false;
                }

                sucursal.Activa = activa;
                _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"Sucursal {sucursal.Nombre} {(activa ? "activada" : "desactivada")}");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cambiando estado de sucursal {id}: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // OBTENER SUCURSALES POR CIUDAD
        // =====================================================
        public List<DataAPI.SucursalDto> ObtenerSucursalesPorCiudad(string ciudad)
        {
            try
            {
                var sucursales = _context.Sucursales
                    .Where(s => s.Activa && s.Ciudad.Contains(ciudad))
                    .OrderBy(s => s.Nombre)
                    .ToList();

                return sucursales.Select(s => new DataAPI.SucursalDto
                {
                    Id = s.Id,
                    CodigoSucursal = s.CodigoSucursal,
                    Nombre = s.Nombre,
                    Direccion = s.Direccion,
                    Ciudad = s.Ciudad,
                    Estado = s.Estado,
                    Telefono = s.Telefono,
                    Email = s.Email,
                    Gerente = s.Gerente,
                    Activa = s.Activa
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo sucursales por ciudad {ciudad}: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // OBTENER ESTADÍSTICAS DE SUCURSAL
        // =====================================================
        public object ObtenerEstadisticasSucursal(int sucursalId)
        {
            try
            {
                var sucursal = _context.Sucursales.FirstOrDefault(s => s.Id == sucursalId);

                if (sucursal == null)
                {
                    return null;
                }

                var totalRespuestas = _context.RespuestasEncuesta
                    .Count(r => r.SucursalId == sucursalId && r.Estado == "completada");

                var respuestasPorEncuesta = _context.RespuestasEncuesta
                    .Include("Encuesta")
                    .Where(r => r.SucursalId == sucursalId && r.Estado == "completada")
                    .GroupBy(r => new { r.EncuestaId, r.Encuesta.Titulo })
                    .Select(g => new
                    {
                        EncuestaId = g.Key.EncuestaId,
                        TituloEncuesta = g.Key.Titulo,
                        TotalRespuestas = g.Count()
                    })
                    .OrderByDescending(x => x.TotalRespuestas)
                    .ToList();

                var respuestasPorMes = _context.Database.SqlQuery<DataAPI.RespuestasPorMesDto>(@"
                    SELECT 
                        YEAR(fecha_completada) as Anio,
                        MONTH(fecha_completada) as Mes,
                        COUNT(*) as TotalRespuestas
                    FROM respuestas_encuesta 
                    WHERE sucursal_id = @p0 AND estado = 'completada'
                        AND fecha_completada >= DATEADD(month, -6, GETDATE())
                    GROUP BY YEAR(fecha_completada), MONTH(fecha_completada)
                    ORDER BY Anio DESC, Mes DESC
                ", sucursalId).ToList();

                return new
                {
                    Sucursal = new
                    {
                        sucursal.Id,
                        sucursal.CodigoSucursal,
                        sucursal.Nombre,
                        sucursal.Ciudad,
                        sucursal.Gerente
                    },
                    TotalRespuestas = totalRespuestas,
                    RespuestasPorEncuesta = respuestasPorEncuesta,
                    RespuestasPorMes = respuestasPorMes,
                    FechaGeneracion = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo estadísticas de sucursal {sucursalId}: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // VALIDAR SI SUCURSAL EXISTE Y ESTÁ ACTIVA
        // =====================================================
        public bool ValidarSucursalActiva(int sucursalId)
        {
            try
            {
                return _context.Sucursales
                    .Any(s => s.Id == sucursalId && s.Activa);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validando sucursal {sucursalId}: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}