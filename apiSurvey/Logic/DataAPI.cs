using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace apiSurvey.Logic
{
    public class DataAPI
    {
        // DTOs para requests/responses
        public class LoginRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }
        }


        public class LoginResponse
        {
            public string Token { get; set; }
            public string TipoUsuario { get; set; }
            public string Nombre { get; set; }
            public long ID { get; set; }
            public DateTime Expiracion { get; set; }
        }

        public class CrearEncuestaRequest
        {
            [Required]
            [StringLength(200)]
            public string Titulo { get; set; }

            public string Descripcion { get; set; }
            public DateTime? FechaLimite { get; set; }

            [Required]
            public List<CrearPreguntaRequest> Preguntas { get; set; }
        }

        public class CrearPreguntaRequest
        {
            [Required]
            public string TextoPregunta { get; set; }

            [Required]
            public string TipoPregunta { get; set; }

            public bool Obligatoria { get; set; }
            public int Orden { get; set; }

            public List<CrearOpcionRequest> Opciones { get; set; }
        }

        public class CrearOpcionRequest
        {
            [Required]
            public string TextoOpcion { get; set; }
            public string Valor { get; set; }
            public int Orden { get; set; }
        }

        public class ResponderEncuestaRequest
        {
            [Required]
            public List<RespuestaPreguntaRequest> Respuestas { get; set; }

            // NUEVO CAMPO OPCIONAL
            [Required]
            public int SucursalId { get; set; }

            // CAMPOS PARA IDENTIFICAR LA SOLICITUD

            [StringLength(50)]
            public string NumeroSolicitud { get; set; }    // Opcional: auto-generado si no se envía

            [StringLength(20)]
            public string TipoAtencion { get; set; }       // Ej: "Queja", "Consulta", "Reclamo"

            [StringLength(100)]
            public string ClienteNombre { get; set; }

            [StringLength(20)]
            public string ClienteTelefono { get; set; }
        }

        // DTO para mostrar sucursales
        public class SucursalDto
        {
            public int Id { get; set; }
            public string CodigoSucursal { get; set; }
            public string Nombre { get; set; }
            public string Direccion { get; set; }
            public string Ciudad { get; set; }
            public string Estado { get; set; }
            public string Telefono { get; set; }
            public string Gerente { get; set; }
            public string Email { get; set; }
            public bool Activa { get; set; }
        }

        // =====================================================
        // DTOs ADICIONALES
        // =====================================================

        public class ActualizarSucursalRequest
        {
            [Required]
            [StringLength(10)]
            public string CodigoSucursal { get; set; }

            [Required]
            [StringLength(100)]
            public string Nombre { get; set; }

            [StringLength(200)]
            public string Direccion { get; set; }

            [StringLength(50)]
            public string Ciudad { get; set; }

            [StringLength(50)]
            public string Estado { get; set; }

            [StringLength(20)]
            public string Telefono { get; set; }

            [StringLength(100)]
            public string Email { get; set; }

            [StringLength(100)]
            public string Gerente { get; set; }
        }

        public class RespuestasPorMesDto
        {
            public int Anio { get; set; }
            public int Mes { get; set; }
            public int TotalRespuestas { get; set; }
        }

        public class CrearSucursalRequest
        {
            [Required]
            [StringLength(10)]
            public string CodigoSucursal { get; set; }

            [Required]
            [StringLength(100)]
            public string Nombre { get; set; }

            [StringLength(200)]
            public string Direccion { get; set; }

            [StringLength(50)]
            public string Ciudad { get; set; }

            [StringLength(50)]
            public string Estado { get; set; }

            [StringLength(20)]
            public string Telefono { get; set; }

            [StringLength(100)]
            public string Email { get; set; }

            [StringLength(100)]
            public string Gerente { get; set; }
        }


        public class RespuestaPreguntaRequest
        {
            public int PreguntaId { get; set; }
            public List<int> OpcionesSeleccionadas { get; set; } // Para selección múltiple
            public int? OpcionSeleccionada { get; set; } // Para opción única
            public string TextoRespuesta { get; set; } // Para texto libre
        }

        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
            public List<string> Errors { get; set; }

            public ApiResponse()
            {
                Errors = new List<string>();
            }
        }

        // DTOs para respuestas
        public class EncuestaDto
        {
            public int Id { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public DateTime FechaCreacion { get; set; }
            public DateTime? FechaLimite { get; set; }
            public string NombreCreador { get; set; }
            public List<PreguntaDto> Preguntas { get; set; }
        }

        public class PreguntaDto
        {
            public int Id { get; set; }
            public string TextoPregunta { get; set; }
            public string TipoPregunta { get; set; }
            public bool Obligatoria { get; set; }
            public int Orden { get; set; }
            public List<OpcionRespuestaDto> Opciones { get; set; }
        }

        public class OpcionRespuestaDto
        {
            public int Id { get; set; }
            public string TextoOpcion { get; set; }
            public string Valor { get; set; }
            public int Orden { get; set; }
        }

        public class UsuarioDto
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public string Email { get; set; }
            public string TipoUsuario { get; set; }
            public DateTime FechaCreacion { get; set; }
            public bool Activo { get; set; }
            public string Password { get; set; }
        }
        
    

        //Usuarios
        public class RegistrarUsuarioRequest
        {
            [Required]
            [StringLength(100)]
            public string Nombre { get; set; }

            [Required]
            [EmailAddress]
            [StringLength(150)]
            public string Email { get; set; }

            [Required]
            [StringLength(50, MinimumLength = 6)]
            public string Password { get; set; }

            [Required]
            public string TipoUsuario { get; set; } // administrador, asesor
        }
    }
}