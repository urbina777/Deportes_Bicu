$(function () {
    const config = window.facultadConfig || {};
    const modal = new bootstrap.Modal("#facultadModal");
    let isEdit = false;

    const facultadValidator = new JustValidate("#facultadForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    facultadValidator.
        addField("#facultadNombre", [
            { rule: 'required', errorMessage: "El nombre es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 100, errorMessage: "Maximo es de 100 caracteres" },
        ])
        .addField("#facultadDescripcion", [
            { rule: 'maxLength', value: 200, errorMessage: "Maximo es de 200 caracteres" },
        ]);

    const table = $("#facultadTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Facultad/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'nombre' },
            { data: 'descripcion', render: d => d || '-' },
            { data: 'totalCarreras' },
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
                        data-id="${data.id}" data-nombre="${data.nombre}">
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

    //El controller responde 'message' (validacion del modelo) o 'errorMessage' (regla de negocio)
    function getErrorMessage(error, fallback) {
        return error.response?.data?.message || error.response?.data?.errorMessage || fallback;
    }

    function clearForm() {
        $("#facultadId").val('');
        $("#facultadNombre").val('');
        $("#facultadDescripcion").val('');
        facultadValidator.refresh();
    }

    $("#btnNuevaFacultad").on('click', function () {
        isEdit = false;
        clearForm();
        $("#facultadModalTitle").text("Nueva Facultad");
        modal.show();
    });

    $('#facultadTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Facultad/GetById?id=${id}`);
            isEdit = true;
            $('#facultadId').val(response.data.id);
            $('#facultadNombre').val(response.data.nombre);
            $('#facultadDescripcion').val(response.data.descripcion);
            $('#facultadModalTitle').text("Editar Facultad");
            facultadValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar la facultad', 'error');
        }
    });

    $('#facultadTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar facultad?',
            html: `¿Seguro que deseas eliminar <strong>${$('<div>').text(nombre).html()}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        });
        if (!confirm.isConfirmed) return
        try {
            const response = await axios.post(`/Facultad/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    facultadValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#facultadId').val() ? parseInt($('#facultadId').val()) : null,
            nombre: $('#facultadNombre').val().trim(),
            descripcion: $('#facultadDescripcion').val().trim(),
        };

        try {
            const response = isEdit
                ? await axios.post(`/Facultad/Update`, payload)
                : await axios.post(`/Facultad/Create`, payload);
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
    })
});
