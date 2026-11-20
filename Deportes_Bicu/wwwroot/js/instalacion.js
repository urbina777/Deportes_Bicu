$(function () {
    const config = window.instalacionConfig || {};
    const modal = new bootstrap.Modal("#instalacionModal");
    let isEdit = false;

    const instalacionValidator = new JustValidate("#instalacionForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    instalacionValidator
        .addField("#instalacionNombre", [
            { rule: 'required', errorMessage: "El nombre es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 80, errorMessage: "Maximo es de 80 caracteres" },
        ])
        .addField("#instalacionUbicacion", [
            { rule: 'maxLength', value: 150, errorMessage: "Maximo es de 150 caracteres" },
        ])
        .addField("#instalacionCapacidad", [
            { rule: 'required', errorMessage: "La capacidad es obligatoria." },
            { validator: (value) => Number.isInteger(Number(value)) && Number(value) >= 1 && Number(value) <= 10000, errorMessage: "La capacidad debe estar entre 1 y 10000." },
        ])
        .addField("#instalacionEstado", [
            { rule: 'required', errorMessage: "El estado es obligatorio." },
        ]);

    const table = $("#instalacionTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Instalacion/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'nombre' },
            { data: 'ubicacion', render: d => d || '-' },
            { data: 'capacidad' },
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
        $("#instalacionId").val('');
        $('#instalacionNombre').val('');
        $('#instalacionUbicacion').val('');
        $('#instalacionCapacidad').val('');
        $('#instalacionEstado').val('');
        instalacionValidator.refresh();
    }

    $("#btnNuevaInstalacion").on('click', function () {
        isEdit = false;
        clearForm();
        $("#instalacionModalTitle").text("Nueva Instalación");
        modal.show();
    });

    $('#instalacionTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Instalacion/GetById?id=${id}`);
            isEdit = true;
            $('#instalacionId').val(response.data.id);
            $('#instalacionNombre').val(response.data.nombre);
            $('#instalacionUbicacion').val(response.data.ubicacion);
            $('#instalacionCapacidad').val(response.data.capacidad);
            $('#instalacionEstado').val(response.data.estado);
            $('#instalacionModalTitle').text("Editar Instalación");
            instalacionValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar la instalación', 'error');
        }
    });

    $('#instalacionTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar instalación?',
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
            const response = await axios.post(`/Instalacion/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    instalacionValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#instalacionId').val() ? parseInt($('#instalacionId').val()) : null,
            nombre: $('#instalacionNombre').val().trim(),
            ubicacion: $('#instalacionUbicacion').val().trim(),
            capacidad: parseInt($('#instalacionCapacidad').val()),
            estado: $('#instalacionEstado').val(),
        };

        try {
            const response = isEdit
                ? await axios.post(`/Instalacion/Update`, payload)
                : await axios.post(`/Instalacion/Create`, payload);
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
