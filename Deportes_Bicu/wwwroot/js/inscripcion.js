$(function () {
    const config = window.inscripcionConfig || {};

    const table = $("#inscripcionTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Inscripcion/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'fecha' },
            { data: 'estudianteNombre', render: (d, t, row) => `${escapeHtml(d)}<br><small class="text-muted">${escapeHtml(row.carnet)}</small>` },
            { data: 'deporteNombre' },
            { data: 'categoriaNombre' },
            { data: 'periodoNombre' },
            {
                data: 'horarios',
                orderable: false,
                searchable: false,
                render: function (horarios) {
                    return horarios.map(h =>
                        `<div class="small">${escapeHtml(h.dia)} ${horaCorta(h.horaInicio)}-${horaCorta(h.horaFin)} · ${escapeHtml(h.instalacionNombre)}</div>`
                    ).join('');
                }
            },
            { data: 'estado', render: d => estadoBadge(d) },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    let buttons = '';
                    const nombre = escapeHtml(data.estudianteNombre + ' - ' + data.deporteNombre);
                    if (config.canUpdate && data.estado === 'Activa') {
                        buttons += `<button class="btn btn-sm btn-outline-primary me-1 btn-finalizar" data-id="${data.id}" data-nombre="${nombre}" title="Finalizar">
                        <i class="fa-solid fa-flag-checkered"></i></button>`;
                        buttons += `<button class="btn btn-sm btn-outline-warning me-1 btn-cancelar" data-id="${data.id}" data-nombre="${nombre}" title="Cancelar">
                        <i class="fa-solid fa-ban"></i></button>`;
                    }
                    if (config.canDelete) {
                        buttons += `<button class="btn btn-sm btn-outline-danger btn-delete" data-id="${data.id}" data-nombre="${nombre}" title="Eliminar">
                        <i class="fa-solid fa-trash"></i></button>`;
                    }
                    return buttons || '<span class="text-muted">-</span>';
                }
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
        },
        order: [[1, 'desc']]
    });

    if (config.canCreate) {
        InscripcionForm.init({
            urls: {
                opciones: '/Inscripcion/GetOpciones',
                estudiantes: '/Inscripcion/GetEstudiantes',
                categorias: '/Inscripcion/GetCategorias',
                horarios: '/Inscripcion/GetHorariosDisponibles',
                guardar: '/Inscripcion/Create'
            },
            onSaved: () => table.ajax.reload()
        });
        $('#btnNuevaInscripcion').on('click', () => InscripcionForm.open());
    }

    async function cambiarEstado(id, estado, titulo, texto, boton) {
        const confirm = await Swal.fire({
            title: titulo,
            html: texto,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: boton,
            cancelButtonText: 'Volver'
        });
        if (!confirm.isConfirmed) return;
        try {
            const response = await axios.post(`/Inscripcion/CambiarEstado?id=${id}&estado=${estado}`);
            if (response.data.success) {
                table.ajax.reload(null, false);
                Swal.fire('Listo', response.data.message, 'success');
            }
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo cambiar el estado'), 'error');
        }
    }

    $('#inscripcionTable').on('click', '.btn-finalizar', function () {
        cambiarEstado($(this).data('id'), 'Finalizada', '¿Finalizar inscripción?',
            `Se marcará como finalizada: <strong>${escapeHtml($(this).data('nombre'))}</strong>`, 'Sí, finalizar');
    });

    $('#inscripcionTable').on('click', '.btn-cancelar', function () {
        cambiarEstado($(this).data('id'), 'Cancelada', '¿Cancelar inscripción?',
            `Se cancelará: <strong>${escapeHtml($(this).data('nombre'))}</strong>. Libera el cupo en los horarios.`, 'Sí, cancelar');
    });

    $('#inscripcionTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre');
        const confirm = await Swal.fire({
            title: '¿Eliminar inscripción?',
            html: `¿Seguro que deseas eliminar <strong>${escapeHtml(nombre)}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        });
        if (!confirm.isConfirmed) return;
        try {
            const response = await axios.post(`/Inscripcion/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });
});
