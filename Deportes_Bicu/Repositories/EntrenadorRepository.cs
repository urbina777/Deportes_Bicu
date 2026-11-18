using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Helper;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class EntrenadorRepository(AppDbContext context) : IEntrenadorRepository
    {
        public async Task<Entrenador> CreateAsync(Entrenador entrenador)
        {
            var entry = await context.Entrenador.AddAsync(entrenador);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entrenador = await context.Entrenador.FindAsync(id);
            if (entrenador == null) return false;

            context.Entrenador.Remove(entrenador);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CorreoExisteAsync(string correo, int? excludeId = null)
        {
            return await context.Entrenador.AnyAsync(e => e.Correo == correo && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        public async Task<bool> CodigoExisteAsync(string codigo, int usuarioId, int? excludeId = null)
        {
            var enEntrenador = await context.Entrenador.AnyAsync(e => e.Carnet == codigo
                && (!excludeId.HasValue || e.Id != excludeId.Value));
            if (enEntrenador)
                return true;

            return await context.Estudiante.AnyAsync(s => s.Carnet == codigo && s.UsuarioId != usuarioId);
        }

        public async Task<Entrenador?> GetByIdAsync(int Id)
        {
            return await context.Entrenador.FindAsync(Id);
        }

        public async Task<int?> GetIdPorUsuarioAsync(int usuarioId)
        {
            return await context.Entrenador
                .Where(e => e.UsuarioId == usuarioId)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<UsuarioSelectDto>> GetUsuariosDisponiblesAsync(int? incluirUsuarioId = null)
        {
            return await context.Usuario
                .Where(u => !context.Entrenador.Any(e => e.UsuarioId == u.Id)
                    || (incluirUsuarioId.HasValue && u.Id == incluirUsuarioId.Value))
                .OrderBy(u => u.userName)
                .Select(u => new UsuarioSelectDto
                {
                    Id = u.Id,
                    UserName = u.userName,
                    Email = u.Email
                }).ToListAsync();
        }

        public async Task<DataTableResponseDto<EntrenadorTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Entrenador.Include(e => e.Usuario).AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Nombre.Contains(search)
                || e.Apellido.Contains(search) || e.Carnet.Contains(search) || e.Correo.Contains(search)
                || e.Usuario.userName.Contains(search));
            }

            var recordsFiltered = await query.CountAsync();
            var orderColumn = request.Order.FirstOrDefault()?.Column ?? 0;
            var orderDir = request.Order.FirstOrDefault()?.Dir ?? "asc";
            var columnName = request.Columns.ElementAtOrDefault(orderColumn)?.Data ?? "nombre";

            query = columnName.ToLowerInvariant() switch
            {
                "apellido" => orderDir == "desc" ? query.OrderByDescending(c => c.Apellido) : query.OrderBy(c => c.Apellido),
                "carnet" => orderDir == "desc" ? query.OrderByDescending(c => c.Carnet) : query.OrderBy(c => c.Carnet),
                "correo" => orderDir == "desc" ? query.OrderByDescending(c => c.Correo) : query.OrderBy(c => c.Correo),
                "username" => orderDir == "desc" ? query.OrderByDescending(c => c.Usuario.userName) : query.OrderBy(c => c.Usuario.userName),
                "totaldeportes" => orderDir == "desc" ? query.OrderByDescending(c => c.EntrenadorDeporte.Count) : query.OrderBy(c => c.EntrenadorDeporte.Count),
                "id" => orderDir == "desc" ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(c => c.Nombre) : query.OrderBy(c => c.Nombre)
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(c => new EntrenadorTableDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido,
                    Carnet = c.Carnet,
                    Correo = c.Correo,
                    UsuarioId = c.UsuarioId,
                    UserName = c.Usuario.userName,
                    TotalDeportes = c.EntrenadorDeporte.Count
                }).ToListAsync();

            return new DataTableResponseDto<EntrenadorTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> HasRelatedRecordsAsync(int id)
        {
            return await context.EntrenadorDeporte.AnyAsync(ed => ed.EntrenadorId == id);
        }

        public async Task<bool> UpdateAsync(Entrenador entrenador)
        {
            context.Entrenador.Update(entrenador);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsuarioExisteAsync(int usuarioId)
        {
            return await context.Usuario.AnyAsync(u => u.Id == usuarioId);
        }

        public async Task<bool> UsuarioTienePerfilAsync(int usuarioId, int? excludeId = null)
        {
            return await context.Entrenador.AnyAsync(e => e.UsuarioId == usuarioId
                && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        public async Task<AsignacionesDto> GetDeportesAsync(int entrenadorId)
        {
            var asignadas = await context.EntrenadorDeporte
                .Where(ed => ed.EntrenadorId == entrenadorId)
                .OrderBy(ed => ed.Deporte.Nombre)
                .Select(ed => new { ed.Id, ed.Deporte.Nombre, ed.Deporte.Estado })
                .ToListAsync();

            var disponibles = await context.Deporte
                .Where(d => d.Estado == Estados.Activo
                    && !context.EntrenadorDeporte.Any(ed => ed.EntrenadorId == entrenadorId && ed.DeporteId == d.Id))
                .OrderBy(d => d.Nombre)
                .Select(d => new OpcionDto { Id = d.Id, Texto = d.Nombre })
                .ToListAsync();

            return new AsignacionesDto
            {
                Asignadas = asignadas.Select(a => new AsignacionDto { Id = a.Id, Texto = a.Nombre, Detalle = a.Estado }).ToList(),
                Disponibles = disponibles
            };
        }

        public async Task<bool> DeporteActivoAsync(int deporteId)
        {
            return await context.Deporte.AnyAsync(d => d.Id == deporteId && d.Estado == Estados.Activo);
        }

        public async Task<bool> DeporteAsignadoAsync(int entrenadorId, int deporteId)
        {
            return await context.EntrenadorDeporte.AnyAsync(ed => ed.EntrenadorId == entrenadorId && ed.DeporteId == deporteId);
        }

        public async Task AsignarDeporteAsync(int entrenadorId, int deporteId)
        {
            context.EntrenadorDeporte.Add(new EntrenadorDeporte { EntrenadorId = entrenadorId, DeporteId = deporteId });
            await context.SaveChangesAsync();
        }

        public async Task<bool> AsignacionTieneHorariosAsync(int entrenadorDeporteId)
        {
            return await context.Horario.AnyAsync(h => h.EntrenadorDeporteId == entrenadorDeporteId);
        }

        public async Task<bool> QuitarDeporteAsync(int entrenadorDeporteId)
        {
            var ed = await context.EntrenadorDeporte.FindAsync(entrenadorDeporteId);
            if (ed == null) return false;
            context.EntrenadorDeporte.Remove(ed);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
