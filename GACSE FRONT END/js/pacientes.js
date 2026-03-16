const Pacientes = {
    modal: null,
    deleteModal: null,
    form: null,

    init() {
        this.modal = new bootstrap.Modal(document.getElementById('pacienteModal'));
        this.deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
        this.form = document.getElementById('pacienteForm');
        this.load();
    },

    async load() {
        UI.showLoading(true);
        try {
            const pacientes = await ApiService.get('/pacientes');
            const tbody = document.getElementById('pacientesTableBody');
            tbody.innerHTML = '';

            pacientes.forEach(p => {
                const row = `
                    <tr>
                        <td><span class="badge bg-light text-dark border">${p.id}</span></td>
                        <td class="fw-bold">${p.nombre}</td>
                        <td>${new Date(p.fechaNacimiento).toLocaleDateString()}</td>
                        <td>
                            <div class="small">
                                <i class="bi bi-telephone text-muted me-2"></i>${p.telefono || 'N/A'}<br>
                                <i class="bi bi-envelope text-muted me-2"></i>${p.correoElectronico || 'N/A'}
                            </div>
                        </td>
                        <td class="text-end">
                            <button class="btn btn-sm btn-outline-success me-1" onclick="Pacientes.edit(${p.id})">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger" onclick="Pacientes.confirmDelete(${p.id})">
                                <i class="bi bi-trash"></i>
                            </button>
                        </td>
                    </tr>
                `;
                tbody.insertAdjacentHTML('beforeend', row);
            });
        } catch (error) {
            UI.showAlert('No se pudieron cargar los pacientes. ' + error.message, 'danger');
        } finally {
            UI.showLoading(false);
        }
    },

    showCreateModal() {
        this.form.reset();
        document.getElementById('pacienteId').value = '';
        document.getElementById('modalTitle').innerText = 'Nuevo Paciente';
        this.modal.show();
    },

    async edit(id) {
        UI.showLoading(true);
        try {
            const p = await ApiService.get(`/pacientes/${id}`);
            document.getElementById('pacienteId').value = p.id;
            document.getElementById('nombre').value = p.nombre;
            document.getElementById('fechaNacimiento').value = p.fechaNacimiento.substring(0, 10);
            document.getElementById('telefono').value = p.telefono || '';
            document.getElementById('correo').value = p.correoElectronico || '';

            document.getElementById('modalTitle').innerText = 'Editar Paciente';
            this.modal.show();
        } catch (error) {
            UI.showAlert('Error al obtener datos: ' + error.message, 'danger');
        } finally {
            UI.showLoading(false);
        }
    },

    async save() {
        const id = document.getElementById('pacienteId').value;
        const data = {
            nombre: document.getElementById('nombre').value,
            fechaNacimiento: document.getElementById('fechaNacimiento').value,
            telefono: document.getElementById('telefono').value,
            correoElectronico: document.getElementById('correo').value
        };

        if (!data.nombre || !data.fechaNacimiento) return UI.showAlert('Nombre y fecha son requeridos', 'warning');

        UI.showLoading(true);
        try {
            if (id) {
                await ApiService.put(`/pacientes/${id}`, { id: parseInt(id), ...data });
                UI.showAlert('Paciente actualizado con éxito', 'success');
            } else {
                await ApiService.post('/pacientes', data);
                UI.showAlert('Paciente registrado con éxito', 'success');
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
                await ApiService.delete(`/pacientes/${id}`);
                UI.showAlert('Paciente eliminado', 'success');
                this.deleteModal.hide();
                this.load();
            } catch (error) {
                UI.showAlert('No se pudo eliminar: ' + error.message, 'danger');
            } finally {
                UI.showLoading(false);
            }
        };
        this.deleteModal.show();
    }
};

document.addEventListener('DOMContentLoaded', () => Pacientes.init());
