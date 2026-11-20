//Funciones compartidas por las pantallas del sistema

function escapeHtml(value) {
    return String(value ?? '').replace(/[&<>"']/g, c => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[c]));
}

//El controller responde 'message' (validacion del modelo) o 'errorMessage' (regla de negocio)
function getErrorMessage(error, fallback) {
    return error?.response?.data?.message || error?.response?.data?.errorMessage || fallback;
}

//Insignia de color segun el estado
function estadoBadge(estado) {
    const clases = {
        'Activo': 'bg-success', 'Abierto': 'bg-success', 'Activa': 'bg-success', 'Presente': 'bg-success',
        'Finalizada': 'bg-primary',
        'Justificado': 'bg-warning text-dark',
        'Inactivo': 'bg-secondary', 'Cerrado': 'bg-secondary',
        'Cancelada': 'bg-danger', 'Ausente': 'bg-danger'
    };
    return `<span class="badge ${clases[estado] || 'bg-secondary'}">${escapeHtml(estado)}</span>`;
}

//'08:00:00' -> '08:00'
function horaCorta(valor) {
    return valor ? String(valor).substring(0, 5) : '';
}

//'08:00' -> '08:00:00' (formato que acepta TimeOnly en el servidor)
function horaCompleta(valor) {
    return valor && valor.length === 5 ? valor + ':00' : valor;
}
