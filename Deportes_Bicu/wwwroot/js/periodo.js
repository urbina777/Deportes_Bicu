$(function () {
    const config = window.periodoConfig || {};
    const modal = new bootstrap.Modal("#periodoModal");
    let isEdit = false;

    const periodoValidator = new JustValidate("#periodoForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    periodoValidator
        .addField("#periodoNombre", [
            { rule: 'required', errorMessage: "El nombre es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 80, errorMessage: "Maximo es de 80 caracteres" },
        ])
        .addField("#periodoFechaInicio", [
            { rule: 'required', errorMessage: "La fecha de inicio es obligatoria." },
        ])
        .addField("#periodoFechaFin", [
            { rule: 'required', errorMessage: "La fecha de fin es obligatoria." },
        ])
        .addField("#periodoEstado", [
            { rule: 'required', errorMessage: "El estado es obligatorio." },
        ]);

    const table = $("#periodoTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Periodo/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'nombre' },
            { data: 'fechaInicio' },
            { data: 'fechaFin' },
            { data: 'estado', render: d => estadoBadge(d) },
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
                        data-id="${data.id}" data-nombre="${escapeHtml(data.nombre)}">
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
        $("#periodoId").val('');
        $('#periodoNombre').val('');
        $('#periodoFechaInicio').val('');
        $('#periodoFechaFin').val('');
        $('#periodoEstado').val('');
        periodoValidator.refresh();
    }

    $("#btnNuevoPeriodo").on('click', function () {
        isEdit = false;
        clearForm();
        $("#periodoModalTitle").text("Nuevo Período");
        modal.show();
    });

    $('#periodoTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Periodo/GetById?id=${id}`);
            isEdit = true;
            $('#periodoId').val(response.data.id);
            $('#periodoNombre').val(response.data.nombre);
            $('#periodoFechaInicio').val(response.data.fechaInicio);
            $('#periodoFechaFin').val(response.data.fechaFin);
            $('#periodoEstado').val(response.data.estado);
            $('#periodoModalTitle').text("Editar Período");
            periodoValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar el período', 'error');
        }
    });

    $('#periodoTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar período?',
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
            const response = await axios.post(`/Periodo/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    periodoValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#periodoId').val() ? parseInt($('#periodoId').val()) : null,
            nombre: $('#periodoNombre').val().trim(),
            fechaInicio: $('#periodoFechaInicio').val(),
            fechaFin: $('#periodoFechaFin').val(),
            estado: $('#periodoEstado').val(),
        };

        try {
            const response = isEdit
                ? await axios.post(`/Periodo/Update`, payload)
                : await axios.post(`/Periodo/Create`, payload);
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
