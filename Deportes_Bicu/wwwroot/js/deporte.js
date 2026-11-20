$(function () {
    const config = window.deporteConfig || {};
    const modal = new bootstrap.Modal("#deporteModal");
    let isEdit = false;

    const deporteValidator = new JustValidate("#deporteForm", {
        validateBeforeSubmitting: true,
        focusInvalidField: true,
        errorFieldCssClass: 'is-invalid',
        successFieldCssClass: 'is-valid'
    });

    deporteValidator
        .addField("#deporteNombre", [
            { rule: 'required', errorMessage: "El nombre es obligatorio." },
            { rule: 'minLength', value: 2, errorMessage: "Minimo 2 caracteres" },
            { rule: 'maxLength', value: 80, errorMessage: "Maximo es de 80 caracteres" },
        ])
        .addField("#deporteDescripcion", [
            { rule: 'maxLength', value: 200, errorMessage: "Maximo es de 200 caracteres" },
        ])
        .addField("#deporteEstado", [
            { rule: 'required', errorMessage: "El estado es obligatorio." },
        ]);

    const table = $("#deporteTable").DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: '/Deporte/GetData',
            type: 'POST',
            contentType: 'application/json',
            data: function (d) { return JSON.stringify(d); }
        },
        columns: [
            { data: 'id' },
            { data: 'nombre' },
            { data: 'descripcion', render: d => d || '-' },
            { data: 'estado', render: d => estadoBadge(d) },
            {
                data: null,
                orderable: false,
                searchable: false,
                render: function (data) {
                    let buttons = '';
                    buttons += `<button class="btn btn-sm btn-outline-secondary me-1 btn-categorias" data-id="${data.id}"
                        data-nombre="${escapeHtml(data.nombre)}" title="Categorías"><i class="fa-solid fa-tags"></i></button>`;
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
        $("#deporteId").val('');
        $('#deporteNombre').val('');
        $('#deporteDescripcion').val('');
        $('#deporteEstado').val('');
        deporteValidator.refresh();
    }

    $("#btnNuevoDeporte").on('click', function () {
        isEdit = false;
        clearForm();
        $("#deporteModalTitle").text("Nuevo Deporte");
        modal.show();
    });

    $('#deporteTable').on('click', '.btn-edit', async function () {
        const id = $(this).data('id');
        try {
            const response = await axios.get(`/Deporte/GetById?id=${id}`);
            isEdit = true;
            $('#deporteId').val(response.data.id);
            $('#deporteNombre').val(response.data.nombre);
            $('#deporteDescripcion').val(response.data.descripcion);
            $('#deporteEstado').val(response.data.estado);
            $('#deporteModalTitle').text("Editar Deporte");
            deporteValidator.refresh();
            modal.show();

        } catch {
            Swal.fire('Error', 'No se pudo cargar el deporte', 'error');
        }
    });

    $('#deporteTable').on('click', '.btn-delete', async function () {
        const id = $(this).data('id');
        const nombre = $(this).data('nombre')
        const confirm = await Swal.fire({
            title: '¿Eliminar deporte?',
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
            const response = await axios.post(`/Deporte/Delete?id=${id}`);
            if (response.data.success) {
                table.ajax.reload();
                Swal.fire('Eliminado', response.data.message, 'success');
            }
        }
        catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo eliminar'), 'error');
        }
    });

    deporteValidator.onSuccess(async (event) => {
        event.preventDefault();

        const payload = {
            id: $('#deporteId').val() ? parseInt($('#deporteId').val()) : null,
            nombre: $('#deporteNombre').val().trim(),
            descripcion: $('#deporteDescripcion').val().trim(),
            estado: $('#deporteEstado').val(),
        };

        try {
            const response = isEdit
                ? await axios.post(`/Deporte/Update`, payload)
                : await axios.post(`/Deporte/Create`, payload);
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

    //----- Categorias del deporte (tabla DeporteCategoria) -----
    const categoriasModal = new bootstrap.Modal("#categoriasModal");
    let deporteActualId = null;

    async function cargarCategorias() {
        const response = await axios.get(`/Deporte/GetCategorias?deporteId=${deporteActualId}`);
        const tbody = $('#categoriasAsignadas').empty();
        if (response.data.asignadas.length === 0) {
            tbody.append('<tr><td colspan="3" class="text-muted">Sin categorías asignadas.</td></tr>');
        }
        response.data.asignadas.forEach(a => {
            const quitar = config.canAsignar
                ? `<button class="btn btn-sm btn-outline-danger btn-quitar-categoria" data-id="${a.id}"><i class="fa-solid fa-xmark"></i></button>`
                : '';
            tbody.append(`<tr><td>${escapeHtml(a.texto)}</td><td>${escapeHtml(a.detalle || '')}</td><td class="text-end">${quitar}</td></tr>`);
        });
        const select = $('#categoriaAsignarId');
        if (select.length) {
            select.empty().append($('<option>').val('').text('Seleccione una categoría'));
            response.data.disponibles.forEach(o => select.append($('<option>').val(o.id).text(o.texto)));
        }
    }

    $('#deporteTable').on('click', '.btn-categorias', async function () {
        deporteActualId = $(this).data('id');
        $('#categoriasModalTitle').text(`Categorías de ${$(this).data('nombre')}`);
        try {
            await cargarCategorias();
            categoriasModal.show();
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudieron cargar las categorías'), 'error');
        }
    });

    $('#btnAsignarCategoria').on('click', async function () {
        const categoriaId = $('#categoriaAsignarId').val();
        if (!categoriaId) {
            Swal.fire('Atención', 'Seleccione una categoría.', 'warning');
            return;
        }
        try {
            await axios.post(`/Deporte/AsignarCategoria?deporteId=${deporteActualId}&categoriaId=${categoriaId}`);
            await cargarCategorias();
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo asignar'), 'error');
        }
    });

    $('#categoriasAsignadas').on('click', '.btn-quitar-categoria', async function () {
        try {
            await axios.post(`/Deporte/QuitarCategoria?id=${$(this).data('id')}`);
            await cargarCategorias();
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo quitar'), 'error');
        }
    });
});
