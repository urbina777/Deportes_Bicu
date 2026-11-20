//Formulario de inscripcion compartido (administracion y portal del estudiante)
//Uso: InscripcionForm.init({ urls: { opciones, estudiantes, categorias, horarios, guardar }, onSaved }); InscripcionForm.open();
window.InscripcionForm = (function () {
    let modal = null;
    let validator = null;
    let opts = null;
    let mostrarEstudiante = false;

    function init(options) {
        opts = options;
        mostrarEstudiante = $('#inscEstudianteId').length > 0;
        modal = new bootstrap.Modal('#inscripcionModal');

        validator = new JustValidate('#inscripcionForm', {
            validateBeforeSubmitting: true,
            focusInvalidField: true,
            errorFieldCssClass: 'is-invalid',
            successFieldCssClass: 'is-valid'
        });
        if (mostrarEstudiante) {
            validator.addField('#inscEstudianteId', [{ rule: 'required', errorMessage: 'Seleccione un estudiante.' }]);
        }
        validator
            .addField('#inscDeporteId', [{ rule: 'required', errorMessage: 'Seleccione un deporte.' }])
            .addField('#inscPeriodoId', [{ rule: 'required', errorMessage: 'Seleccione un período.' }])
            .addField('#inscCategoriaId', [{ rule: 'required', errorMessage: 'Seleccione una categoría.' }]);

        $('#inscDeporteId').on('change', async function () {
            await cargarCategorias();
            await cargarHorarios();
        });
        $('#inscPeriodoId').on('change', cargarHorarios);
        $('#inscHorarios').on('change', '.horario-check', function () {
            $('#inscHorariosError').addClass('d-none');
        });

        validator.onSuccess(guardar);
    }

    function llenar(selector, items, placeholder) {
        const select = $(selector).empty().append($('<option>').val('').text(placeholder));
        items.forEach(o => select.append($('<option>').val(o.id).text(o.texto)));
    }

    async function open() {
        try {
            const [ops, estudiantes] = await Promise.all([
                axios.get(opts.urls.opciones),
                mostrarEstudiante ? axios.get(opts.urls.estudiantes) : Promise.resolve(null)
            ]);
            if (estudiantes) llenar('#inscEstudianteId', estudiantes.data, 'Seleccione un estudiante');
            llenar('#inscDeporteId', ops.data.deportes, 'Seleccione un deporte');
            llenar('#inscPeriodoId', ops.data.periodos, 'Seleccione un período');
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudieron cargar las opciones'), 'error');
            return;
        }
        $('#inscCategoriaId').empty().append($('<option>').val('').text('Seleccione primero un deporte'));
        $('#inscHorarios').html('<span class="text-muted">Seleccione deporte y período para ver los horarios.</span>');
        $('#inscHorariosError').addClass('d-none');
        validator.refresh();
        modal.show();
    }

    async function cargarCategorias() {
        const deporteId = $('#inscDeporteId').val();
        if (!deporteId) {
            $('#inscCategoriaId').empty().append($('<option>').val('').text('Seleccione primero un deporte'));
            return;
        }
        try {
            const response = await axios.get(`${opts.urls.categorias}?deporteId=${deporteId}`);
            llenar('#inscCategoriaId', response.data, response.data.length ? 'Seleccione una categoría' : 'El deporte no tiene categorías');
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudieron cargar las categorías'), 'error');
        }
    }

    async function cargarHorarios() {
        const deporteId = $('#inscDeporteId').val();
        const periodoId = $('#inscPeriodoId').val();
        const cont = $('#inscHorarios');
        if (!deporteId || !periodoId) {
            cont.html('<span class="text-muted">Seleccione deporte y período para ver los horarios.</span>');
            return;
        }
        try {
            const response = await axios.get(`${opts.urls.horarios}?deporteId=${deporteId}&periodoId=${periodoId}`);
            if (response.data.length === 0) {
                cont.html('<span class="text-muted">El deporte no tiene horarios disponibles.</span>');
                return;
            }
            cont.empty();
            response.data.forEach(h => {
                const lleno = h.inscritos >= h.capacidad;
                const etiqueta = `${escapeHtml(h.dia)} ${horaCorta(h.horaInicio)} - ${horaCorta(h.horaFin)} · ${escapeHtml(h.instalacionNombre)} · ${escapeHtml(h.entrenadorNombre)}`;
                const cupo = lleno
                    ? '<span class="badge bg-danger ms-1">Sin cupo</span>'
                    : `<span class="badge bg-light text-dark border ms-1">Cupos: ${h.capacidad - h.inscritos} de ${h.capacidad}</span>`;
                cont.append(`<div class="form-check">
                    <input class="form-check-input horario-check" type="checkbox" id="insc-h-${h.id}" value="${h.id}" ${lleno ? 'disabled' : ''}>
                    <label class="form-check-label" for="insc-h-${h.id}">${etiqueta}${cupo}</label>
                </div>`);
            });
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudieron cargar los horarios'), 'error');
        }
    }

    async function guardar(event) {
        event.preventDefault();
        const horarioIds = $('.horario-check:checked').map(function () { return parseInt($(this).val()); }).get();
        if (horarioIds.length === 0) {
            $('#inscHorariosError').removeClass('d-none');
            return;
        }
        const payload = {
            deporteId: parseInt($('#inscDeporteId').val()),
            periodoId: parseInt($('#inscPeriodoId').val()),
            categoriaDeportivaId: parseInt($('#inscCategoriaId').val()),
            horarioIds: horarioIds
        };
        if (mostrarEstudiante) payload.estudianteId = parseInt($('#inscEstudianteId').val());

        try {
            const response = await axios.post(opts.urls.guardar, payload);
            if (response.data.success) {
                modal.hide();
                if (opts.onSaved) opts.onSaved();
                Swal.fire({ title: 'Listo', text: response.data.message, icon: 'success', timer: 2000, showConfirmButton: false });
            }
        } catch (error) {
            Swal.fire('Error', getErrorMessage(error, 'No se pudo registrar la inscripción.'), 'error');
        }
    }

    return { init, open };
})();
