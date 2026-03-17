const Medicos = {
    modal: null,
    deleteModal: null,
    form: null,

    init() {
        this.modal = new bootstrap.Modal(document.getElementById('medicoModal'));
        this.deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        this.form = document.getElementById('medicoForm');
        this.load();
    },

    async load() {
        UI.showLoading(true);
        try {
            const medicos = await ApiService.get('/medicos');
            const tbody = document.getElementById('medicosTableBody');
            tbody.innerHTML = '';

            medicos.forEach(m => {
                const row = `
                    <tr>
                        <td><span class="badge bg-light text-dark border">${m.id}</span></td>
                        <td class="fw-bold">${m.nombre}</td>
                        <td><span class="badge bg-info text-dark">${this.formatEspecialidad(m.especialidad)}</span></td>
                        <td><small>${this.formatHorarios(m.horarios)}</small></td>
                        <td class="text-end">
                            <button class="btn btn-sm btn-outline-primary me-1" onclick="Medicos.edit(${m.id})">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger" onclick="Medicos.confirmDelete(${m.id})">
                                <i class="bi bi-trash"></i>
                            </button>
                        </td>
                    </tr>
                `;
                tbody.insertAdjacentHTML('beforeend', row);
            });
        } catch (error) {
            UI.showAlert('No se pudieron cargar los médicos. ' + error.message, 'danger');
        } finally {
            UI.showLoading(false);
        }
    },

    showCreateModal() {
        this.form.reset();
        document.getElementById('medicoId').value = '';
        document.getElementById('modalTitle').innerText = 'Nuevo Médico';
        this.modal.show();
    },

    async edit(id) {
        UI.showLoading(true);
        try {
            const m = await ApiService.get(`/medicos/${id}`);
            document.getElementById('medicoId').value = m.id;
            document.getElementById('nombre').value = m.nombre;
            document.getElementById('especialidad').value = m.especialidad;
            
            // Asignar primer horario encontrado o default
            if (m.horarios && m.horarios.length > 0) {
                document.getElementById('horaInicio').value = m.horarios[0].horaInicio.substring(0, 5);
                document.getElementById('horaFin').value = m.horarios[0].horaFin.substring(0, 5);
            }

            document.getElementById('modalTitle').innerText = 'Editar Médico';
            this.modal.show();
        } catch (error) {
            UI.showAlert('Error al obtener datos del médico: ' + error.message, 'danger');
        } finally {
            UI.showLoading(false);
        }
    },

    async save() {
        const id = document.getElementById('medicoId').value;
        const medicoData = {
            nombre: document.getElementById('nombre').value,
            especialidad: document.getElementById('especialidad').value,
            // En este frontend simplificado, enviamos horarios para Lunes-Viernes
            horarios: [1, 2, 3, 4, 5].map(dia => ({
                diaSemana: dia,
                horaInicio: document.getElementById('horaInicio').value + ':00',
                horaFin: document.getElementById('horaFin').value + ':00'
            }))
        };

        if (!medicoData.nombre) return UI.showAlert('El nombre es requerido', 'warning');

        UI.showLoading(true);
        try {
            if (id) {
                await ApiService.put(`/medicos/${id}`, { id: parseInt(id), ...medicoData });
                UI.showAlert('Médico actualizado con éxito', 'success');
            } else {
                await ApiService.post('/medicos', medicoData);
                UI.showAlert('Médico creado con éxito', 'success');
            }
            this.modal.hide();
            this.load();
        } catch (error) {
            UI.showAlert('Error al guardar: ' + error.message, 'danger');
        } finally {
            UI.showLoading(false);
        }
    },

    confirmDelete(id) {
        const btn = document.getElementById('confirmDeleteBtn');
        btn.onclick = async () => {
            UI.showLoading(true);
            try {
                await ApiService.delete(`/medicos/${id}`);
                UI.showAlert('Médico eliminado correctamente', 'success');
                this.deleteModal.hide();
                this.load();
            } catch (error) {
                UI.showAlert('No se pudo eliminar: ' + error.message, 'danger');
            } finally {
                UI.showLoading(false);
            }
        };
        this.deleteModal.show();
    },

    formatEspecialidad(esp) {
        const map = {
            'MedicinaGeneral': 'Medicina General',
            'Cardiologia': 'Cardiología',
            'Cirugia': 'Cirugía',
            'Pediatria': 'Pediatría',
            'Ginecologia': 'Ginecología'
        };
        return map[esp] || esp;
    },

    formatHorarios(horarios) {
        if (!horarios || horarios.length === 0) return 'Sin horario';
        // Agrupar si son iguales (simplificación)
        const h = horarios[0];
        const dias = horarios.length === 5 ? 'Lun-Vie' : 'Varios días';
        return `${dias}: ${h.horaInicio.substring(0, 5)} - ${h.horaFin.substring(0, 5)}`;
    }
};

document.addEventListener('DOMContentLoaded', () => Medicos.init());
