using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static apiSurvey.Models.Data;


namespace apiSurvey.Logic
{
    public class Services
    {
        public class UsuarioService : IDisposable
        {
            private readonly EncuestasContext _context;

            public UsuarioService()
            {
                try
                {
                    _context = new EncuestasContext();

                    // Verificar conexión al crear el servicio
                    var canConnect = _context.Database.Exists();
                    System.Diagnostics.Debug.WriteLine($"Conexión a BD: {(canConnect ? "Exitosa" : "Fallida")}");

                    if (!canConnect)
                    {
                        throw new InvalidOperationException("No se puede conectar a la base de datos");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al conectar BD: {ex.Message}");
                    throw new InvalidOperationException($"Error de conexión a BD: {ex.Message}", ex);
                }
            }

            public Models.Model.Usuario ValidarUsuario(string email, string password)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== VALIDANDO USUARIO ===");
                    System.Diagnostics.Debug.WriteLine($"Email: {email}");
                    System.Diagnostics.Debug.WriteLine($"Password length: {password?.Length ?? 0}");

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                    {
                        System.Diagnostics.Debug.WriteLine("Email o password vacío");
                        return null;
                    }

                    // PASO 1: Verificar que la tabla usuarios existe
                    System.Diagnostics.Debug.WriteLine("Verificando tabla usuarios...");
                    try
                    {
                        var tableExists = _context.Database.SqlQuery<int>(
                            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'usuarios'"
                        ).FirstOrDefault();

                        System.Diagnostics.Debug.WriteLine($"Tabla usuarios existe: {tableExists > 0}");

                        if (tableExists == 0)
                        {
                            throw new InvalidOperationException("La tabla 'usuarios' no existe en la base de datos");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error verificando tabla: {ex.Message}");
                        throw new InvalidOperationException($"Error accediendo a tabla usuarios: {ex.Message}", ex);
                    }

                    // PASO 2: Contar total de usuarios
                    System.Diagnostics.Debug.WriteLine("Contando usuarios...");
                    try
                    {
                        var totalUsuarios = _context.Database.SqlQuery<int>("SELECT COUNT(*) FROM usuarios").FirstOrDefault();
                        System.Diagnostics.Debug.WriteLine($"Total usuarios en BD: {totalUsuarios}");

                        if (totalUsuarios == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("No hay usuarios en la base de datos");
                            return null;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error contando usuarios: {ex.Message}");
                        throw new InvalidOperationException($"Error contando usuarios: {ex.Message}", ex);
                    }

                    // PASO 3: Buscar usuario por email (sin usar Entity Framework primero)
                    System.Diagnostics.Debug.WriteLine("Buscando usuario con SQL directo...");
                    try
                    {
                        var sqlQuery = "SELECT id, nombre, email, password, tipo_usuario, fecha_creacion, activo FROM usuarios WHERE email = @p0 AND activo = 1";
                        var usuariosEncontrados = _context.Database.SqlQuery<Logic.DataAPI.UsuarioDto>(sqlQuery, email).ToList();

                        System.Diagnostics.Debug.WriteLine($"Usuarios encontrados con email {email}: {usuariosEncontrados.Count}");

                        if (usuariosEncontrados.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Usuario no encontrado");
                            return null;
                        }

                        var usuarioDto = usuariosEncontrados.First();
                        System.Diagnostics.Debug.WriteLine($"Usuario encontrado: {usuarioDto.Nombre} ({usuarioDto.TipoUsuario})");

                        // PASO 4: Verificar password
                        System.Diagnostics.Debug.WriteLine("Verificando password...");
                        bool passwordValido = Helpers.VerifyPassword(password, usuarioDto.Password);
                        System.Diagnostics.Debug.WriteLine($"Password válido: {passwordValido}");

                        if (!passwordValido)
                        {
                            System.Diagnostics.Debug.WriteLine("Password incorrecto");
                            return null;
                        }

                        // PASO 5: Convertir DTO a entidad
                        var usuario = new Models.Model.Usuario
                        {
                            Id = usuarioDto.Id,
                            Nombre = usuarioDto.Nombre,
                            Email = usuarioDto.Email,
                            Password = usuarioDto.Password,
                            TipoUsuario = usuarioDto.TipoUsuario,
                            FechaCreacion = usuarioDto.FechaCreacion,
                            Activo = usuarioDto.Activo
                        };

                        System.Diagnostics.Debug.WriteLine("=== USUARIO VALIDADO EXITOSAMENTE ===");
                        return usuario;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error en consulta SQL: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                        throw new InvalidOperationException($"Error consultando usuario: {ex.Message}", ex);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== ERROR EN VALIDAR USUARIO ===");
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.GetType().Name}");
                    System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                    if (ex.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        System.Diagnostics.Debug.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                    }

                    System.Diagnostics.Debug.WriteLine($"=== FIN ERROR ===");

                    // Re-lanzar la excepción para que el controlador la maneje
                    throw;
                }
            }

            // Método alternativo usando solo Entity Framework (para comparar)
            public Models.Model.Usuario ValidarUsuarioEF(string email, string password)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== VALIDANDO USUARIO CON EF ===");

                    var usuario = _context.Usuarios
                        .Where(u => u.Email == email && u.Activo)
                        .FirstOrDefault();

                    if (usuario != null && Helpers.VerifyPassword(password, usuario.Password))
                    {
                        return usuario;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en ValidarUsuarioEF: {ex.Message}");
                    throw;
                }
            }
            public Models.Model.Usuario CrearUsuario(string nombre, string email, string password, string tipoUsuario)
            {
                var usuario = new Models.Model.Usuario
                {
                    Nombre = nombre,
                    Email = email,
                    Password = Helpers.HashPassword(password),
                    TipoUsuario = tipoUsuario,
                    FechaCreacion = DateTime.Now,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                return usuario;
            }

            // Método para verificar la integridad de la base de datos
            public void VerificarBaseDatos()
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== VERIFICANDO BASE DE DATOS ===");

                    // Verificar conexión
                    var exists = _context.Database.Exists();
                    System.Diagnostics.Debug.WriteLine($"BD existe: {exists}");

                    if (!exists)
                    {
                        throw new InvalidOperationException("La base de datos no existe");
                    }

                    // Verificar tablas
                    var tablas = _context.Database.SqlQuery<string>(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
                    ).ToList();

                    System.Diagnostics.Debug.WriteLine($"Tablas encontradas: {string.Join(", ", tablas)}");

                    // Verificar estructura de tabla usuarios
                    var columnas = _context.Database.SqlQuery<string>(
                        "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'usuarios'"
                    ).ToList();

                    System.Diagnostics.Debug.WriteLine($"Columnas en usuarios: {string.Join(", ", columnas)}");

                    // Verificar datos
                    var countUsuarios = _context.Database.SqlQuery<int>("SELECT COUNT(*) FROM usuarios").FirstOrDefault();
                    System.Diagnostics.Debug.WriteLine($"Total usuarios: {countUsuarios}");

                    if (countUsuarios > 0)
                    {
                        var primerosUsuarios = _context.Database.SqlQuery<string>(
                            "SELECT TOP 3 nombre + ' (' + email + ')' FROM usuarios"
                        ).ToList();

                        System.Diagnostics.Debug.WriteLine($"Primeros usuarios: {string.Join(", ", primerosUsuarios)}");
                    }

                    System.Diagnostics.Debug.WriteLine("=== FIN VERIFICACIÓN ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error verificando BD: {ex.Message}");
                    throw;
                }
            }

            public List<DataAPI.UsuarioDto> ObtenerUsuarios()
            {
                return _context.Usuarios
                    .Where(u => u.Activo)
                    .Select(u => new DataAPI.UsuarioDto
                    {
                        Id = u.Id,
                        Nombre = u.Nombre,
                        Email = u.Email,
                        TipoUsuario = u.TipoUsuario,
                        FechaCreacion = u.FechaCreacion,
                        Activo = u.Activo
                    })
                    .ToList();
            }

            public void Dispose()
            {
                _context?.Dispose();
            }
        }

        public class EncuestaService
        {
            private readonly EncuestasContext _context;

            public EncuestaService()
            {
                _context = new EncuestasContext();
            }

            public List<DataAPI.EncuestaDto> ObtenerEncuestasActivas()
            {
                // Usar string en lugar de lambda para Include
                var encuestasEntidades = _context.Encuestas
                    .Include("UsuarioAdmin")
                    .Where(e => e.Activa)
                    .OrderByDescending(e => e.FechaCreacion)
                    .ToList();

                // Proyectar a DTO en memoria
                return encuestasEntidades.Select(e => new DataAPI.EncuestaDto
                {
                    Id = e.Id,
                    Titulo = e.Titulo,
                    Descripcion = e.Descripcion,
                    FechaCreacion = e.FechaCreacion,
                    FechaLimite = e.FechaLimite,
                    NombreCreador = e.UsuarioAdmin.Nombre
                }).ToList();
            }

            public DataAPI.EncuestaDto ObtenerEncuestaCompleta(int encuestaId)
            {
                // Usar strings para navegación anidada
                var encuestaEntidad = _context.Encuestas
                    .Include("UsuarioAdmin")
                    .Include("Preguntas")
                    .Include("Preguntas.Opciones")
                    .FirstOrDefault(e => e.Id == encuestaId && e.Activa);

                if (encuestaEntidad == null) return null;

                return new DataAPI.EncuestaDto
                {
                    Id = encuestaEntidad.Id,
                    Titulo = encuestaEntidad.Titulo,
                    Descripcion = encuestaEntidad.Descripcion,
                    FechaCreacion = encuestaEntidad.FechaCreacion,
                    FechaLimite = encuestaEntidad.FechaLimite,
                    NombreCreador = encuestaEntidad.UsuarioAdmin.Nombre,
                    Preguntas = encuestaEntidad.Preguntas
                        .OrderBy(p => p.Orden)
                        .Select(p => new DataAPI.PreguntaDto
                        {
                            Id = p.Id,
                            TextoPregunta = p.TextoPregunta,
                            TipoPregunta = p.TipoPregunta,
                            Obligatoria = p.Obligatoria,
                            Orden = p.Orden,
                            Opciones = p.Opciones
                                .OrderBy(o => o.Orden)
                                .Select(o => new DataAPI.OpcionRespuestaDto
                                {
                                    Id = o.Id,
                                    TextoOpcion = o.TextoOpcion,
                                    Valor = o.Valor,
                                    Orden = o.Orden
                                }).ToList()
                        }).ToList()
                };
            }

            public int CrearEncuesta(DataAPI.CrearEncuestaRequest request, int usuarioAdminId)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var encuesta = new Models.Model.Encuesta
                        {
                            Titulo = request.Titulo,
                            Descripcion = request.Descripcion,
                            UsuarioAdminId = usuarioAdminId,
                            FechaLimite = request.FechaLimite,
                            FechaCreacion = DateTime.Now,
                            Activa = true
                        };

                        _context.Encuestas.Add(encuesta);
                        _context.SaveChanges();

                        // Crear preguntas
                        foreach (var preguntaReq in request.Preguntas)
                        {
                            var pregunta = new Models.Model.Pregunta
                            {
                                EncuestaId = encuesta.Id,
                                TextoPregunta = preguntaReq.TextoPregunta,
                                TipoPregunta = preguntaReq.TipoPregunta,
                                Obligatoria = preguntaReq.Obligatoria,
                                Orden = preguntaReq.Orden
                            };

                            _context.Preguntas.Add(pregunta);
                            _context.SaveChanges();

                            // Crear opciones si las hay
                            if (preguntaReq.Opciones != null && preguntaReq.Opciones.Any())
                            {
                                foreach (var opcionReq in preguntaReq.Opciones)
                                {
                                    var opcion = new Models.Model.OpcionRespuesta
                                    {
                                        PreguntaId = pregunta.Id,
                                        TextoOpcion = opcionReq.TextoOpcion,
                                        Valor = opcionReq.Valor,
                                        Orden = opcionReq.Orden
                                    };

                                    _context.OpcionesRespuesta.Add(opcion);
                                }
                            }
                        }

                        _context.SaveChanges();
                        transaction.Commit();

                        return encuesta.Id;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            public bool ResponderEncuesta(int encuestaId, int usuarioAsesorId, DataAPI.ResponderEncuestaRequest request)
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Verificar si ya respondió
                        //var yaRespondida = _context.RespuestasEncuesta
                        //    .Any(r => r.EncuestaId == encuestaId && r.UsuarioAsesorId == usuarioAsesorId);

                        //if (yaRespondida)
                        //{
                        //    throw new InvalidOperationException("Ya ha respondido esta encuesta");
                        //}

                        // Generar número de solicitud si no viene
                        var numeroSolicitud = request.NumeroSolicitud;
                        if (string.IsNullOrEmpty(numeroSolicitud))
                        {
                            numeroSolicitud = GenerarNumeroSolicitud();
                        }

                        // Crear respuesta de encuesta
                        var respuestaEncuesta = new Models.Model.RespuestaEncuesta
                        {
                            EncuestaId = encuestaId,
                            UsuarioAsesorId = usuarioAsesorId,
                            SucursalId = request.SucursalId,
                            NumeroSolicitud = numeroSolicitud,
                            TipoAtencion = request.TipoAtencion,
                            ClienteNombre = request.ClienteNombre,
                            ClienteTelefono = request.ClienteTelefono,
                            FechaInicio = DateTime.Now,
                            FechaCompletada = DateTime.Now,
                            Estado = "completada"
                        };

                        _context.RespuestasEncuesta.Add(respuestaEncuesta);
                        _context.SaveChanges();

                        // Guardar respuestas detalladas
                        foreach (var respuesta in request.Respuestas)
                        {
                            if (respuesta.OpcionesSeleccionadas != null && respuesta.OpcionesSeleccionadas.Any())
                            {
                                // Selección múltiple
                                foreach (var opcionId in respuesta.OpcionesSeleccionadas)
                                {
                                    var detalle = new Models.Model.RespuestaDetalle
                                    {
                                        RespuestaEncuestaId = respuestaEncuesta.Id,
                                        PreguntaId = respuesta.PreguntaId,
                                        OpcionId = opcionId,
                                        FechaRespuesta = DateTime.Now
                                    };

                                    _context.RespuestasDetalle.Add(detalle);
                                }
                            }
                            else if (respuesta.OpcionSeleccionada.HasValue)
                            {
                                // Opción única
                                var detalle = new Models.Model.RespuestaDetalle
                                {
                                    RespuestaEncuestaId = respuestaEncuesta.Id,
                                    PreguntaId = respuesta.PreguntaId,
                                    OpcionId = respuesta.OpcionSeleccionada.Value,
                                    FechaRespuesta = DateTime.Now
                                };

                                _context.RespuestasDetalle.Add(detalle);
                            }
                            else if (!string.IsNullOrEmpty(respuesta.TextoRespuesta))
                            {
                                // Texto libre
                                var detalle = new Models.Model.RespuestaDetalle
                                {
                                    RespuestaEncuestaId = respuestaEncuesta.Id,
                                    PreguntaId = respuesta.PreguntaId,
                                    TextoRespuesta = respuesta.TextoRespuesta,
                                    FechaRespuesta = DateTime.Now
                                };

                                _context.RespuestasDetalle.Add(detalle);
                            }
                        }

                        _context.SaveChanges();
                        transaction.Commit();

                        System.Diagnostics.Debug.WriteLine($"Respuesta guardada - Solicitud: {numeroSolicitud}, Usuario: {usuarioAsesorId}");
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // =====================================================
            // MÉTODO PARA GENERAR NÚMERO DE SOLICITUD
            // =====================================================

            private string GenerarNumeroSolicitud()
            {
                try
                {
                    // Formato: SOL-YYYY-NNNNNN
                    var año = DateTime.Now.Year;

                    // Obtener el último número del año actual
                    var ultimoNumero = _context.Database.SqlQuery<int>(@"
                            SELECT ISNULL(MAX(CAST(RIGHT(numero_solicitud, 6) AS INT)), 0)
                            FROM respuestas_encuesta 
                            WHERE numero_solicitud LIKE @p0
                        ", $"SOL-{año}-%").FirstOrDefault();

                    var nuevoNumero = ultimoNumero + 1;
                    return $"SOL-{año}-{nuevoNumero:D6}";
                }
                catch
                {
                    // Fallback: usar timestamp
                    return $"SOL-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmss}";
                }
            }



            public bool UsuarioYaRespondio(int encuestaId, int usuarioId)
            {
                return _context.RespuestasEncuesta
                    .Any(r => r.EncuestaId == encuestaId && r.UsuarioAsesorId == usuarioId);
            }

            public void Dispose()
            {
                _context?.Dispose();
            }
        }
    }
}