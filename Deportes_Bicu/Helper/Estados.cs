namespace Deportes_Bicu.Helper
{
    public static class Estados
    {
        public const string Activo = "Activo";
        public const string Inactivo = "Inactivo";

        public const string PeriodoAbierto = "Abierto";
        public const string PeriodoCerrado = "Cerrado";

        public const string InscripcionActiva = "Activa";
        public const string InscripcionFinalizada = "Finalizada";
        public const string InscripcionCancelada = "Cancelada";

        public const string Presente = "Presente";
        public const string Ausente = "Ausente";
        public const string Justificado = "Justificado";

        public static readonly string[] Deporte = [Activo, Inactivo];
        public static readonly string[] Instalacion = [Activo, Inactivo];
        public static readonly string[] Periodo = [PeriodoAbierto, PeriodoCerrado];
        public static readonly string[] Inscripcion = [InscripcionActiva, InscripcionFinalizada, InscripcionCancelada];
        public static readonly string[] Asistencia = [Presente, Ausente, Justificado];
        public static readonly string[] Dias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

        public static string DiaDeSemana(DateOnly fecha) => fecha.DayOfWeek switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miércoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sábado",
            _ => "Domingo"
        };

        public static bool Solapan(string diaA, TimeOnly inicioA, TimeOnly finA, string diaB, TimeOnly inicioB, TimeOnly finB)
            => diaA == diaB && inicioA < finB && inicioB < finA;
    }
}
