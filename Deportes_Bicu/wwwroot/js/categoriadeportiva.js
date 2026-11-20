$(function () {
    const config = window.categoriaDeportivaConfig || {};
    const modal = new bootstrap.Modal("#categoriaDeportivaModal");
    let isEdit = false;

    const categoriaDeportivaValidator = new JustValidate("#categoriaDeportivaForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    categoriaDeportivaValidator
        .addField("#categoriaDeportivaNombre", [
            { rule: 'required', errorMessage: "El nombre es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 80, errorMessage: "Maximo es de 80 caracteres" },
        ])
        .addField("#categoriaDeportivaEdadMinima", [
            { rule: 'required', errorMessage: "La edad mínima es obligatoria." },
            { validator: (value) => Number.isInteger(Number(value)) && Number(value) >= 0 && Number(value) <= 120, errorMessage: "La edad mínima debe estar entre 0 y 120." },
        ])
        .addField("#categoriaDeportivaEdadMaxima", [
            { rule: 'required', errorMessage: "La edad máxima es obligatoria." },
            { validator: (value) => Number.isInteger(Number(value)) && Number(value) >= 0 && Number(value) <= 120, errorMessage: "La edad máxima debe estar entre 0 y 120." },
        ]);

    const table = $("#categoriaDeportivaTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/CategoriaDeportiva/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'nombre' },
            { data: 'edadMinima' },
            { data: 'edadMaxima' },
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
        $("#categoriaDeportivaId").val('');
        $('#categoriaDeportivaNombre').val('');
        $('#categoriaDeportivaEdadMinima').val('');
        $('#categoriaDeportivaEdadMaxima').val('');
        categoriaDeportivaValidator.refresh();
    }

    $("#btnNuevaCategoriaDeportiva").on('click', function () {
        isEdit = false;
        clearForm();
        $("#categoriaDeportivaModalTitle").text("Nueva Categoría deportiva");
        modal.show();
    });

    $('#categoriaDeportivaTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/CategoriaDeportiva/GetById?id=${id}`);
            isEdit = true;
            $('#categoriaDeportivaId').val(response.data.id);
            $('#categoriaDeportivaNombre').val(response.data.nombre);
            $('#categoriaDeportivaEdadMinima').val(response.data.edadMinima);
            $('#categoriaDeportivaEdadMaxima').val(response.data.edadMaxima);
            $('#categoriaDeportivaModalTitle').text("Editar Categoría deportiva");
            categoriaDeportivaValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar la categoría deportiva', 'error');
        }
    });

    $('#categoriaDeportivaTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar categoría deportiva?',
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
            const response = await axios.post(`/CategoriaDeportiva/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    categoriaDeportivaValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#categoriaDeportivaId').val() ? parseInt($('#categoriaDeportivaId').val()) : null,
            nombre: $('#categoriaDeportivaNombre').val().trim(),
            edadMinima: parseInt($('#categoriaDeportivaEdadMinima').val()),
            edadMaxima: parseInt($('#categoriaDeportivaEdadMaxima').val()),
        };

        try {
            const response = isEdit
                ? await axios.post(`/CategoriaDeportiva/Update`, payload)
                : await axios.post(`/CategoriaDeportiva/Create`, payload);
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
