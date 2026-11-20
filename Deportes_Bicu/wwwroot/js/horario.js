$(function () {
    const config = window.horarioConfig || {};
    const modal = new bootstrap.Modal("#horarioModal");
    let isEdit = false;

    const horarioValidator = new JustValidate("#horarioForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    horarioValidator
        .addField("#horarioEntrenadorDeporteId", [
            { rule: 'required', errorMessage: "Seleccione el deporte y el entrenador." },
        ])
        .addField("#horarioInstalacionId", [
            { rule: 'required', errorMessage: "Seleccione una instalación." },
        ])
        .addField("#horarioDia", [
            { rule: 'required', errorMessage: "Seleccione el día." },
        ])
        .addField("#horarioHoraInicio", [
            { rule: 'required', errorMessage: "La hora de inicio es obligatoria." },
        ])
        .addField("#horarioHoraFin", [
            { rule: 'required', errorMessage: "La hora de fin es obligatoria." },
            {
                validator: (value) => !$('#horarioHoraInicio').val() || value > $('#horarioHoraInicio').val(),
                errorMessage: "La hora de fin debe ser posterior a la de inicio."
            },
        ]);

    const table = $("#horarioTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Horario/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'dia' },
            { data: 'horaInicio', render: d => horaCorta(d) },
            { data: 'horaFin', render: d => horaCorta(d) },
            { data: 'deporteNombre' },
            { data: 'entrenadorNombre' },
            { data: 'instalacionNombre' },
            { data: null, orderable: false, searchable: false, render: d => `${d.inscritos} / ${d.capacidad}` },
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
                        buttons += `<button class="btn btn-sm btn-outline-danger btn-delete"
                        data-id="${data.id}" data-nombre="${escapeHtml(data.dia + ' ' + horaCorta(data.horaInicio) + ' - ' + data.deporteNombre)}">
                        <i class="fa-solid fa-trash"></i></button>`;
                    }
                    return buttons || '<span class="text-muted">-</span>'
                }
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
        },
        order: [[1, 'asc']]
    });

    function clearForm() {
        $("#horarioId").val('');
        $("#horarioEntrenadorDeporteId").val('');
        $("#horarioInstalacionId").val('');
        $("#horarioDia").val('');
        $("#horarioHoraInicio").val('');
        $("#horarioHoraFin").val('');
        horarioValidator.refresh();
    }

    $("#btnNuevoHorario").on('click', function () {
        isEdit = false;
        clearForm();
        $("#horarioModalTitle").text("Nuevo Horario");
        modal.show();
    });

    $('#horarioTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Horario/GetById?id=${id}`);
            isEdit = true;
            $('#horarioId').val(response.data.id);
            $('#horarioEntrenadorDeporteId').val(response.data.entrenadorDeporteId);
            $('#horarioInstalacionId').val(response.data.instalacionId);
            $('#horarioDia').val(response.data.dia);
            $('#horarioHoraInicio').val(horaCorta(response.data.horaInicio));
            $('#horarioHoraFin').val(horaCorta(response.data.horaFin));
            $('#horarioModalTitle').text("Editar Horario");
            horarioValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar el horario', 'error');
        }
    });

    $('#horarioTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar horario?',
            html: `¿Seguro que deseas eliminar <strong>${escapeHtml(nombre)}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        });
        if (!confirm.isConfirmed) return
        try {
            const response = await axios.post(`/Horario/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    horarioValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#horarioId').val() ? parseInt($('#horarioId').val()) : null,
            entrenadorDeporteId: parseInt($('#horarioEntrenadorDeporteId').val()),
            instalacionId: parseInt($('#horarioInstalacionId').val()),
            dia: $('#horarioDia').val(),
            horaInicio: horaCompleta($('#horarioHoraInicio').val()),
            horaFin: horaCompleta($('#horarioHoraFin').val())
        };

        try {
            const response = isEdit
                ? await axios.post(`/Horario/Update`, payload)
                : await axios.post(`/Horario/Create`, payload);
            if (response.data.success) {
                modal.hide();
                table.ajax.reload();
                Swal.fire({
                    title: 'Listo',
                    text: response.data.message,
                    icon: 'success',
                    timer: 2000,
                    showConfirmButton: false
                });
            }

        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo guardar.'), 'error');
        }
    });
});
