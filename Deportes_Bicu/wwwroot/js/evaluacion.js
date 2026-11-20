$(function () {
    const config = window.evaluacionConfig || {};
    const modal = new bootstrap.Modal("#evaluacionModal");
    let isEdit = false;

    function hoy() {
        const d = new Date();
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
    }
    $('#evaluacionFecha').attr('max', hoy());

    const evaluacionValidator = new JustValidate("#evaluacionForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    evaluacionValidator
        .addField("#evaluacionEstudianteId", [
            { rule: 'required', errorMessage: "Seleccione un estudiante." },
        ])
        .addField("#evaluacionFecha", [
            { rule: 'required', errorMessage: "La fecha es obligatoria." },
            { validator: (value) => !value || value <= hoy(), errorMessage: "La fecha no puede ser futura." },
        ])
        .addField("#evaluacionPeso", [
            { rule: 'required', errorMessage: "El peso es obligatorio." },
            { validator: (value) => parseFloat(value) > 0 && parseFloat(value) <= 999.99, errorMessage: "Ingrese un peso valido en kg." },
        ])
        .addField("#evaluacionEstatura", [
            { rule: 'required', errorMessage: "La estatura es obligatoria." },
            { validator: (value) => parseFloat(value) > 0 && parseFloat(value) <= 99.99, errorMessage: "Ingrese la estatura en metros (ej. 1.75)." },
        ])
        .addField("#evaluacionObservaciones", [
            { rule: 'maxLength', value: 250, errorMessage: "Maximo es de 250 caracteres" },
        ]);

    const table = $("#evaluacionTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/EvaluacionFisica/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'fecha' },
            { data: 'estudianteNombre', render: (d, t, row) => `${escapeHtml(d)}<br><small class="text-muted">${escapeHtml(row.carnet)}</small>` },
            { data: 'peso' },
            { data: 'estatura' },
            { data: 'observaciones', orderable: false, render: d => escapeHtml(d ?? '') },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    let buttons = '';
                    if (config.canUpdate) {
                        buttons += `<button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${data.id}">
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

    async function cargarEstudiantes() {
        const response = await axios.get('/EvaluacionFisica/GetEstudiantes');
        const select = $('#evaluacionEstudianteId').empty().append($('<option>').val('').text('Seleccione un estudiante'));
        response.data.forEach(o => select.append($('<option>').val(o.id).text(o.texto)));
    }

    function clearForm() {
        $('#evaluacionId').val('');
        $('#evaluacionEstudianteId').val('');
        $('#evaluacionFecha').val('');
        $('#evaluacionPeso').val('');
        $('#evaluacionEstatura').val('');
        $('#evaluacionObservaciones').val('');
        evaluacionValidator.refresh();
    }

    $("#btnNuevaEvaluacion").on('click', async function () {
        isEdit = false;
        clearForm();
        try {
            await cargarEstudiantes();
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudieron cargar los estudiantes'), 'error');
            return;
        }
        $('#evaluacionModalTitle').text("Nueva Evaluación");
        modal.show();
    });

    $('#evaluacionTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/EvaluacionFisica/GetById?id=${id}`);
            await cargarEstudiantes();
            isEdit = true;
            $('#evaluacionId').val(response.data.id);
            $('#evaluacionEstudianteId').val(response.data.estudianteId);
            $('#evaluacionFecha').val(response.data.fecha);
            $('#evaluacionPeso').val(response.data.peso);
            $('#evaluacionEstatura').val(response.data.estatura);
            $('#evaluacionObservaciones').val(response.data.observaciones);
            $('#evaluacionModalTitle').text("Editar Evaluación");
            evaluacionValidator.refresh();
            modal.show();
        } catch {
            Swal.fire('Error', 'No se pudo cargar la evaluación', 'error');
        }
    });

    $('#evaluacionTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre');
        const confirm = await Swal.fire({
            title: '¿Eliminar evaluación?',
            html: `¿Seguro que deseas eliminar la evaluación de <strong>${escapeHtml(nombre)}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        });
        if (!confirm.isConfirmed) return;
        try {
            const response = await axios.post(`/EvaluacionFisica/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    evaluacionValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#evaluacionId').val() ? parseInt($('#evaluacionId').val()) : null,
            estudianteId: parseInt($('#evaluacionEstudianteId').val()),
            fecha: $('#evaluacionFecha').val(),
            peso: parseFloat($('#evaluacionPeso').val()),
            estatura: parseFloat($('#evaluacionEstatura').val()),
            observaciones: $('#evaluacionObservaciones').val().trim() || null
        };

        try {
            const response = isEdit
                ? await axios.post(`/EvaluacionFisica/Update`, payload)
                : await axios.post(`/EvaluacionFisica/Create`, payload);
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
