const Agenda = {
    async init() {
        const today = new Date().toISOString().split('T')[0];
        document.getElementById('fecha').value = today;
        await this.loadMedicos();
    },

    async loadMedicos() {
        try {
            const medicos = await ApiService.get('/medicos');
            const medSelect = document.getElementById('medicoId');
            medicos.forEach(m => {
                const opt = new Option(m.nombre, m.id);
                medSelect.add(opt);
            });
        } catch (error) {
            UI.showAlert('Error cargando médicos: ' + error.message, 'danger');
        }
    },

    async search() {
        const medicoId = document.getElementById('medicoId').value;
        const fecha = document.getElementById('fecha').value;

        if (!medicoId || !fecha) {
            return UI.showAlert('Seleccione médico y fecha para buscar', 'warning');
        }

        UI.showLoading(true);
        try {
            const agenda = await ApiService.get(`/citas/agenda?medicoId=${medicoId}&fecha=${fecha}`);
            this.displayResults(agenda);
        } catch (error) {
            UI.showAlert('Error al consultar: ' + error.message, 'danger');
        } finally {
            UI.showLoading(false);
        }
    },

    displayResults(data) {
        const container = document.getElementById('resultsContainer');
        const tbody = document.getElementById('agendaTableBody');
        const noResults = document.getElementById('noResults');
        const table = tbody.closest('table');

        document.getElementById('agendaTitle').innerText = `Agenda de ${data.nombreMedico}`;
        document.getElementById('fechaBadge').innerText = new Date(data.fecha).toLocaleDateString();

        tbody.innerHTML = '';
        container.classList.remove('d-none');

        if (data.citas && data.citas.length > 0) {
            table.classList.remove('d-none');
            noResults.classList.add('d-none');

            data.citas.forEach(c => {
                const horaInicio = c.hora.substring(0, 5);
                const durMin = c.duracionMinutos || 20;
                const parts = c.hora.split(':');
                const totalMin = parseInt(parts[0]) * 60 + parseInt(parts[1]) + durMin;
                const hEnd = Math.floor(totalMin / 60).toString().padStart(2, '0');
                const mEnd = (totalMin % 60).toString().padStart(2, '0');
                const horaFin = `${hEnd}:${mEnd}`;
                const paciente = c.nombrePaciente || 'Sin asignar';
                const row = `
                    <tr>
                        <td class="ps-4 fw-bold text-primary">${horaInicio} - ${horaFin}</td>
                        <td>${paciente}</td>
                        <td class="small">${c.motivo}</td>
                        <td>${this.formatEstado(c.estado)}</td>
                        <td>
                            ${c.estado === 'Programada' ? 
                                `<button class="btn btn-sm btn-outline-danger" onclick="Agenda.cancelar(${c.id})">Cancelar</button>` : 
                                '-'
                            }
                        </td>
                    </tr>
                `;
                tbody.insertAdjacentHTML('beforeend', row);
            });
        } else {
            table.classList.add('d-none');
            noResults.classList.remove('d-none');
        }
    },

    async cancelar(id) {
        if (!confirm('¿Seguro que desea cancelar esta cita?')) return;

        UI.showLoading(true);
        try {
            await ApiService.put(`/citas/${id}/cancelar`, {});
            UI.showAlert('Cita cancelada correctamente', 'success');
            this.search(); // Recargar datos
        } catch (error) {
            UI.showAlert('Error al cancelar: ' + error.message, 'danger');
        } finally {
            UI.showLoading(false);
        }
    },

    formatEstado(estado) {
        const map = {
            'Programada': '<span class="badge bg-primary">Programada</span>',
            'Completada': '<span class="badge bg-success">Completada</span>',
            'Cancelada': '<span class="badge bg-danger">Cancelada</span>'
        };
        return map[estado] || estado;
    }
};

document.addEventListener('DOMContentLoaded', () => Agenda.init());
