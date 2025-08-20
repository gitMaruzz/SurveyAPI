using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static apiSurvey.Models.Data;

namespace apiSurvey.Logic
{
    public class ReporteService
    {
        private readonly EncuestasContext _context;

        public ReporteService()
        {
            _context = new EncuestasContext();
        }

        // Método adicional para obtener todas las respuestas de una encuesta
        public List<object> ObtenerTodasLasRespuestasEncuesta(int encuestaId)
        {
            try
            {
                var respuestasCompletas = _context.RespuestasEncuesta
                    .Include("Encuesta")
                    .Include("UsuarioAsesor")
                    .Include("RespuestasDetalle")
                    .Include("RespuestasDetalle.Pregunta")
                    .Include("RespuestasDetalle.Opcion")
                    .Where(r => r.EncuestaId == encuestaId && r.Estado == "completada")
                    .ToList();

                return respuestasCompletas.Select(r => new
                {
                    RespuestaId = r.Id,
                    UsuarioId = r.UsuarioAsesorId,
                    NombreUsuario = r.UsuarioAsesor.Nombre,
                    EmailUsuario = r.UsuarioAsesor.Email,
                    FechaInicio = r.FechaInicio,
                    FechaRespuesta = r.FechaCompletada,
                    Estado = r.Estado,
                    Respuestas = r.RespuestasDetalle.OrderBy(rd => rd.Pregunta.Orden).Select(rd => new
                    {
                        PreguntaId = rd.PreguntaId,
                        TextoPregunta = rd.Pregunta.TextoPregunta,
                        TipoPregunta = rd.Pregunta.TipoPregunta,
                        OrdenPregunta = rd.Pregunta.Orden,
                        OpcionSeleccionada = rd.Opcion?.TextoOpcion,
                        ValorOpcion = rd.Opcion?.Valor,
                        TextoRespuesta = rd.TextoRespuesta,
                        FechaRespuesta = rd.FechaRespuesta
                    }).ToList()
                }).OrderByDescending(r => r.FechaRespuesta).ToList<object>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo todas las respuestas de la encuesta {encuestaId}: {ex.Message}");
                throw;
            }
        }

        // Método para obtener resumen general
        public object ObtenerResumenGeneral()
        {
            try
            {
                var totalEncuestas = _context.Encuestas.Count(e => e.Activa);
                var totalRespuestas = _context.RespuestasEncuesta.Count(r => r.Estado == "completada");
                var totalUsuarios = _context.Usuarios.Count(u => u.Activo);
                var totalAsesores = _context.Usuarios.Count(u => u.Activo && u.TipoUsuario == "asesor");

                var encuestasMasRespondidas = _context.Encuestas
                    .Include("RespuestasEncuestas")
                    .Where(e => e.Activa)
                    .Select(e => new
                    {
                        EncuestaId = e.Id,
                        Titulo = e.Titulo,
                        TotalRespuestas = e.RespuestasEncuestas.Count(r => r.Estado == "completada"),
                        FechaCreacion = e.FechaCreacion
                    })
                    .OrderByDescending(e => e.TotalRespuestas)
                    .Take(5)
                    .ToList();

                var usuariosMasActivos = _context.Usuarios
                    .Include("RespuestasEncuestas")
                    .Where(u => u.Activo && u.TipoUsuario == "asesor")
                    .Select(u => new
                    {
                        UsuarioId = u.Id,
                        Nombre = u.Nombre,
                        Email = u.Email,
                        TotalRespuestas = u.RespuestasEncuestas.Count(r => r.Estado == "completada")
                    })
                    .OrderByDescending(u => u.TotalRespuestas)
                    .Take(10)
                    .ToList();

                return new
                {
                    TotalEncuestas = totalEncuestas,
                    TotalRespuestas = totalRespuestas,
                    TotalUsuarios = totalUsuarios,
                    TotalAsesores = totalAsesores,
                    PromedioRespuestasPorEncuesta = totalEncuestas > 0 ? (double)totalRespuestas / totalEncuestas : 0,
                    EncuestasMasRespondidas = encuestasMasRespondidas,
                    UsuariosMasActivos = usuariosMasActivos,
                    FechaGeneracion = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo resumen general: {ex.Message}");
                throw;
            }
        }

        public object ObtenerEstadisticasEncuesta(int encuestaId)
        {
            // Usar strings para Include múltiples
            var encuesta = _context.Encuestas
                .Include("Preguntas")
                .Include("Preguntas.Opciones")
                .Include("RespuestasEncuestas")
                .Include("RespuestasEncuestas.RespuestasDetalle")
                .FirstOrDefault(e => e.Id == encuestaId);

            if (encuesta == null) return null;

            var totalRespuestas = encuesta.RespuestasEncuestas.Count(r => r.Estado == "completada");

            // Materializar las preguntas primero
            var preguntasList = encuesta.Preguntas.ToList();
            var estadisticasPorPregunta = new List<object>();

            foreach (var pregunta in preguntasList)
            {
                var respuestas = encuesta.RespuestasEncuestas
                    .Where(r => r.Estado == "completada")
                    .SelectMany(r => r.RespuestasDetalle)
                    .Where(rd => rd.PreguntaId == pregunta.Id)
                    .ToList();

                if (pregunta.TipoPregunta == "texto_libre")
                {
                    estadisticasPorPregunta.Add(new
                    {
                        PreguntaId = pregunta.Id,
                        TextoPregunta = pregunta.TextoPregunta,
                        TipoPregunta = pregunta.TipoPregunta,
                        TotalRespuestas = respuestas.Count,
                        Respuestas = respuestas.Select(r => r.TextoRespuesta).Where(t => !string.IsNullOrEmpty(t)).ToList()
                    });
                }
                else
                {
                    var opcionesList = pregunta.Opciones.ToList();
                    var estadisticasOpciones = new List<object>();

                    foreach (var opcion in opcionesList)
                    {
                        var cantidad = respuestas.Count(r => r.OpcionId == opcion.Id);
                        var porcentaje = totalRespuestas > 0
                            ? Math.Round((double)cantidad / totalRespuestas * 100, 2)
                            : 0;

                        estadisticasOpciones.Add(new
                        {
                            OpcionId = opcion.Id,
                            TextoOpcion = opcion.TextoOpcion,
                            Valor = opcion.Valor,
                            Cantidad = cantidad,
                            Porcentaje = porcentaje
                        });
                    }

                    estadisticasPorPregunta.Add(new
                    {
                        PreguntaId = pregunta.Id,
                        TextoPregunta = pregunta.TextoPregunta,
                        TipoPregunta = pregunta.TipoPregunta,
                        TotalRespuestas = respuestas.Count,
                        Opciones = estadisticasOpciones
                    });
                }
            }

            return new
            {
                EncuestaId = encuesta.Id,
                Titulo = encuesta.Titulo,
                TotalRespuestas = totalRespuestas,
                FechaCreacion = encuesta.FechaCreacion,
                Preguntas = estadisticasPorPregunta
            };
        }

        public List<object> ObtenerRespuestasUsuario(int usuarioId)
        {
            try
            {
                // Materializar primero, luego proyectar
                var respuestasEntidades = _context.RespuestasEncuesta
                    .Include("Encuesta")
                    .Include("UsuarioAsesor")
                    .Include("RespuestasDetalle")
                    .Include("RespuestasDetalle.Pregunta")
                    .Include("RespuestasDetalle.Opcion")
                    .Where(r => r.UsuarioAsesorId == usuarioId)
                    .ToList();

                return respuestasEntidades.Select(r => new
                {
                    RespuestaEncuestaId = r.Id,
                    EncuestaId = r.EncuestaId,
                    TituloEncuesta = r.Encuesta.Titulo,
                    DescripcionEncuesta = r.Encuesta.Descripcion,
                    FechaInicio = r.FechaInicio,
                    FechaRespuesta = r.FechaCompletada,
                    Estado = r.Estado,
                    NombreUsuario = r.UsuarioAsesor.Nombre,
                    EmailUsuario = r.UsuarioAsesor.Email,
                    Respuestas = r.RespuestasDetalle.Select(rd => new
                    {
                        RespuestaDetalleId = rd.Id,
                        PreguntaId = rd.PreguntaId,
                        TextoPregunta = rd.Pregunta.TextoPregunta,
                        TipoPregunta = rd.Pregunta.TipoPregunta,
                        OrdenPregunta = rd.Pregunta.Orden,
                        OpcionId = rd.OpcionId,
                        OpcionSeleccionada = rd.Opcion != null ? rd.Opcion.TextoOpcion : null,
                        ValorOpcion = rd.Opcion != null ? rd.Opcion.Valor : null,
                        TextoRespuesta = rd.TextoRespuesta,
                        FechaRespuesta = rd.FechaRespuesta
                    }).OrderBy(resp => resp.OrdenPregunta).ToList()
                }).ToList<object>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo respuestas del usuario {usuarioId}: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // NUEVOS MÉTODOS DE CONSULTA
        // =====================================================

        // Obtener respuestas por número de solicitud
        public object ObtenerRespuestaPorSolicitud(string numeroSolicitud)
        {
            try
            {
                var respuesta = _context.RespuestasEncuesta
                    .Include("Encuesta")
                    .Include("UsuarioAsesor")
                    .Include("Sucursal")
                    .Include("RespuestasDetalle")
                    .Include("RespuestasDetalle.Pregunta")
                    .Include("RespuestasDetalle.Opcion")
                    .FirstOrDefault(r => r.NumeroSolicitud == numeroSolicitud);

                if (respuesta == null)
                {
                    return null;
                }

                return new
                {
                    NumeroSolicitud = respuesta.NumeroSolicitud,
                    TipoAtencion = respuesta.TipoAtencion,
                    ClienteNombre = respuesta.ClienteNombre,
                    ClienteTelefono = respuesta.ClienteTelefono,
                    FechaAtencion = respuesta.FechaCompletada,
                    Asesor = respuesta.UsuarioAsesor.Nombre,
                    Sucursal = respuesta.Sucursal?.Nombre,
                    Encuesta = respuesta.Encuesta.Titulo,
                    Respuestas = respuesta.RespuestasDetalle.Select(rd => new
                    {
                        Pregunta = rd.Pregunta.TextoPregunta,
                        TipoPregunta = rd.Pregunta.TipoPregunta,
                        OpcionSeleccionada = rd.Opcion?.TextoOpcion,
                        TextoRespuesta = rd.TextoRespuesta
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo respuesta por solicitud {numeroSolicitud}: {ex.Message}");
                throw;
            }
        }

        // Obtener respuestas de un asesor en un rango de fechas
        public List<object> ObtenerRespuestasAsesorPorFecha(int usuarioId, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var respuestas = _context.RespuestasEncuesta
                    .Include("Encuesta")
                    .Include("Sucursal")
                    .Where(r => r.UsuarioAsesorId == usuarioId &&
                               r.FechaCompletada >= fechaInicio &&
                               r.FechaCompletada <= fechaFin &&
                               r.Estado == "completada")
                    .OrderByDescending(r => r.FechaCompletada)
                    .ToList();

                return respuestas.Select(r => new
                {
                    NumeroSolicitud = r.NumeroSolicitud,
                    TipoAtencion = r.TipoAtencion,
                    ClienteNombre = r.ClienteNombre,
                    FechaAtencion = r.FechaCompletada,
                    Encuesta = r.Encuesta.Titulo,
                    Sucursal = r.Sucursal?.Nombre
                }).ToList<object>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo respuestas por fecha: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}