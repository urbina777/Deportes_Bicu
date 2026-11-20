$(function () {
    const config = window.carreraConfig || {};
    const modal = new bootstrap.Modal("#carreraModal");
    let isEdit = false;

    const carreraValidator = new JustValidate("#carreraForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    carreraValidator.
        addField("#carreraNombre", [
            { rule: 'required', errorMessage: "El nombre es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 100, errorMessage: "Maximo es de 100 caracteres" },
        ])
        .addField("#carreraFacultadId", [
            { rule: 'required', errorMessage: "Seleccione una facultad." },
        ]);

    const table = $("#carreraTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Carrera/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'nombre' },
            { data: 'facultadNombre' },
            { data: 'totalEstudiantes' },
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
        $("#carreraId").val('');
        $("#carreraNombre").val('');
        $("#carreraFacultadId").val('');
        carreraValidator.refresh();
    }

    $("#btnNuevaCarrera").on('click', function () {
        isEdit = false;
        clearForm();
        $("#carreraModalTitle").text("Nueva Carrera");
        modal.show();
    });

    $('#carreraTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Carrera/GetById?id=${id}`);
            isEdit = true;
            $('#carreraId').val(response.data.id);
            $('#carreraNombre').val(response.data.nombre);
            $('#carreraFacultadId').val(response.data.facultadId);
            $('#carreraModalTitle').text("Editar Carrera");
            carreraValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar la carrera', 'error');
        }
    });

    $('#carreraTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar carrera?',
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
            const response = await axios.post(`/Carrera/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    carreraValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#carreraId').val() ? parseInt($('#carreraId').val()) : null,
            nombre: $('#carreraNombre').val().trim(),
            facultadId: parseInt($('#carreraFacultadId').val())
        };

        try {
            const response = isEdit
                ? await axios.post(`/Carrera/Update`, payload)
                : await axios.post(`/Carrera/Create`, payload);
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
