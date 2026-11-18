using Microsoft.EntityFrameworkCore;
using Deportes_Bicu.Models;

namespace Deportes_Bicu.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext>options):DbContext(options)
    {
        public DbSet<Permission> Permission { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<RolePermission> RolePermission { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<UsuarioPermission> UsuarioPermission { get; set; }


        public DbSet<Facultad> Facultad { get; set; }
        public DbSet<Carrera> Carrera { get; set; }
        public DbSet<Estudiante> Estudiante { get; set; }
        public DbSet<Entrenador> Entrenador { get; set; }
        public DbSet<Deporte> Deporte { get; set; }
        public DbSet<CategoriaDeportiva> CategoriaDeportiva { get; set; }
        public DbSet<DeporteCategoria> DeporteCategoria { get; set; }
        public DbSet<EntrenadorDeporte> EntrenadorDeporte { get; set; }
        public DbSet<Instalacion> Instalacion { get; set; }
        public DbSet<Periodo> Periodo { get; set; }
        public DbSet<Horario> Horario { get; set; }
        public DbSet<Inscripcion> Inscripcion { get; set; }
        public DbSet<InscripcionHorario> InscripcionHorario { get; set; }
        public DbSet<EvaluacionFisica> EvaluacionFisica { get; set; }
        public DbSet<Asistencia> Asistencia { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermission)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(r => r.RolePermission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsuarioPermission>()
                .HasKey(up => new { up.UsuarioId, up.PermissionId });

            modelBuilder.Entity<UsuarioPermission>()
                .HasOne(up => up.Usuario)
                .WithMany(u => u.UsuarioPermission)
                .HasForeignKey(up => up.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UsuarioPermission>()
                .HasOne(up => up.Permission)
                .WithMany(u => u.UsuarioPermission)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<Facultad>(e =>
            {
                e.Property(f => f.Nombre).HasMaxLength(100).IsRequired();
                e.Property(f => f.Descripcion).HasMaxLength(200);
                e.HasIndex(f => f.Nombre).IsUnique();
            });

            modelBuilder.Entity<Carrera>(e =>
            {
                e.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
                e.HasIndex(c => new { c.FacultadId, c.Nombre }).IsUnique();
            });

            modelBuilder.Entity<Carrera>()
                .HasOne(c => c.Facultad)
                .WithMany(f => f.Carrera)
                .HasForeignKey(c => c.FacultadId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Estudiante>(e =>
            {
                e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
                e.Property(x => x.Apellido).HasMaxLength(50).IsRequired();
                e.Property(x => x.Cedula).HasMaxLength(20).IsRequired();
                e.Property(x => x.Carnet).HasMaxLength(30).IsRequired();
                e.Property(x => x.Correo).HasMaxLength(100).IsRequired();
                e.Property(x => x.Telefono).HasMaxLength(20);
                e.HasIndex(x => x.Carnet).IsUnique();
                e.HasIndex(x => x.Correo).IsUnique();
            });

            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Carrera)
                .WithMany(c => c.Estudiante)
                .HasForeignKey(e => e.CarreraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Estudiante>()
                .HasOne(e => e.Usuario)
                .WithOne()
                .HasForeignKey<Estudiante>(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Entrenador>(e =>
            {
                e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
                e.Property(x => x.Apellido).HasMaxLength(50).IsRequired();
                e.Property(x => x.Cedula).HasMaxLength(20).IsRequired();
                e.Property(x => x.Carnet).HasMaxLength(30).IsRequired();
                e.Property(x => x.Telefono).HasMaxLength(20);
                e.Property(x => x.Correo).HasMaxLength(100).IsRequired();
                e.HasIndex(x => x.Carnet).IsUnique();
                e.HasIndex(x => x.Correo).IsUnique();
            });

            modelBuilder.Entity<Entrenador>()
                .HasOne(e => e.Usuario)
                .WithOne()
                .HasForeignKey<Entrenador>(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

     
            modelBuilder.Entity<Deporte>(e =>
            {
                e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
                e.Property(x => x.Descripcion).HasMaxLength(200);
                e.Property(x => x.Categoria).HasMaxLength(50);
                e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            modelBuilder.Entity<CategoriaDeportiva>(e =>
            {
                e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            modelBuilder.Entity<DeporteCategoria>()
                .HasOne(dc => dc.Deporte)
                .WithMany(d => d.DeporteCategoria)
                .HasForeignKey(dc => dc.DeporteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DeporteCategoria>()
                .HasOne(dc => dc.CategoriaDeportiva)
                .WithMany(c => c.DeporteCategoria)
                .HasForeignKey(dc => dc.CategoriaDeportivaId)
                .OnDelete(DeleteBehavior.Restrict);

     
            modelBuilder.Entity<DeporteCategoria>()
                .HasIndex(dc => new { dc.DeporteId, dc.CategoriaDeportivaId })
                .IsUnique();

            modelBuilder.Entity<EntrenadorDeporte>()
                .HasOne(ed => ed.Entrenador)
                .WithMany(e => e.EntrenadorDeporte)
                .HasForeignKey(ed => ed.EntrenadorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EntrenadorDeporte>()
                .HasOne(ed => ed.Deporte)
                .WithMany(d => d.EntrenadorDeporte)
                .HasForeignKey(ed => ed.DeporteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EntrenadorDeporte>()
                .HasIndex(ed => new { ed.EntrenadorId, ed.DeporteId })
                .IsUnique();

            modelBuilder.Entity<Instalacion>(e =>
            {
                e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
                e.Property(x => x.Ubicacion).HasMaxLength(150);
                e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            modelBuilder.Entity<Periodo>(e =>
            {
                e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
                e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
                e.HasIndex(x => x.Nombre).IsUnique();
            });

            modelBuilder.Entity<Horario>()
                .Property(h => h.Dia).HasMaxLength(20).IsRequired();

            modelBuilder.Entity<Horario>()
                .HasOne(h => h.Instalacion)
                .WithMany(i => i.Horario)
                .HasForeignKey(h => h.InstalacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Horario>()
                .HasOne(h => h.EntrenadorDeporte)
                .WithMany(ed => ed.Horario)
                .HasForeignKey(h => h.EntrenadorDeporteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inscripcion>()
                .Property(i => i.Estado).HasMaxLength(20).IsRequired();

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Estudiante)
                .WithMany(e => e.Inscripcion)
                .HasForeignKey(i => i.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Deporte)
                .WithMany(d => d.Inscripcion)
                .HasForeignKey(i => i.DeporteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.Periodo)
                .WithMany(p => p.Inscripcion)
                .HasForeignKey(i => i.PeriodoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inscripcion>()
                .HasOne(i => i.DeporteCategoria)
                .WithMany(dc => dc.Inscripcion)
                .HasForeignKey(i => i.DeporteCategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inscripcion>()
                .HasIndex(i => new { i.EstudianteId, i.DeporteId, i.PeriodoId })
                .IsUnique();

            modelBuilder.Entity<InscripcionHorario>()
                .HasOne(ih => ih.Inscripcion)
                .WithMany(i => i.InscripcionHorario)
                .HasForeignKey(ih => ih.InscripcionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InscripcionHorario>()
                .HasOne(ih => ih.Horario)
                .WithMany(h => h.InscripcionHorario)
                .HasForeignKey(ih => ih.HorarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InscripcionHorario>()
                .HasIndex(ih => new { ih.InscripcionId, ih.HorarioId })
                .IsUnique();

            modelBuilder.Entity<Asistencia>()
                .Property(a => a.Estado).HasMaxLength(20).IsRequired();

            modelBuilder.Entity<Asistencia>()
                .HasOne(a => a.Inscripcion)
                .WithMany(i => i.Asistencia)
                .HasForeignKey(a => a.InscripcionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asistencia>()
                .HasIndex(a => new { a.InscripcionId, a.Fecha })
                .IsUnique();

            modelBuilder.Entity<EvaluacionFisica>(e =>
            {
                e.Property(x => x.Peso).HasPrecision(5, 2);
                e.Property(x => x.Estatura).HasPrecision(4, 2);
                e.Property(x => x.Observaciones).HasMaxLength(250);
            });

            modelBuilder.Entity<EvaluacionFisica>()
                .HasOne(ev => ev.Estudiante)
                .WithMany(e => e.EvaluacionFisica)
                .HasForeignKey(ev => ev.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
