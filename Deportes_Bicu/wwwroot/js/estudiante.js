$(function () {
    const config = window.estudiantesConfig || {};
    const modal = new bootstrap.Modal("#estudianteModal");
    let isEdit = false;

    //Telefono opcional: 7 a 20 caracteres entre digitos, +, espacios, guiones y parentesis
    const telefonoRegex = /^\+?[0-9\s\-()]{7,20}$/;

    const estudianteValidator = new JustValidate("#estudianteForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    estudianteValidator.
        addField("#estudianteUsuarioId", [
            { rule: 'required', errorMessage: "Seleccione una cuenta de usuario." },
        ])
        .addField("#estudianteNombre", [
            { rule: 'required', errorMessage: "El nombre es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 50, errorMessage: "Maximo es de 50 caracteres" },
        ])
        .addField("#estudianteApellido", [
            { rule: 'required', errorMessage: "El Apellido es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 50, errorMessage: "Maximo es de 50 caracteres" },
        ])
        .addField("#estudianteCedula", [
            { rule: 'required', errorMessage: "La cedula es obligatoria." },
            { rule: 'maxLength', value: 20, errorMessage: "Maximo es de 20 caracteres" },
        ])
        .addField("#estudianteCarnet", [
            { rule: 'required', errorMessage: "El codigo institucional es obligatorio." },
            { rule: 'maxLength', value: 30, errorMessage: "Maximo es de 30 caracteres" },
        ])
        .addField("#estudianteCorreo", [
            { rule: 'required', errorMessage: "El correo es obligatorio." },
            { rule: 'email', errorMessage: "Ingrese un correo valido" },
            { rule: 'maxLength', value: 100, errorMessage: "Maximo es de 100 caracteres" },
        ])
        .addField("#estudianteTelefono", [
            {
                validator: (value) => value === '' || telefonoRegex.test(value),
                errorMessage: "Ingrese un telefono valido"
            },
        ])
        .addField("#estudianteCarreraId", [
            { rule: 'required', errorMessage: "Seleccione una carrera." },
        ]);

    const table = $("#estudianteTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Estudiante/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'nombre' },
            { data: 'apellido' },
            { data: 'carnet' },
            { data: 'correo' },
            { data: 'carreraNombre' },
            { data: 'userName' },
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
                        data-id="${data.id}" data-nombre="${data.nombre} ${data.apellido}">
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

    //Carga las cuentas sin perfil de estudiante; al editar se incluye la cuenta actual
    async function cargarUsuarios(incluirId) {
        const select = $('#estudianteUsuarioId');
        const url = incluirId
            ? `/Estudiante/GetUsuariosDisponibles?incluirId=${incluirId}`
            : '/Estudiante/GetUsuariosDisponibles';
        const response = await axios.get(url);
        select.empty().append($('<option>').val('').text('Seleccione una cuenta de usuario'));
        response.data.forEach(u => {
            select.append($('<option>').val(u.id).text(`${u.userName} (${u.email})`));
        });
    }

    function clearForm() {
        $("#estudianteId").val('');
        $("#estudianteUsuarioId").val('');
        $("#estudianteNombre").val('');
        $("#estudianteApellido").val('');
        $("#estudianteCedula").val('');
        $("#estudianteCarnet").val('');
        $("#estudianteCorreo").val('');
        $("#estudianteTelefono").val('');
        $("#estudianteCarreraId").val('');
        estudianteValidator.refresh();
    }

    $("#btnNuevoEstudiante").on('click', async function () {
        isEdit = false;
        clearForm();
        try {
            await cargarUsuarios();
        } catch {
            Swal.fire('Error', 'No se pudieron cargar las cuentas de usuario', 'error');
            return;
        }
        $("#estudianteModalTitle").text("Nuevo Estudiante");
        modal.show();
    });

    $('#estudianteTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Estudiante/GetById?id=${id}`);
            await cargarUsuarios(response.data.usuarioId);
            isEdit = true;
            $('#estudianteId').val(response.data.id);
            $('#estudianteUsuarioId').val(response.data.usuarioId);
            $('#estudianteNombre').val(response.data.nombre);
            $('#estudianteApellido').val(response.data.apellido);
            $('#estudianteCedula').val(response.data.cedula);
            $('#estudianteCarnet').val(response.data.carnet);
            $('#estudianteCorreo').val(response.data.correo);
            $('#estudianteTelefono').val(response.data.telefono);
            $('#estudianteCarreraId').val(response.data.carreraId);
            $('#estudianteModalTitle').text("Editar Estudiante");
            estudianteValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar el estudiante', 'error');
        }
    });

    $('#estudianteTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar estudiante?',
            html: `¿Seguro que deseas eliminar a <strong>${$('<div>').text(nombre).html()}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        });
        if (!confirm.isConfirmed) return
        try {
            const response = await axios.post(`/Estudiante/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    estudianteValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#estudianteId').val() ? parseInt($('#estudianteId').val()) : null,
            nombre: $('#estudianteNombre').val().trim(),
            apellido: $('#estudianteApellido').val().trim(),
            cedula: $('#estudianteCedula').val().trim(),
            carnet: $('#estudianteCarnet').val().trim(),
            correo: $('#estudianteCorreo').val().trim(),
            telefono: $('#estudianteTelefono').val().trim() || null,
            carreraId: parseInt($('#estudianteCarreraId').val()),
            usuarioId: parseInt($('#estudianteUsuarioId').val())
        };

        try {
            const response = isEdit
                ? await axios.post(`/Estudiante/Update`, payload)
                : await axios.post(`/Estudiante/Create`, payload);
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
