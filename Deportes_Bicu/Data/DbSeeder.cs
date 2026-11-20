using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;
using Microsoft.EntityFrameworkCore;

using System;

namespace Deportes_Bicu.Data
{
    public class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            await SeedRolesAsync(context);
            var permisosNuevos = await SeedPermissionsAsync(context);
            await SeedRolePermissionAsync(context, permisosNuevos);
            await SeedUsersAsync(context);
            await SeedFacultadAsync(context);
            await SeedCarreraAsync(context);
            await SeedEstudianteAsync(context);
            await SeedEntrenadorAsync(context);
            await SeedCatalogosDeportivosAsync(context);
            await SeedActividadAsync(context);
        }
        private static async Task SeedRolesAsync(AppDbContext context)
        {
            var changed = false;
            if (!await context.Role.AnyAsync(r => r.Descripcion == "Admin"))
            {
                context.Role.Add(new Role { Descripcion = "Admin" });
                changed = true;
            }
            if (!await context.Role.AnyAsync(r => r.Descripcion == "Usuario"))
            {
                context.Role.Add(new Role { Descripcion = "Usuario" });
                changed = true;
            }
            
            if (!await context.Role.AnyAsync(r => r.Descripcion == "Estudiante"))
            {
                context.Role.Add(new Role { Descripcion = "Estudiante" });
                changed = true;
            }
            if (!await context.Role.AnyAsync(r => r.Descripcion == "Entrenador"))
            {
                context.Role.Add(new Role { Descripcion = "Entrenador" });
                changed = true;
            }
            if (changed)
                await context.SaveChangesAsync();

        }

        private static async Task<List<Permission>> SeedPermissionsAsync(AppDbContext context)
        {
            var requeridos = new List<Permission>
            {
                new Permission { Nombre = "estudiante.ver", Descripcion = "Ver estudiante" },
                new Permission { Nombre = "estudiante.crear", Descripcion = "crear estudiante" },
                new Permission { Nombre = "estudiante.actualizar", Descripcion = "actualizar estudiante" },
                new Permission { Nombre = "estudiante.borrar", Descripcion = "borrar estudiante" },

                new Permission { Nombre = "rol.ver", Descripcion = "Ver rol" },
                new Permission { Nombre = "rol.crear", Descripcion = "crear rol" },
                new Permission { Nombre = "rol.asignar", Descripcion = "asignar rol" },

                new Permission { Nombre = "permiso.ver", Descripcion = "ver permiso" },
                new Permission { Nombre = "permiso.crear", Descripcion = "crear permiso" },
                new Permission { Nombre = "permiso.actualizar", Descripcion = "actualizar permiso" },
                new Permission { Nombre = "permiso.borrar", Descripcion = "borrar permiso" },

                new Permission { Nombre = "usuario.ver", Descripcion = "ver usuario" },
                new Permission { Nombre = "usuario.crear", Descripcion = "crear usuario" },
                new Permission { Nombre = "usuario.asignar", Descripcion = "asignar permisos a usuario" },
                new Permission { Nombre = "usuario.cambiar_rol", Descripcion = "cambiar rol de usuario" },

                new Permission { Nombre = "facultad.ver", Descripcion = "ver facultad" },
                new Permission { Nombre = "facultad.crear", Descripcion = "crear facultad" },
                new Permission { Nombre = "facultad.actualizar", Descripcion = "actualizar facultad" },
                new Permission { Nombre = "facultad.borrar", Descripcion = "borrar facultad" },

                new Permission { Nombre = "carrera.ver", Descripcion = "ver carrera" },
                new Permission { Nombre = "carrera.crear", Descripcion = "crear carrera" },
                new Permission { Nombre = "carrera.actualizar", Descripcion = "actualizar carrera" },
                new Permission { Nombre = "carrera.borrar", Descripcion = "borrar carrera" },

                new Permission { Nombre = "deporte.ver", Descripcion = "ver deporte" },
                new Permission { Nombre = "deporte.crear", Descripcion = "crear deporte" },
                new Permission { Nombre = "deporte.actualizar", Descripcion = "actualizar deporte" },
                new Permission { Nombre = "deporte.borrar", Descripcion = "borrar deporte" },
                new Permission { Nombre = "deporte.asignar", Descripcion = "asignar categorias a deporte" },

                new Permission { Nombre = "categoria.ver", Descripcion = "ver categoria deportiva" },
                new Permission { Nombre = "categoria.crear", Descripcion = "crear categoria deportiva" },
                new Permission { Nombre = "categoria.actualizar", Descripcion = "actualizar categoria deportiva" },
                new Permission { Nombre = "categoria.borrar", Descripcion = "borrar categoria deportiva" },

                new Permission { Nombre = "instalacion.ver", Descripcion = "ver instalacion" },
                new Permission { Nombre = "instalacion.crear", Descripcion = "crear instalacion" },
                new Permission { Nombre = "instalacion.actualizar", Descripcion = "actualizar instalacion" },
                new Permission { Nombre = "instalacion.borrar", Descripcion = "borrar instalacion" },

                new Permission { Nombre = "periodo.ver", Descripcion = "ver periodo" },
                new Permission { Nombre = "periodo.crear", Descripcion = "crear periodo" },
                new Permission { Nombre = "periodo.actualizar", Descripcion = "actualizar periodo" },
                new Permission { Nombre = "periodo.borrar", Descripcion = "borrar periodo" },

                new Permission { Nombre = "entrenador.ver", Descripcion = "ver entrenador" },
                new Permission { Nombre = "entrenador.crear", Descripcion = "crear entrenador" },
                new Permission { Nombre = "entrenador.actualizar", Descripcion = "actualizar entrenador" },
                new Permission { Nombre = "entrenador.borrar", Descripcion = "borrar entrenador" },
                new Permission { Nombre = "entrenador.asignar", Descripcion = "asignar deportes a entrenador" },

                new Permission { Nombre = "horario.ver", Descripcion = "ver horario" },
                new Permission { Nombre = "horario.crear", Descripcion = "crear horario" },
                new Permission { Nombre = "horario.actualizar", Descripcion = "actualizar horario" },
                new Permission { Nombre = "horario.borrar", Descripcion = "borrar horario" },

                new Permission { Nombre = "inscripcion.ver", Descripcion = "ver inscripciones" },
                new Permission { Nombre = "inscripcion.crear", Descripcion = "inscribir estudiantes" },
                new Permission { Nombre = "inscripcion.actualizar", Descripcion = "finalizar o cancelar inscripciones" },
                new Permission { Nombre = "inscripcion.borrar", Descripcion = "borrar inscripciones" },

                new Permission { Nombre = "asistencia.ver", Descripcion = "ver asistencias" },
                new Permission { Nombre = "asistencia.crear", Descripcion = "crear asistencias manualmente" },
                new Permission { Nombre = "asistencia.actualizar", Descripcion = "actualizar asistencias" },
                new Permission { Nombre = "asistencia.borrar", Descripcion = "borrar asistencias" },
                new Permission { Nombre = "asistencia.registrar", Descripcion = "registrar asistencia de sus horarios (entrenador)" },

                new Permission { Nombre = "evaluacion.ver", Descripcion = "ver evaluaciones fisicas" },
                new Permission { Nombre = "evaluacion.crear", Descripcion = "crear evaluaciones fisicas" },
                new Permission { Nombre = "evaluacion.actualizar", Descripcion = "actualizar evaluaciones fisicas" },
                new Permission { Nombre = "evaluacion.borrar", Descripcion = "borrar evaluaciones fisicas" },

                new Permission { Nombre = "portalestudiante.ver", Descripcion = "ver su portal de estudiante" },
                new Permission { Nombre = "portalestudiante.inscribir", Descripcion = "inscribirse y cancelar sus inscripciones" },
                new Permission { Nombre = "portalentrenador.ver", Descripcion = "ver su portal de entrenador" }
            };

            var existentes = await context.Permission.Select(p => p.Nombre).ToListAsync();
            var faltantes = requeridos.Where(p => !existentes.Contains(p.Nombre)).ToList();
            if (faltantes.Count == 0)
                return faltantes;

            context.Permission.AddRange(faltantes);
            await context.SaveChangesAsync();
            return faltantes;
        }

        private static async Task SeedRolePermissionAsync(AppDbContext context, List<Permission> permisosNuevos)
        {
            var adminRole = await context.Role.FirstAsync(r => r.Descripcion == "Admin");
            var usuarioRol = await context.Role.FirstAsync(r => r.Descripcion == "Usuario");

            if (await context.RolePermission.AnyAsync())
            {
                foreach (var permission in permisosNuevos)
                {
                    context.RolePermission.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    });
                }
                if (permisosNuevos.Count > 0)
                    await context.SaveChangesAsync();
                await SeedRolesDeportivosAsync(context);
                return;
            }

            var permissions = await context.Permission.ToListAsync();

            foreach (var permission in permissions)
            {
                context.RolePermission.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            var usuarioPermissions = permissions.
                Where(p => p.Nombre is "estudiante.ver")
                .ToList();

            foreach (var permission in usuarioPermissions)
            {
                context.RolePermission.Add(new RolePermission
                {
                    RoleId = usuarioRol.Id,
                    PermissionId = permission.Id
                });
            }
            await context.SaveChangesAsync();
            await SeedRolesDeportivosAsync(context);
        }

  
        private static async Task SeedRolesDeportivosAsync(AppDbContext context)
        {
            var porRol = new Dictionary<string, string[]>
            {
                ["Estudiante"] = ["portalestudiante.ver", "portalestudiante.inscribir"],
                ["Entrenador"] = ["portalentrenador.ver", "asistencia.registrar"]
            };

            foreach (var (rolNombre, permisos) in porRol)
            {
                var rol = await context.Role.FirstAsync(r => r.Descripcion == rolNombre);
                if (await context.RolePermission.AnyAsync(rp => rp.RoleId == rol.Id))
                    continue;

                var ids = await context.Permission
                    .Where(p => permisos.Contains(p.Nombre))
                    .Select(p => p.Id)
                    .ToListAsync();
                foreach (var id in ids)
                    context.RolePermission.Add(new RolePermission { RoleId = rol.Id, PermissionId = id });
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsersAsync(AppDbContext context)
        {
        
            var cuentas = new (string UserName, string Email, string Password, string Rol)[]
            {
                ("admin", "admin@gmail.com", "Admin123!", "Admin"),
                ("usuario", "usuario@gmail.com", "Usuario123!", "Usuario"),
                ("estudiante", "estudiante@gmail.com", "Estudiante123!", "Estudiante"),
                ("entrenador", "entrenador@gmail.com", "Entrenador123!", "Entrenador"),
            };

            var changed = false;
            foreach (var (userName, email, password, rolNombre) in cuentas)
            {
                if (await context.Usuario.AnyAsync(u => u.userName == userName))
                    continue;
                var rol = await context.Role.FirstAsync(r => r.Descripcion == rolNombre);
                context.Usuario.Add(new Usuario
                {
                    userName = userName,
                    Email = email,
                    Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password),
                    EmailConfirmed = true,
                    RoleId = rol.Id,
                });
                changed = true;
            }
            if (changed)
                await context.SaveChangesAsync();
        }

        private static async Task SeedFacultadAsync(AppDbContext context)
        {
            if (await context.Facultad.AnyAsync())
                return;
            context.Facultad.AddRange(
                new Facultad { Nombre = "Ciencias e Ingeniería", Descripcion = "Carreras técnicas y científicas" },
                new Facultad { Nombre = "Ciencias Económicas", Descripcion = "Administración y finanzas" },
                new Facultad { Nombre = "Humanidades", Descripcion = "Ciencias sociales y educación" }
                );
            await context.SaveChangesAsync();
        }

        private static async Task SeedCarreraAsync(AppDbContext context)
        {
            if (await context.Carrera.AnyAsync())
                return;

            var ingenieria = await context.Facultad.FirstAsync(f => f.Nombre == "Ciencias e Ingeniería");
            var economicas = await context.Facultad.FirstAsync(f => f.Nombre == "Ciencias Económicas");
            var humanidades = await context.Facultad.FirstAsync(f => f.Nombre == "Humanidades");

            context.Carrera.AddRange(
                new Carrera { Nombre = "Ingeniería en Sistemas", FacultadId = ingenieria.Id },
                new Carrera { Nombre = "Ingeniería Civil", FacultadId = ingenieria.Id },
                new Carrera { Nombre = "Administración de Empresas", FacultadId = economicas.Id },
                new Carrera { Nombre = "Psicología", FacultadId = humanidades.Id }
                );
            await context.SaveChangesAsync();
        }

       
        private static async Task SeedEstudianteAsync(AppDbContext context)
        {
            if (await context.Estudiante.AnyAsync())
                return;

            var usuario = await context.Usuario.FirstOrDefaultAsync(u => u.userName == "estudiante");
            var carrera = await context.Carrera.FirstOrDefaultAsync(c => c.Nombre == "Ingeniería en Sistemas");
            if (usuario == null || carrera == null)
                return;

            context.Estudiante.Add(new Estudiante
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Cedula = "001-010101-0001A",
                Carnet = "2026-0001",
                Correo = "juan.perez@gmail.com",
                Telefono = "88881111",
                CarreraId = carrera.Id,
                UsuarioId = usuario.Id
            });
            await context.SaveChangesAsync();
        }

        private static async Task SeedEntrenadorAsync(AppDbContext context)
        {
            if (await context.Entrenador.AnyAsync())
                return;

            var usuario = await context.Usuario.FirstOrDefaultAsync(u => u.userName == "entrenador");
            if (usuario == null)
                return;

            context.Entrenador.Add(new Entrenador
            {
                Nombre = "Carlos",
                Apellido = "Mendoza",
                Cedula = "001-020202-0002B",
                Carnet = "ENT-0001",
                Correo = "carlos.mendoza@gmail.com",
                Telefono = "88882222",
                UsuarioId = usuario.Id
            });
            await context.SaveChangesAsync();
        }

   
        private static async Task SeedCatalogosDeportivosAsync(AppDbContext context)
        {
            if (!await context.Deporte.AnyAsync())
            {
                context.Deporte.AddRange(
                    new Deporte { Nombre = "Fútbol", Descripcion = "Fútbol de campo", Estado = Estados.Activo },
                    new Deporte { Nombre = "Baloncesto", Descripcion = "Baloncesto en cancha techada", Estado = Estados.Activo },
                    new Deporte { Nombre = "Voleibol", Descripcion = "Voleibol de sala", Estado = Estados.Activo }
                    );
                await context.SaveChangesAsync();
            }

            if (!await context.CategoriaDeportiva.AnyAsync())
            {
                context.CategoriaDeportiva.AddRange(
                    new CategoriaDeportiva { Nombre = "Juvenil", EdadMinima = 16, EdadMaxima = 20 },
                    new CategoriaDeportiva { Nombre = "Libre", EdadMinima = 18, EdadMaxima = 35 }
                    );
                await context.SaveChangesAsync();
            }

            if (!await context.DeporteCategoria.AnyAsync())
            {
                var deportes = await context.Deporte.ToListAsync();
                var categorias = await context.CategoriaDeportiva.ToListAsync();
                foreach (var d in deportes)
                    foreach (var c in categorias)
                        context.DeporteCategoria.Add(new DeporteCategoria { DeporteId = d.Id, CategoriaDeportivaId = c.Id });
                await context.SaveChangesAsync();
            }

            if (!await context.Instalacion.AnyAsync())
            {
                context.Instalacion.AddRange(
                    new Instalacion { Nombre = "Cancha Principal", Ubicacion = "Campus central", Capacidad = 30, Estado = Estados.Activo },
                    new Instalacion { Nombre = "Gimnasio Techado", Ubicacion = "Edificio deportivo", Capacidad = 20, Estado = Estados.Activo }
                    );
                await context.SaveChangesAsync();
            }

           
            if (!await context.Periodo.AnyAsync())
            {
                var hoy = DateOnly.FromDateTime(DateTime.Now);
                var etiqueta = (DateOnly f) => $"{f.Year}-{(f.Month <= 6 ? "I" : "II")}";
                var inicioActual = hoy.AddMonths(-2);
                var finActual = hoy.AddMonths(3);
                var inicioAnterior = hoy.AddMonths(-8);
                var finAnterior = inicioActual.AddDays(-1);
                context.Periodo.AddRange(
                    new Periodo { Nombre = $"Semestre {etiqueta(inicioActual)}", FechaInicio = inicioActual, FechaFin = finActual, Estado = Estados.PeriodoAbierto },
                    new Periodo { Nombre = $"Semestre {etiqueta(inicioAnterior)}", FechaInicio = inicioAnterior, FechaFin = finAnterior, Estado = Estados.PeriodoCerrado }
                    );
                await context.SaveChangesAsync();
            }

            var entrenador = await context.Entrenador.FirstOrDefaultAsync();
            if (entrenador != null && !await context.EntrenadorDeporte.AnyAsync())
            {
                var futbol = await context.Deporte.FirstAsync(d => d.Nombre == "Fútbol");
                var baloncesto = await context.Deporte.FirstAsync(d => d.Nombre == "Baloncesto");
                context.EntrenadorDeporte.AddRange(
                    new EntrenadorDeporte { EntrenadorId = entrenador.Id, DeporteId = futbol.Id },
                    new EntrenadorDeporte { EntrenadorId = entrenador.Id, DeporteId = baloncesto.Id }
                    );
                await context.SaveChangesAsync();
            }

            if (!await context.Horario.AnyAsync() && await context.EntrenadorDeporte.AnyAsync())
            {
                var cancha = await context.Instalacion.FirstAsync(i => i.Nombre == "Cancha Principal");
                var gimnasio = await context.Instalacion.FirstAsync(i => i.Nombre == "Gimnasio Techado");
                var futbol = await context.EntrenadorDeporte.FirstAsync(ed => ed.Deporte.Nombre == "Fútbol");
                var baloncesto = await context.EntrenadorDeporte.FirstAsync(ed => ed.Deporte.Nombre == "Baloncesto");
                context.Horario.AddRange(
                    new Horario { Dia = "Lunes", HoraInicio = new TimeOnly(8, 0), HoraFin = new TimeOnly(9, 30), InstalacionId = cancha.Id, EntrenadorDeporteId = futbol.Id },
                    new Horario { Dia = "Miércoles", HoraInicio = new TimeOnly(8, 0), HoraFin = new TimeOnly(9, 30), InstalacionId = cancha.Id, EntrenadorDeporteId = futbol.Id },
                    new Horario { Dia = "Martes", HoraInicio = new TimeOnly(10, 0), HoraFin = new TimeOnly(11, 30), InstalacionId = gimnasio.Id, EntrenadorDeporteId = baloncesto.Id }
                    );
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedActividadAsync(AppDbContext context)
        {
            var estudiante = await context.Estudiante.FirstOrDefaultAsync();
            if (estudiante == null)
                return;

            if (!await context.Inscripcion.AnyAsync())
            {
                var periodo = await context.Periodo.FirstOrDefaultAsync(p => p.Estado == Estados.PeriodoAbierto);
                var futbol = await context.Deporte.FirstOrDefaultAsync(d => d.Nombre == "Fútbol");
                var categoria = await context.CategoriaDeportiva.FirstOrDefaultAsync(c => c.Nombre == "Libre");
                if (periodo != null && futbol != null && categoria != null)
                {
                    var deporteCategoria = await context.DeporteCategoria
                        .FirstOrDefaultAsync(dc => dc.DeporteId == futbol.Id && dc.CategoriaDeportivaId == categoria.Id);
                    var horarios = await context.Horario
                        .Where(h => h.EntrenadorDeporte.DeporteId == futbol.Id)
                        .ToListAsync();
                    if (deporteCategoria != null && horarios.Count > 0)
                    {
                        var inscripcion = new Inscripcion
                        {
                            Fecha = DateOnly.FromDateTime(DateTime.Now),
                            Estado = Estados.InscripcionActiva,
                            EstudianteId = estudiante.Id,
                            DeporteId = futbol.Id,
                            PeriodoId = periodo.Id,
                            DeporteCategoriaId = deporteCategoria.Id,
                            InscripcionHorario = horarios.Select(h => new InscripcionHorario { HorarioId = h.Id }).ToList()
                        };
                        
                        var lunes = DateOnly.FromDateTime(DateTime.Now);
                        while (lunes.DayOfWeek != DayOfWeek.Monday)
                            lunes = lunes.AddDays(-1);
                        if (lunes >= periodo.FechaInicio && horarios.Any(h => h.Dia == "Lunes"))
                            inscripcion.Asistencia.Add(new Asistencia { Fecha = lunes, Estado = Estados.Presente });
                        context.Inscripcion.Add(inscripcion);
                        await context.SaveChangesAsync();
                    }
                }
            }

            if (!await context.EvaluacionFisica.AnyAsync())
            {
                context.EvaluacionFisica.Add(new EvaluacionFisica
                {
                    EstudianteId = estudiante.Id,
                    Fecha = DateOnly.FromDateTime(DateTime.Now),
                    Peso = 70.5m,
                    Estatura = 1.75m,
                    Observaciones = "Evaluación inicial de ejemplo"
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
