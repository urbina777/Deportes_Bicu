$(function () {
    const config = window.asistenciaConfig || {};
    const modal = new bootstrap.Modal("#asistenciaModal");
    let isEdit = false;

    //Fecha local de hoy en formato yyyy-MM-dd (la asistencia no puede ser futura)
    function hoy() {
        const d = new Date();
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }
    $('#asistenciaFecha').attr('max', hoy());

    const asistenciaValidator = new JustValidate("#asistenciaForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    asistenciaValidator
        .addField("#asistenciaInscripcionId", [
            { rule: 'required', errorMessage: "Seleccione una inscripción." },
        ])
        .addField("#asistenciaFecha", [
            { rule: 'required', errorMessage: "La fecha es obligatoria." },
            { validator: (value) => !value || value <= hoy(), errorMessage: "La fecha no puede ser futura." },
        ])
        .addField("#asistenciaEstado", [
            { rule: 'required', errorMessage: "Seleccione el estado." },
        ]);

    const table = $("#asistenciaTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Asistencia/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'fecha' },
            { data: 'estudianteNombre', render: (d, t, row) => `${escapeHtml(d)}<br><small class="text-muted">${escapeHtml(row.carnet)}</small>` },
            { data: 'deporteNombre' },
            { data: 'periodoNombre' },
            { data: 'estado', render: d => estadoBadge(d) },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    let buttons = '';
                    const texto = escapeHtml(data.estudianteNombre + ' - ' + data.deporteNombre + ' - ' + data.periodoNombre);
                    if (config.canUpdate) {
                        buttons += `<button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${data.id}" data-texto="${texto}">
                        <i class="fa-solid fa-pen"></i></button>`;
                    }
                    if (config.canDelete) {
                        buttons += `<button class="btn btn-sm btn-outline-danger btn-delete" data-id="${data.id}"
                        data-nombre="${escapeHtml(data.estudianteNombre + ' (' + data.fecha + ')')}">
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

    function clearForm() {
        $('#asistenciaId').val('');
        $('#asistenciaInscripcionId').val('');
        $('#asistenciaFecha').val('');
        $('#asistenciaEstado').val('');
        asistenciaValidator.refresh();
    }

    $("#btnNuevaAsistencia").on('click', async function () {
        isEdit = false;
        clearForm();
        try {
            const response = await axios.get('/Asistencia/GetInscripciones');
            const select = $('#asistenciaInscripcionId').empty().append($('<option>').val('').text('Seleccione una inscripción'));
            response.data.forEach(o => select.append($('<option>').val(o.id).text(o.texto)));
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudieron cargar las inscripciones'), 'error');
            return;
        }
        $('#asistenciaModalTitle').text("Nueva Asistencia");
        modal.show();
    });

    $('#asistenciaTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        const texto = $(this).data('texto');
        try {
            const response = await axios.get(`/Asistencia/GetById?id=${id}`);
            isEdit = true;
            //La inscripcion de un registro existente no se cambia: el selector solo muestra la actual
            $('#asistenciaInscripcionId').empty()
                .append($('<option>').val(response.data.inscripcionId).text(texto))
                .val(response.data.inscripcionId);
            $('#asistenciaId').val(response.data.id);
            $('#asistenciaFecha').val(response.data.fecha);
            $('#asistenciaEstado').val(response.data.estado);
            $('#asistenciaModalTitle').text("Editar Asistencia");
            asistenciaValidator.refresh();
            modal.show();
        } catch {
            Swal.fire('Error', 'No se pudo cargar la asistencia', 'error');
        }
    });

    $('#asistenciaTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre');
        const confirm = await Swal.fire({
            title: '¿Eliminar asistencia?',
            html: `¿Seguro que deseas eliminar la asistencia de <strong>${escapeHtml(nombre)}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        });
        if (!confirm.isConfirmed) return;
        try {
            const response = await axios.post(`/Asistencia/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    asistenciaValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#asistenciaId').val() ? parseInt($('#asistenciaId').val()) : null,
            inscripcionId: parseInt($('#asistenciaInscripcionId').val()),
            fecha: $('#asistenciaFecha').val(),
            estado: $('#asistenciaEstado').val()
        };

        try {
            const response = isEdit
                ? await axios.post(`/Asistencia/Update`, payload)
                : await axios.post(`/Asistencia/Create`, payload);
            if (response.data.success) {
                modal.hide();
                table.ajax.reload();
                Swal.fire({ title: 'Listo', text: response.data.message, icon: 'success', timer: 2000, showConfirmButton: false });
            }
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo guardar.'), 'error');
        }
    });
});
