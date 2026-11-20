$(function () {
    const config = window.portalEntrenadorConfig || {};
    const idioma = { url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json' };
    let horarios = [];

    function hoy() {
        const d = new Date();
        return formatear(d);
    }
    function formatear(d) {
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }

    //Fecha mas reciente (hoy o anterior) que cae en el dia de la semana del horario
    const diasJs = { 'Domingo': 0, 'Lunes': 1, 'Martes': 2, 'Miércoles': 3, 'Jueves': 4, 'Viernes': 5, 'Sábado': 6 };
    function ultimaFechaDeDia(dia) {
        const d = new Date();
        while (d.getDay() !== diasJs[dia]) d.setDate(d.getDate() - 1);
        return formatear(d);
    }

    axios.get('/PortalEntrenador/GetMisDatos').then(r => {
        const p = r.data;
        const deportes = p.deportes.length
            ? p.deportes.map(d => `<span class="badge bg-primary me-1">${escapeHtml(d.texto)}</span>`).join('')
            : '<span class="text-muted">Sin deportes asignados</span>';
        $('#miPerfil').html(`<div class="row">
            <div class="col-md-4"><strong>${escapeHtml(p.nombre)} ${escapeHtml(p.apellido)}</strong><br><small class="text-muted">Código ${escapeHtml(p.carnet)}</small></div>
            <div class="col-md-4"><span class="text-muted">Correo</span><br>${escapeHtml(p.correo)}</div>
            <div class="col-md-4"><span class="text-muted">Deportes</span><br>${deportes}</div>
        </div>`);
    }).catch(error => {
        $('#miPerfil').html(`<span class="text-danger">${escapeHtml(getErrorMessage(error, 'No se pudieron cargar sus datos'))}</span>`);
    });

    $('#misHorariosTable').DataTable({
        ajax: {
            url: '/PortalEntrenador/GetMisHorarios',
            dataSrc: function (data) {
                horarios = data;
                const select = $('#sesionHorario');
                if (select.length) {
                    select.empty().append($('<option>').val('').text('Seleccione un horario'));
                    data.forEach(h => select.append($('<option>').val(h.id)
                        .text(`${h.dia} ${horaCorta(h.horaInicio)}-${horaCorta(h.horaFin)} · ${h.deporteNombre} · ${h.instalacionNombre}`)));
                }
                return data;
            }
        },
        columns: [
            { data: 'dia' },
            { data: 'horaInicio', render: d => horaCorta(d) },
            { data: 'horaFin', render: d => horaCorta(d) },
            { data: 'deporteNombre' },
            { data: 'instalacionNombre' }
        ],
        language: idioma,
        ordering: false
    });

    $('#historialTable').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/PortalEntrenador/GetHistorial',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'fecha' },
            { data: 'estudianteNombre', render: (d, t, row) => `${escapeHtml(d)}<br><small class="text-muted">${escapeHtml(row.carnet)}</small>` },
            { data: 'deporteNombre' },
            { data: 'periodoNombre' },
            { data: 'estado', render: d => estadoBadge(d) }
        ],
        language: idioma,
        order: [[0, 'desc']]
    });

    if (!config.canRegistrar) return;

    $('#sesionFecha').attr('max', hoy());
    $('#sesionHorario').on('change', function () {
        const h = horarios.find(x => x.id === parseInt($(this).val()));
        if (h) $('#sesionFecha').val(ultimaFechaDeDia(h.dia));
        $('#sesionLista').addClass('d-none');
    });
    $('#sesionFecha').on('change', () => $('#sesionLista').addClass('d-none'));

    $('#btnCargarLista').on('click', async function () {
        const horarioId = $('#sesionHorario').val();
        const fecha = $('#sesionFecha').val();
        if (!horarioId || !fecha) {
            Swal.fire('Atención', 'Seleccione el horario y la fecha.', 'warning');
            return;
        }
        try {
            const response = await axios.get(`/PortalEntrenador/GetInscritos?horarioId=${horarioId}&fecha=${fecha}`);
            const tbody = $('#sesionInscritos').empty();
            if (response.data.length === 0) {
                tbody.append('<tr><td colspan="3" class="text-muted">No hay estudiantes inscritos en este horario para esa fecha.</td></tr>');
                $('#btnGuardarSesion').prop('disabled', true);
            } else {
                $('#btnGuardarSesion').prop('disabled', false);
            }
            response.data.forEach(i => {
                const opciones = ['<option value="">Sin registrar</option>']
                    .concat(config.estados.map(e => `<option value="${e}" ${i.estado === e ? 'selected' : ''}>${e}</option>`))
                    .join('');
                tbody.append(`<tr data-inscripcion="${i.inscripcionId}">
                    <td>${escapeHtml(i.estudianteNombre)}</td><td>${escapeHtml(i.carnet)}</td>
                    <td><select class="form-select form-select-sm sesion-estado">${opciones}</select></td></tr>`);
            });
            $('#sesionLista').removeClass('d-none');
        } catch (error) {
            $('#sesionLista').addClass('d-none');
            Swal.fire('Error', getErrorMessage(error, 'No se pudo cargar la lista'), 'error');
        }
    });

    $('#btnGuardarSesion').on('click', async function () {
        const registros = [];
        $('#sesionInscritos tr[data-inscripcion]').each(function () {
            const estado = $(this).find('.sesion-estado').val();
            if (estado) registros.push({ inscripcionId: parseInt($(this).data('inscripcion')), estado: estado });
        });
        if (registros.length === 0) {
            Swal.fire('Atención', 'Marque al menos un estudiante.', 'warning');
            return;
        }
        try {
            const response = await axios.post('/PortalEntrenador/GuardarAsistencia', {
                horarioId: parseInt($('#sesionHorario').val()),
                fecha: $('#sesionFecha').val(),
                registros: registros
            });
            if (response.data.success) {
                $('#historialTable').DataTable().ajax.reload(null, false);
                Swal.fire({ title: 'Listo', text: response.data.message, icon: 'success', timer: 2000, showConfirmButton: false });
            }
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo guardar la asistencia'), 'error');
        }
    });
});
