using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace apiSurvey.Models
{
    public class Model
    {

        [Table("usuarios", Schema = "migue_survey")]
        public class Usuario
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            [Column("nombre")]
            public string Nombre { get; set; }

            [Required]
            [EmailAddress]
            [StringLength(150)]
            [Column("email")]
            [Index(IsUnique = true)]
            public string Email { get; set; }

            [Required]
            [Column("password")]
            public string Password { get; set; }

            [Required]
            [StringLength(20)]
            [Column("tipo_usuario")]
            public string TipoUsuario { get; set; } // administrador, asesor

            [Column("fecha_creacion")]
            public DateTime FechaCreacion { get; set; } = DateTime.Now;

            [Column("activo")]
            public bool Activo { get; set; } = true;

            // Navegación
            public virtual ICollection<Encuesta> EncuestasCreadas { get; set; }
            public virtual ICollection<RespuestaEncuesta> RespuestasEncuestas { get; set; }

            public Usuario()
            {
                EncuestasCreadas = new HashSet<Encuesta>();
                RespuestasEncuestas = new HashSet<RespuestaEncuesta>();
            }
        }


        [Table("encuestas", Schema = "migue_survey")]
        public class Encuesta
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Required]
            [StringLength(200)]
            [Column("titulo")]
            public string Titulo { get; set; }

            [Column("descripcion")]
            public string Descripcion { get; set; }

            [Required]
            [Column("usuario_admin_id")]
            public int UsuarioAdminId { get; set; }

            [Column("fecha_creacion")]
            public DateTime FechaCreacion { get; set; } = DateTime.Now;

            [Column("fecha_limite")]
            public DateTime? FechaLimite { get; set; }

            [Column("activa")]
            public bool Activa { get; set; } = true;

            // Navegación
            [ForeignKey("UsuarioAdminId")]
            public virtual Usuario UsuarioAdmin { get; set; }

            public virtual ICollection<Pregunta> Preguntas { get; set; }
            public virtual ICollection<RespuestaEncuesta> RespuestasEncuestas { get; set; }

            public Encuesta()
            {
                Preguntas = new HashSet<Pregunta>();
                RespuestasEncuestas = new HashSet<RespuestaEncuesta>();
            }
        }

        [Table("preguntas", Schema = "migue_survey")]
        public class Pregunta
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Required]
            [Column("encuesta_id")]
            public int EncuestaId { get; set; }

            [Required]
            [Column("texto_pregunta")]
            public string TextoPregunta { get; set; }

            [Required]
            [StringLength(20)]
            [Column("tipo_pregunta")]
            public string TipoPregunta { get; set; } // opcion_multiple, seleccion_multiple, texto_libre

            [Column("obligatoria")]
            public bool Obligatoria { get; set; } = false;

            [Column("orden")]
            public int Orden { get; set; } = 1;

            // Navegación
            [ForeignKey("EncuestaId")]
            public virtual Encuesta Encuesta { get; set; }

            public virtual ICollection<OpcionRespuesta> Opciones { get; set; }
            public virtual ICollection<RespuestaDetalle> RespuestasDetalle { get; set; }

            public Pregunta()
            {
                Opciones = new HashSet<OpcionRespuesta>();
                RespuestasDetalle = new HashSet<RespuestaDetalle>();
            }
        }


        [Table("opciones_respuesta", Schema = "migue_survey")]
        public class OpcionRespuesta
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Required]
            [Column("pregunta_id")]
            public int PreguntaId { get; set; }

            [Required]
            [StringLength(500)]
            [Column("texto_opcion")]
            public string TextoOpcion { get; set; }

            [StringLength(100)]
            [Column("valor")]
            public string Valor { get; set; }

            [Column("orden")]
            public int Orden { get; set; } = 1;

            // Navegación
            [ForeignKey("PreguntaId")]
            public virtual Pregunta Pregunta { get; set; }

            public virtual ICollection<RespuestaDetalle> RespuestasDetalle { get; set; }

            public OpcionRespuesta()
            {
                RespuestasDetalle = new HashSet<RespuestaDetalle>();
            }
        }


        [Table("respuestas_encuesta", Schema = "migue_survey")]
        public class RespuestaEncuesta
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Required]
            [Column("encuesta_id")]
            public int EncuestaId { get; set; }

            [Required]
            [Column("usuario_asesor_id")]
            public int UsuarioAsesorId { get; set; }

            // NUEVO CAMPO
            [Required]
            [Column("sucursal_id")]
            public int? SucursalId { get; set; }

            // NUEVOS CAMPOS PARA SOLICITUDES MÚLTIPLES
            [StringLength(50)]
            [Column("numero_solicitud")]
            public string NumeroSolicitud { get; set; }  // Ej: "SOL-2024-001234"

            [StringLength(20)]
            [Column("tipo_atencion")]
            public string TipoAtencion { get; set; }     // Ej: "Queja", "Consulta", "Reclamo"

            [StringLength(100)]
            [Column("cliente_nombre")]
            public string ClienteNombre { get; set; }    // Nombre del cliente atendido

            [StringLength(20)]
            [Column("cliente_telefono")]
            public string ClienteTelefono { get; set; }  // Teléfono del cliente

            [Column("fecha_inicio")]
            public DateTime FechaInicio { get; set; } = DateTime.Now;

            [Column("fecha_completada")]
            public DateTime? FechaCompletada { get; set; }

            [StringLength(20)]
            [Column("estado")]
            public string Estado { get; set; } = "iniciada"; // iniciada, completada, abandonada

            // Navegación
            [ForeignKey("EncuestaId")]
            public virtual Encuesta Encuesta { get; set; }

            [ForeignKey("UsuarioAsesorId")]
            public virtual Usuario UsuarioAsesor { get; set; }

            // NUEVA NAVEGACIÓN
            [ForeignKey("SucursalId")]
            public virtual Sucursal Sucursal { get; set; }

            public virtual ICollection<RespuestaDetalle> RespuestasDetalle { get; set; }

            public RespuestaEncuesta()
            {
                RespuestasDetalle = new HashSet<RespuestaDetalle>();
            }
        }


        [Table("respuestas_detalle", Schema = "migue_survey")]
        public class RespuestaDetalle
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Required]
            [Column("respuesta_encuesta_id")]
            public int RespuestaEncuestaId { get; set; }

            [Required]
            [Column("pregunta_id")]
            public int PreguntaId { get; set; }

            [Column("opcion_id")]
            public int? OpcionId { get; set; } // Para preguntas de opción/selección múltiple

            [Column("texto_respuesta")]
            public string TextoRespuesta { get; set; } // Para preguntas de texto libre

            [Column("fecha_respuesta")]
            public DateTime FechaRespuesta { get; set; } = DateTime.Now;

            // Navegación
            [ForeignKey("RespuestaEncuestaId")]
            public virtual RespuestaEncuesta RespuestaEncuesta { get; set; }

            [ForeignKey("PreguntaId")]
            public virtual Pregunta Pregunta { get; set; }

            [ForeignKey("OpcionId")]
            public virtual OpcionRespuesta Opcion { get; set; }
        }

        [Table("sucursales", Schema = "migue_survey")]
        public class Sucursal
        {
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Required]
            [StringLength(10)]
            [Column("codigo_sucursal")]
            [Index(IsUnique = true)]
            public string CodigoSucursal { get; set; }

            [Required]
            [StringLength(100)]
            [Column("nombre")]
            public string Nombre { get; set; }

            [StringLength(200)]
            [Column("direccion")]
            public string Direccion { get; set; }

            [StringLength(50)]
            [Column("ciudad")]
            public string Ciudad { get; set; }

            [StringLength(50)]
            [Column("estado")]
            public string Estado { get; set; }

            [StringLength(20)]
            [Column("telefono")]
            public string Telefono { get; set; }

            [StringLength(100)]
            [Column("email")]
            public string Email { get; set; }

            [StringLength(100)]
            [Column("gerente")]
            public string Gerente { get; set; }

            [Column("activa")]
            public bool Activa { get; set; } = true;

            [Column("fecha_creacion")]
            public DateTime FechaCreacion { get; set; } = DateTime.Now;

            // Navegación
            public virtual ICollection<RespuestaEncuesta> RespuestasEncuestas { get; set; }

            public Sucursal()
            {
                RespuestasEncuestas = new HashSet<RespuestaEncuesta>();
            }
        }
    }
}