$(function () {
    const config = window.entrenadorConfig || {};
    const modal = new bootstrap.Modal("#entrenadorModal");
    const deportesModal = new bootstrap.Modal("#deportesModal");
    let isEdit = false;
    let entrenadorActualId = null;

    //Telefono opcional: 7 a 20 caracteres entre digitos, +, espacios, guiones y parentesis
    const telefonoRegex = /^\+?[0-9\s\-()]{7,20}$/;

    const entrenadorValidator = new JustValidate("#entrenadorForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    entrenadorValidator
        .addField("#entrenadorUsuarioId", [
            { rule: 'required', errorMessage: "Seleccione una cuenta de usuario." },
        ])
        .addField("#entrenadorNombre", [
            { rule: 'required', errorMessage: "El nombre es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 50, errorMessage: "Maximo es de 50 caracteres" },
        ])
        .addField("#entrenadorApellido", [
            { rule: 'required', errorMessage: "El Apellido es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 50, errorMessage: "Maximo es de 50 caracteres" },
        ])
        .addField("#entrenadorCedula", [
            { rule: 'required', errorMessage: "La cedula es obligatoria." },
            { rule: 'maxLength', value: 20, errorMessage: "Maximo es de 20 caracteres" },
        ])
        .addField("#entrenadorCarnet", [
            { rule: 'required', errorMessage: "El codigo institucional es obligatorio." },
            { rule: 'maxLength', value: 30, errorMessage: "Maximo es de 30 caracteres" },
        ])
        .addField("#entrenadorCorreo", [
            { rule: 'required', errorMessage: "El correo es obligatorio." },
            { rule: 'email', errorMessage: "Ingrese un correo valido" },
            { rule: 'maxLength', value: 100, errorMessage: "Maximo es de 100 caracteres" },
        ])
        .addField("#entrenadorTelefono", [
            {
                validator: (value) => value === '' || telefonoRegex.test(value),
                errorMessage: "Ingrese un telefono valido"
            },
        ]);

    const table = $("#entrenadorTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Entrenador/GetData',
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
            { data: 'userName' },
            { data: 'totalDeportes' },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    let buttons = `<button class="btn btn-sm btn-outline-secondary me-1 btn-deportes" data-id="${data.id}"
                        data-nombre="${escapeHtml(data.nombre + ' ' + data.apellido)}" title="Deportes"><i class="fa-solid fa-futbol"></i></button>`;
                    if (config.canUpdate) {
                        buttons += `<button class="btn btn-sm btn-outline-primary me-1 btn-edit" data-id="${data.id}">
                        <i class="fa-solid fa-pen"></i></button>`;
                    }
                    if (config.canDelete) {
                        buttons += `<button class="btn btn-sm btn-outline-danger btn-delete"
                        data-id="${data.id}" data-nombre="${escapeHtml(data.nombre + ' ' + data.apellido)}">
                        <i class="fa-solid fa-trash"></i></button>`;
                    }
                    return buttons;
                }
            }
        ],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/es-ES.json'
        },
        order: [[1, 'asc']]
    });

    //Carga las cuentas sin perfil de entrenador; al editar se incluye la cuenta actual
    async function cargarUsuarios(incluirId) {
        const select = $('#entrenadorUsuarioId');
        const url = incluirId
            ? `/Entrenador/GetUsuariosDisponibles?incluirId=${incluirId}`
            : '/Entrenador/GetUsuariosDisponibles';
        const response = await axios.get(url);
        select.empty().append($('<option>').val('').text('Seleccione una cuenta de usuario'));
        response.data.forEach(u => {
            select.append($('<option>').val(u.id).text(`${u.userName} (${u.email})`));
        });
    }

    function clearForm() {
        $("#entrenadorId").val('');
        $("#entrenadorUsuarioId").val('');
        $("#entrenadorNombre").val('');
        $("#entrenadorApellido").val('');
        $("#entrenadorCedula").val('');
        $("#entrenadorCarnet").val('');
        $("#entrenadorCorreo").val('');
        $("#entrenadorTelefono").val('');
        entrenadorValidator.refresh();
    }

    $("#btnNuevoEntrenador").on('click', async function () {
        isEdit = false;
        clearForm();
        try {
            await cargarUsuarios();
        } catch {
            Swal.fire('Error', 'No se pudieron cargar las cuentas de usuario', 'error');
            return;
        }
        $("#entrenadorModalTitle").text("Nuevo Entrenador");
        modal.show();
    });

    $('#entrenadorTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Entrenador/GetById?id=${id}`);
            await cargarUsuarios(response.data.usuarioId);
            isEdit = true;
            $('#entrenadorId').val(response.data.id);
            $('#entrenadorUsuarioId').val(response.data.usuarioId);
            $('#entrenadorNombre').val(response.data.nombre);
            $('#entrenadorApellido').val(response.data.apellido);
            $('#entrenadorCedula').val(response.data.cedula);
            $('#entrenadorCarnet').val(response.data.carnet);
            $('#entrenadorCorreo').val(response.data.correo);
            $('#entrenadorTelefono').val(response.data.telefono);
            $('#entrenadorModalTitle').text("Editar Entrenador");
            entrenadorValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar el entrenador', 'error');
        }
    });

    $('#entrenadorTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar entrenador?',
            html: `¿Seguro que deseas eliminar a <strong>${escapeHtml(nombre)}</strong>?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Si, eliminar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d'
        });
        if (!confirm.isConfirmed) return
        try {
            const response = await axios.post(`/Entrenador/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    entrenadorValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#entrenadorId').val() ? parseInt($('#entrenadorId').val()) : null,
            nombre: $('#entrenadorNombre').val().trim(),
            apellido: $('#entrenadorApellido').val().trim(),
            cedula: $('#entrenadorCedula').val().trim(),
            carnet: $('#entrenadorCarnet').val().trim(),
            correo: $('#entrenadorCorreo').val().trim(),
            telefono: $('#entrenadorTelefono').val().trim() || null,
            usuarioId: parseInt($('#entrenadorUsuarioId').val())
        };

        try {
            const response = isEdit
                ? await axios.post(`/Entrenador/Update`, payload)
                : await axios.post(`/Entrenador/Create`, payload);
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

    //----- Deportes del entrenador (tabla EntrenadorDeporte) -----
    async function cargarDeportes() {
        const response = await axios.get(`/Entrenador/GetDeportes?entrenadorId=${entrenadorActualId}`);
        const tbody = $('#deportesAsignados').empty();
        if (response.data.asignadas.length === 0) {
            tbody.append('<tr><td colspan="3" class="text-muted">Sin deportes asignados.</td></tr>');
        }
        response.data.asignadas.forEach(a => {
            const quitar = config.canAsignar
                ? `<button class="btn btn-sm btn-outline-danger btn-quitar-deporte" data-id="${a.id}"><i class="fa-solid fa-xmark"></i></button>`
                : '';
            tbody.append(`<tr><td>${escapeHtml(a.texto)}</td><td>${estadoBadge(a.detalle)}</td><td class="text-end">${quitar}</td></tr>`);
        });
        const select = $('#deporteAsignarId');
        if (select.length) {
            select.empty().append($('<option>').val('').text('Seleccione un deporte'));
            response.data.disponibles.forEach(o => select.append($('<option>').val(o.id).text(o.texto)));
        }
    }

    $('#entrenadorTable').on('click', '.btn-deportes', async function () {
        entrenadorActualId = $(this).data('id');
        $('#deportesModalTitle').text(`Deportes de ${$(this).data('nombre')}`);
        try {
            await cargarDeportes();
            deportesModal.show();
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudieron cargar los deportes'), 'error');
        }
    });

    $('#btnAsignarDeporte').on('click', async function () {
        const deporteId = $('#deporteAsignarId').val();
        if (!deporteId) {
            Swal.fire('Atención', 'Seleccione un deporte.', 'warning');
            return;
        }
        try {
            await axios.post(`/Entrenador/AsignarDeporte?entrenadorId=${entrenadorActualId}&deporteId=${deporteId}`);
            await cargarDeportes();
            table.ajax.reload(null, false);
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo asignar'), 'error');
        }
    });

    $('#deportesAsignados').on('click', '.btn-quitar-deporte', async function () {
        try {
            await axios.post(`/Entrenador/QuitarDeporte?id=${$(this).data('id')}`);
            await cargarDeportes();
            table.ajax.reload(null, false);
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo quitar'), 'error');
        }
    });
});
