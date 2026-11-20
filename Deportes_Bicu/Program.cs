using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Deportes_Bicu.Data;
using Deportes_Bicu.Extensions;
using Deportes_Bicu.Mapping;
using Deportes_Bicu.Middleware;
using Deportes_Bicu.Models;
using Deportes_Bicu.Models.Settings;
using Deportes_Bicu.Repositories;
using Deportes_Bicu.Service;
using System.Text;
using static Deportes_Bicu.Authorization.JwtPermissionPolicies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<ReglasInscripcionSettings>(builder.Configuration.GetSection("ReglasInscripcion"));

var jwtSetting =
    builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("No esta presente la configuración del JWT");

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(UserProfile).Assembly));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<IFacultadService, FacultadService>();
builder.Services.AddScoped<ICarreraService, CarreraService>();
builder.Services.AddScoped<IDeporteService, DeporteService>();
builder.Services.AddScoped<ICategoriaDeportivaService, CategoriaDeportivaService>();
builder.Services.AddScoped<IInstalacionService, InstalacionService>();
builder.Services.AddScoped<IPeriodoService, PeriodoService>();
builder.Services.AddScoped<IEntrenadorService, EntrenadorService>();
builder.Services.AddScoped<IHorarioService, HorarioService>();
builder.Services.AddScoped<IInscripcionService, InscripcionService>();
builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();
builder.Services.AddScoped<IEvaluacionFisicaService, EvaluacionFisicaService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRoleRepository, RolRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IEstudianteRepository, EstudianteRepository>();
builder.Services.AddScoped<IFacultadRepository, FacultadRepository>();
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IDeporteRepository, DeporteRepository>();
builder.Services.AddScoped<ICategoriaDeportivaRepository, CategoriaDeportivaRepository>();
builder.Services.AddScoped<IInstalacionRepository, InstalacionRepository>();
builder.Services.AddScoped<IPeriodoRepository, PeriodoRepository>();
builder.Services.AddScoped<IEntrenadorRepository, EntrenadorRepository>();
builder.Services.AddScoped<IHorarioRepository, HorarioRepository>();
builder.Services.AddScoped<IInscripcionRepository, InscripcionRepository>();
builder.Services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
builder.Services.AddScoped<IEvaluacionFisicaRepository, EvaluacionFisicaRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();



//============================ DbContext ===========

builder.Services.AddDbContext<AppDbContext>(options => 
options.UseSqlServer(builder.Configuration.GetConnectionString("ProyectoAuth")));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("access_token"))
                {
                    context.Token =
                        context.Request.Cookies["access_token"];
                }
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                context.HandleResponse();
                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/Auth/Login");
                }

                return Task.CompletedTask;
            },

            OnForbidden = context =>
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/Auth/AccessDenied");
                }

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters =
        new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSetting.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSetting.Audience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSetting.Key)),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddAuthorization(options =>
{
options.AddPolicy("admin.panel", policy =>
    policy.RequireAssertion(context =>
    context.User.HasPermision("rol.ver") ||
    context.User.HasPermision("permiso.ver") ||
    context.User.HasPermision("usuario.ver")));

options.AddPolicy("permiso.actualizar", policy =>
    policy.RequireAssertion(context =>
        context.User.HasAnyPermission("permiso.actualizar", "permiso.crear")));

options.AddPolicy("permiso.borrar", policy =>
    policy.RequireAssertion(context =>
            context.User.HasAnyPermission("permiso.borrar", "permiso.crear")));


options.AddPolicy("estudiante.usuarios", policy =>
    policy.RequireAssertion(context =>
        context.User.HasAnyPermission("estudiante.crear", "estudiante.actualizar")));


options.AddPolicy("entrenador.usuarios", policy =>
    policy.RequireAssertion(context =>
        context.User.HasAnyPermission("entrenador.crear", "entrenador.actualizar")));


options.AddPolicy("inscripcion.opciones", policy =>
    policy.RequireAssertion(context =>
        context.User.HasAnyPermission("inscripcion.crear", "portalestudiante.inscribir")));


options.AddPolicy("evaluacion.opciones", policy =>
    policy.RequireAssertion(context =>
        context.User.HasAnyPermission("evaluacion.crear", "evaluacion.actualizar")));
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
 
app.UseHttpsRedirection();
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if(response.StatusCode == 404 && !response.HasStarted)
    {
        response.Redirect("/Home/NotFoundPage");
    }

    await Task.CompletedTask;
});
app.UseRouting();
app.UseSession();
app.UseMiddleware<JwtRefreshMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
