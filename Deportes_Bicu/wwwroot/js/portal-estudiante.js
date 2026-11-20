$(function () {
    const config = window.portalEstudianteConfig || {};
    const idioma = { url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json' };

    axios.get('/PortalEstudiante/GetMisDatos').then(r => {
        const p = r.data;
        $('#miPerfil').html(`<div class="row">
            <div class="col-md-4"><strong>${escapeHtml(p.nombre)} ${escapeHtml(p.apellido)}</strong><br><small class="text-muted">Carnet ${escapeHtml(p.carnet)}</small></div>
            <div class="col-md-4"><span class="text-muted">Carrera</span><br>${escapeHtml(p.carreraNombre)}</div>
            <div class="col-md-4"><span class="text-muted">Facultad</span><br>${escapeHtml(p.facultadNombre)}</div>
        </div>`);
    }).catch(error => {
        $('#miPerfil').html(`<span class="text-danger">${escapeHtml(getErrorMessage(error, 'No se pudieron cargar sus datos'))}</span>`);
    });

    const inscripciones = $('#misInscripcionesTable').DataTable({
        ajax: { url: '/PortalEstudiante/GetMisInscripciones', dataSrc: '' },
        columns: [
            { data: 'fecha' },
            { data: 'deporteNombre' },
            { data: 'categoriaNombre' },
            { data: 'periodoNombre' },
            {
                data: 'horarios',
                orderable: false,
                render: horarios => horarios.map(h =>
                    `<div class="small">${escapeHtml(h.dia)} ${horaCorta(h.horaInicio)}-${horaCorta(h.horaFin)} · ${escapeHtml(h.instalacionNombre)} · ${escapeHtml(h.entrenadorNombre)}</div>`
                ).join('')
            },
            { data: 'totalAsistencias' },
            { data: 'estado', render: d => estadoBadge(d) },
            {
                data: null,
                orderable: false,
                render: function (row) {
                    if (!config.canInscribir || row.estado !== 'Activa') return '';
                    return `<button class="btn btn-sm btn-outline-danger btn-cancelar" data-id="${row.id}"
                        data-nombre="${escapeHtml(row.deporteNombre + ' - ' + row.periodoNombre)}">Cancelar</button>`;
                }
            }
        ],
        language: idioma,
        order: [[0, 'desc']]
    });

    $('#misAsistenciasTable').DataTable({
        ajax: { url: '/PortalEstudiante/GetMisAsistencias', dataSrc: '' },
        columns: [
            { data: 'fecha' },
            { data: 'deporteNombre' },
            { data: 'periodoNombre' },
            { data: 'estado', render: d => estadoBadge(d) }
        ],
        language: idioma,
        order: [[0, 'desc']]
    });

    $('#misEvaluacionesTable').DataTable({
        ajax: { url: '/PortalEstudiante/GetMisEvaluaciones', dataSrc: '' },
        columns: [
            { data: 'fecha' },
            { data: 'peso' },
            { data: 'estatura' },
            { data: 'observaciones', render: d => escapeHtml(d ?? '') }
        ],
        language: idioma,
        order: [[0, 'desc']]
    });

    if (config.canInscribir) {
        InscripcionForm.init({
            urls: {
                opciones: '/Inscripcion/GetOpciones',
                categorias: '/Inscripcion/GetCategorias',
                horarios: '/Inscripcion/GetHorariosDisponibles',
                guardar: '/PortalEstudiante/Inscribir'
            },
            onSaved: () => inscripciones.ajax.reload()
        });
        $('#btnNuevaInscripcion').on('click', () => InscripcionForm.open());
    }

    $('#misInscripcionesTable').on('click', '.btn-cancelar', async function () {
        const id = $(this).data('id');
        const confirm = await Swal.fire({
            title: '¿Cancelar inscripción?',
            html: `Se cancelará <strong>${escapeHtml($(this).data('nombre'))}</strong> y se liberará su cupo.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, cancelar',
            cancelButtonText: 'Volver',
            confirmButtonColor: '#d33'
        });
        if (!confirm.isConfirmed) return;
        try {
            const response = await axios.post(`/PortalEstudiante/Cancelar?id=${id}`);
            if (response.data.success) {
                inscripciones.ajax.reload();
                Swal.fire('Listo', response.data.message, 'success');
            }
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo cancelar'), 'error');
        }
    });
});
