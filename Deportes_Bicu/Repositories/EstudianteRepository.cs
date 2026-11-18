using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Data;
using Deportes_Bicu.DTOS;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Repositories
{
    public class EstudianteRepository(AppDbContext context) : IEstudianteRepository
    {
        public async Task<Estudiante> CreateAsync(Estudiante estudiante)
        {
            var entry = await context.Estudiante.AddAsync(estudiante);
            await context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var estudiante = await context.Estudiante.FindAsync(id);
            if (estudiante == null) return false;

            context.Estudiante.Remove(estudiante);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CorreoExisteAsync(string correo, int? excludeId = null)
        {
            return await context.Estudiante.AnyAsync(e => e.Correo == correo && (!excludeId.HasValue || e.Id != excludeId.Value));
        }

        public async Task<bool> CodigoExisteAsync(string codigo, int usuarioId, int? excludeId = null)
        {
            var enEstudiante = await context.Estudiante.AnyAsync(e => e.Carnet == codigo
                && (!excludeId.HasValue || e.Id != excludeId.Value));
            if (enEstudiante)
                return true;

            return await context.Entrenador.AnyAsync(t => t.Carnet == codigo && t.UsuarioId != usuarioId);
        }

        public async Task<Estudiante?> GetByIdAsync(int Id)
        {
            return await context.Estudiante.Include(e => e.Carrera).FirstOrDefaultAsync(e => e.Id == Id);
        }

        public async Task<IReadOnlyList<UsuarioSelectDto>> GetUsuariosDisponiblesAsync(int? incluirUsuarioId = null)
        {
            return await context.Usuario
                .Where(u => !context.Estudiante.Any(e => e.UsuarioId == u.Id)
                    || (incluirUsuarioId.HasValue && u.Id == incluirUsuarioId.Value))
                .OrderBy(u => u.userName)
                .Select(u => new UsuarioSelectDto
                {
                    Id = u.Id,
                    UserName = u.userName,
                    Email = u.Email
                }).ToListAsync();
        }

        public async Task<int?> GetIdPorUsuarioAsync(int usuarioId)
        {
            return await context.Estudiante
                .Where(e => e.UsuarioId == usuarioId)
                .Select(e => (int?)e.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<EstudianteTableDto?> GetPerfilAsync(int id)
        {
            return await context.Estudiante
                .Where(e => e.Id == id)
                .Select(e => new EstudianteTableDto
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    Apellido = e.Apellido,
                    Carnet = e.Carnet,
                    Correo = e.Correo,
                    CarreraId = e.CarreraId,
                    CarreraNombre = e.Carrera.Nombre,
                    FacultadNombre = e.Carrera.Facultad.Nombre,
                    UsuarioId = e.UsuarioId,
                    UserName = e.Usuario.userName
                }).FirstOrDefaultAsync();
        }

        public async Task<DataTableResponseDto<EstudianteTableDto>> GetPagedAsync(DataTableRequestDto request)
        {
            var query = context.Estudiante
                .Include(e => e.Carrera).ThenInclude(c => c.Facultad)
                .Include(e => e.Usuario)
                .AsQueryable();
            var recordsTotal = await query.CountAsync();
            var search = request.Search?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Nombre.Contains(search)
                || e.Apellido.Contains(search) || e.Carnet.Contains(search) || e.Correo.Contains(search)
                || e.Carrera.Nombre.Contains(search) || e.Carrera.Facultad.Nombre.Contains(search)
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
                "correo" => orderDir == "desc" ?
                query.OrderByDescending(c => c.Correo) : query.OrderBy(c => c.Correo),
                "carreranombre" => orderDir == "desc" ? query.OrderByDescending(c => c.Carrera.Nombre) : query.OrderBy(c => c.Carrera.Nombre),
                "username" => orderDir == "desc" ? query.OrderByDescending(c => c.Usuario.userName) : query.OrderBy(c => c.Usuario.userName),
                "id" => orderDir == "desc" ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id),
                _ => orderDir == "desc" ? query.OrderByDescending(c => c.Nombre) : query.OrderBy(c => c.Nombre)
            };

            var data = await query
                .Skip(request.Start)
                .Take(request.Length > 0 ? request.Length : 10)
                .Select(c => new EstudianteTableDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Apellido = c.Apellido,
                    Carnet = c.Carnet,
                    Correo = c.Correo,
                    CarreraId = c.CarreraId,
                    CarreraNombre = c.Carrera.Nombre,
                    FacultadNombre = c.Carrera.Facultad.Nombre,
                    UsuarioId = c.UsuarioId,
                    UserName = c.Usuario.userName
                }).ToListAsync();

            return new DataTableResponseDto<EstudianteTableDto>
            {
                Draw = request.Draw,
                RecordsTotal = recordsTotal,
                RecordsFiltered = recordsFiltered,
                Data = data
            };
        }

        public async Task<bool> HasRelatedRecordsAsync(int id)
        {
            return await context.Inscripcion.AnyAsync(i => i.EstudianteId == id)
                || await context.EvaluacionFisica.AnyAsync(ev => ev.EstudianteId == id);
        }

        public async Task<bool> UpdateAsync(Estudiante estudiante)
        {
            context.Estudiante.Update(estudiante);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsuarioExisteAsync(int usuarioId)
        {
            return await context.Usuario.AnyAsync(u => u.Id == usuarioId);
        }

        public async Task<bool> UsuarioTienePerfilAsync(int usuarioId, int? excludeId = null)
        {
            return await context.Estudiante.AnyAsync(e => e.UsuarioId == usuarioId
                && (!excludeId.HasValue || e.Id != excludeId.Value));
        }
    }
}
