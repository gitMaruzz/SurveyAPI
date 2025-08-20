using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace apiSurvey.Models
{
    public class Data
    {
        public class EncuestasContext : DbContext
        {
            public EncuestasContext() : base("name=EncuestasContext")
            {
                // IMPORTANTE: Usar "name=" para referenciar connection string por nombre

                // Configuración optimizada para Entity Framework 6
                Database.SetInitializer<EncuestasContext>(null); // Deshabilitar inicializador automático

                // Configuraciones para evitar problemas con Include y proyecciones
                Configuration.LazyLoadingEnabled = false;  // Evita cargas automáticas no deseadas
                Configuration.ProxyCreationEnabled = false; // Mejora rendimiento en proyecciones
                Configuration.AutoDetectChangesEnabled = true;
                Configuration.ValidateOnSaveEnabled = false; // Mejorar rendimiento
            }

            // Método alternativo para crear contexto con connection string específico
            public static EncuestasContext Create()
            {
                try
                {
                    var context = new EncuestasContext();

                    // Verificar que la conexión funcione
                    if (!context.Database.Exists())
                    {
                        System.Diagnostics.Debug.WriteLine("La base de datos no existe. Creándola...");
                        context.Database.Create();
                    }

                    return context;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al crear contexto: {ex.Message}");
                    throw;
                }
            }

            // DbSets
            public DbSet<Model.Usuario> Usuarios { get; set; }
            public DbSet<Model.Encuesta> Encuestas { get; set; }
            public DbSet<Model.Pregunta> Preguntas { get; set; }
            public DbSet<Model.OpcionRespuesta> OpcionesRespuesta { get; set; }
            public DbSet<Model.RespuestaEncuesta> RespuestasEncuesta { get; set; }
            public DbSet<Model.RespuestaDetalle> RespuestasDetalle { get; set; }
            // NUEVO DbSet
            public DbSet<Model.Sucursal> Sucursales { get; set; }


            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                // Remover convención de pluralización
                modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

                // Configurar relaciones y restricciones SIN CASCADAS CONFLICTIVAS

                // Usuario -> Encuesta (Uno a Muchos) - SIN CASCADE
                modelBuilder.Entity<Model.Encuesta>()
                    .HasRequired(e => e.UsuarioAdmin)
                    .WithMany(u => u.EncuestasCreadas)
                    .HasForeignKey(e => e.UsuarioAdminId)
                    .WillCascadeOnDelete(false); // ← NO CASCADE

                // Encuesta -> Pregunta (Uno a Muchos) - CON CASCADE
                modelBuilder.Entity<Model.Pregunta>()
                    .HasRequired(p => p.Encuesta)
                    .WithMany(e => e.Preguntas)
                    .HasForeignKey(p => p.EncuestaId)
                    .WillCascadeOnDelete(true); // ← CASCADE OK

                // Pregunta -> OpcionRespuesta (Uno a Muchos) - CON CASCADE
                modelBuilder.Entity<Model.OpcionRespuesta>()
                    .HasRequired(o => o.Pregunta)
                    .WithMany(p => p.Opciones)
                    .HasForeignKey(o => o.PreguntaId)
                    .WillCascadeOnDelete(true); // ← CASCADE OK

                // Encuesta -> RespuestaEncuesta (Uno a Muchos) - SIN CASCADE
                modelBuilder.Entity<Model.RespuestaEncuesta>()
                    .HasRequired(r => r.Encuesta)
                    .WithMany(e => e.RespuestasEncuestas)
                    .HasForeignKey(r => r.EncuestaId)
                    .WillCascadeOnDelete(false); // ← NO CASCADE para evitar múltiples rutas

                // Usuario -> RespuestaEncuesta (Uno a Muchos) - SIN CASCADE
                modelBuilder.Entity<Model.RespuestaEncuesta>()
                    .HasRequired(r => r.UsuarioAsesor)
                    .WithMany(u => u.RespuestasEncuestas)
                    .HasForeignKey(r => r.UsuarioAsesorId)
                    .WillCascadeOnDelete(false); // ← NO CASCADE

                // RespuestaEncuesta -> RespuestaDetalle (Uno a Muchos) - CON CASCADE
                modelBuilder.Entity<Model.RespuestaDetalle>()
                    .HasRequired(rd => rd.RespuestaEncuesta)
                    .WithMany(r => r.RespuestasDetalle)
                    .HasForeignKey(rd => rd.RespuestaEncuestaId)
                    .WillCascadeOnDelete(true); // ← CASCADE OK

                // Pregunta -> RespuestaDetalle (Uno a Muchos) - SIN CASCADE
                modelBuilder.Entity<Model.RespuestaDetalle>()
                    .HasRequired(rd => rd.Pregunta)
                    .WithMany(p => p.RespuestasDetalle)
                    .HasForeignKey(rd => rd.PreguntaId)
                    .WillCascadeOnDelete(false); // ← NO CASCADE

                // OpcionRespuesta -> RespuestaDetalle (Uno a Muchos - Opcional) - SIN CASCADE
                modelBuilder.Entity<Model.RespuestaDetalle>()
                    .HasOptional(rd => rd.Opcion)
                    .WithMany(o => o.RespuestasDetalle)
                    .HasForeignKey(rd => rd.OpcionId)
                    .WillCascadeOnDelete(false); // ← NO CASCADE

                // Índices únicos
                modelBuilder.Entity<Model.Usuario>()
                    .HasIndex(u => u.Email)
                    .IsUnique();

                // Restricción única: Un usuario solo puede responder una encuesta una vez
                modelBuilder.Entity<Model.RespuestaEncuesta>()
                    .HasIndex(r => new { r.EncuestaId, r.UsuarioAsesorId })
                    .IsUnique();

                // NUEVA CONFIGURACIÓN: Sucursal -> RespuestaEncuesta
                modelBuilder.Entity<Model.RespuestaEncuesta>()
                    .HasOptional(r => r.Sucursal)
                    .WithMany(s => s.RespuestasEncuestas)
                    .HasForeignKey(r => r.SucursalId)
                    .WillCascadeOnDelete(false);

                // Índice único para código de sucursal
                modelBuilder.Entity<Model.Sucursal>()
                    .HasIndex(s => s.CodigoSucursal)
                    .IsUnique();

                base.OnModelCreating(modelBuilder);

                base.OnModelCreating(modelBuilder);
            }
        }
    }
}