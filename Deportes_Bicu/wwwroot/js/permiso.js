$(function () {
    const config = window.permisosConfig || {};
    const modal = new bootstrap.Modal("#permisoModal");
    let isEdit = false;

    const permisoValidator = new JustValidate("#permisoForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    permisoValidator.
        addField("#permisoNombre", [
            { rule: 'required', errorMessage:"El nombre es obligatorio." },
            { rule: 'maxLength', value: 100, errorMessage:"Maximo es de 100 caracteres" },
            { rule: 'customRegexp', value: /^[a-z][a-z0-9._]*$/, errorMessage: "Use formato: recurso.action (eje:estudiante.crear)" }
        ])
        .addField("#permisoDescripcion", [
            { rule: 'maxLength', value: 250, errorMessage: "Maximo es de 250 caracteres" },
        ])
    const table = $("#permisoTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Admin/GetPermissionData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            {
                data: 'name',
                render: d => `<code>${d}</code>`
            },
            { data: 'description' },
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
                        data-id="${data.id}" data-nombre="${data.name}">
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
        $("#permisoId").val('');
        $("#permisoNombre").val('');
        $("#permisoDescripcion").val('');
        permisoValidator.refresh();
    }

    $("#btnNuevoPermiso").on('click', function () {
        isEdit = false;
        clearForm();
        $("#permisoModalTitle").text("Nuevo Permiso");
        modal.show();
    });

    $('#permisoTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Admin/GetPermissionById?id=${id}`);
            isEdit = true;
            $('#permisoId').val(response.data.id);
            $('#permisoNombre').val(response.data.name);
            $('#permisoDescripcion').val(response.data.description);
            $('#permisoModalTitle').text("Editar Permiso");
            permisoValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar el permiso', 'error');
        }
    });
    $('#permisoTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar permiso?',
            html: `¿Seguro que deseas eliminar a <strong>${nombre}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        });
        if (!confirm.isConfirmed) return
        try {
            const response = await axios.post(`/Admin/DeletePermission?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            const message = error.response?.data?.message || 'No se pudo eliminar';
            Swal.fire('Error', message, 'error');
        }
    });

    permisoValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#permisoId').val() ? parseInt($('#permisoId').val()) : null,
            name: $('#permisoNombre').val().trim(),
            descripcion: $('#permisoDescripcion').val().trim(),
        };

        try {
            const response = isEdit
                ? await axios.post(`/Admin/UpdatePermission`, payload)
                : await axios.post(`/Admin/CreatePermission`, payload);
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
            const message = error.response?.data?.message || 'No se pudo guardar.';
            Swal.fire('Error', message, 'error');
        }
    })
});