const Citas = {
    medicos: [],
    selectedMedico: null,
    selectedDay: null,
    currentDays: [],
    occupiedSlots: new Set(),

    DIAS_NOMBRE: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
    MESES_NOMBRE: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
    DURACIONES: { 'MedicinaGeneral': 20, 'Cardiologia': 30, 'Cirugia': 45, 'Pediatria': 20, 'Ginecologia': 30 },
    ESPECIALIDAD_LABEL: {
        'MedicinaGeneral': 'Medicina General', 'Cardiologia': 'Cardiología',
        'Cirugia': 'Cirugía', 'Pediatria': 'Pediatría', 'Ginecologia': 'Ginecología'
    },

    async init() {
        await this.loadData();
    },

    async loadData() {
        UI.showLoading(true);
        try {
            const [medicos, pacientes] = await Promise.all([
                ApiService.get('/medicos'),
                ApiService.get('/pacientes')
            ]);
            this.medicos = medicos;

            const medSelect = document.getElementById('medicoId');
            medicos.forEach(m => {
                medSelect.add(new Option(`${m.nombre} (${this.ESPECIALIDAD_LABEL[m.especialidad] || m.especialidad})`, m.id));
            });

            const pacSelect = document.getElementById('pacienteId');
            pacientes.forEach(p => {
                pacSelect.add(new Option(p.nombre, p.id));
            });
        } catch (error) {
            UI.showAlert('Error cargando datos: ' + error.message, 'danger');
        } finally {
            UI.showLoading(false);
        }
    },

    // --- Toast notification ---
    showToast(message, icon = 'exclamation-triangle-fill') {
        const container = document.getElementById('toastContainer');
        const id = 'toast_' + Date.now();
        const html = `
            <div id="${id}" class="toast align-items-center border-0 shadow" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="alert alert-danger d-flex align-items-center mb-0 rounded">
                    <i class="bi bi-${icon} me-2 fs-5"></i>
                    <div class="flex-grow-1">${message}</div>
                    <button type="button" class="btn-close ms-2" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>`;
        container.insertAdjacentHTML('beforeend', html);
        const toastEl = document.getElementById(id);
        const toast = new bootstrap.Toast(toastEl, { delay: 4000 });
        toast.show();
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
    },

    onMedicoChange() {
        const id = document.getElementById('medicoId').value;
        const info = document.getElementById('medicoInfo');
        this.selectedMedico = this.medicos.find(m => m.id == id) || null;

        document.getElementById('diasContainer').classList.add('d-none');
        document.getElementById('horariosContainer').classList.add('d-none');
        document.getElementById('diasCards').innerHTML = '';

        if (!this.selectedMedico) {
            info.innerText = '';
            return;
        }

        const dur = this.DURACIONES[this.selectedMedico.especialidad] || 30;
        info.innerText = `Especialidad: ${this.ESPECIALIDAD_LABEL[this.selectedMedico.especialidad] || this.selectedMedico.especialidad}. Duración cita: ${dur} min.`;

        this.buildPeriodSelectors();
        this.renderDayCards();
    },

    // --- Week / Month selectors ---
    // Returns Monday of the week containing `date`
    getMonday(date) {
        const d = new Date(date);
        const day = d.getDay(); // 0=Sun
        const diff = day === 0 ? -6 : 1 - day;
        d.setDate(d.getDate() + diff);
        d.setHours(0, 0, 0, 0);
        return d;
    },

    buildPeriodSelectors() {
        const mesSel = document.getElementById('mesSelector');
        const semSel = document.getElementById('semanaSelector');
        mesSel.innerHTML = '';
        semSel.innerHTML = '';

        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const currentYear = today.getFullYear();

        // Build month options: all 12 months of current year + next year
        for (let y = currentYear; y <= currentYear + 1; y++) {
            for (let m = 0; m < 12; m++) {
                const opt = new Option(`${this.MESES_NOMBRE[m]} ${y}`, `${y}-${m}`);
                mesSel.add(opt);
            }
        }

        // Auto-select current month
        mesSel.value = `${currentYear}-${today.getMonth()}`;

        this.buildWeekOptions();
    },

    buildWeekOptions() {
        const semSel = document.getElementById('semanaSelector');
        semSel.innerHTML = '';

        const mesSel = document.getElementById('mesSelector');
        const [year, month] = mesSel.value.split('-').map(Number);

        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const todayMonday = this.getMonday(today);

        // Get all weeks that touch this month
        const firstOfMonth = new Date(year, month, 1);
        const lastOfMonth = new Date(year, month + 1, 0);

        let weekMonday = this.getMonday(firstOfMonth);
        let weekNum = 0;

        while (weekMonday <= lastOfMonth) {
            const weekSunday = new Date(weekMonday);
            weekSunday.setDate(weekSunday.getDate() + 6);

            weekNum++;
            const mondayStr = weekMonday.getDate().toString().padStart(2, '0');
            const sundayDay = weekSunday.getDate().toString().padStart(2, '0');
            const sundayMonth = weekSunday.getMonth() !== weekMonday.getMonth()
                ? ` ${this.MESES_NOMBRE[weekSunday.getMonth()].substring(0, 3)}`
                : '';
            const label = `Semana ${weekNum} (${mondayStr} - ${sundayDay}${sundayMonth})`;
            const val = this.formatDateISO(weekMonday);
            const opt = new Option(label, val);
            semSel.add(opt);

            weekMonday = new Date(weekMonday);
            weekMonday.setDate(weekMonday.getDate() + 7);
        }

        // Auto-select current week if in this month
        const currentMondayStr = this.formatDateISO(todayMonday);
        const options = Array.from(semSel.options);
        const currentOpt = options.find(o => o.value === currentMondayStr);
        if (currentOpt) {
            semSel.value = currentMondayStr;
        }
    },

    onMonthChange() {
        this.buildWeekOptions();
        // Clear horarios section — user must pick a day again
        document.getElementById('horariosContainer').classList.add('d-none');
        document.getElementById('slotsCards').innerHTML = '';
        this.selectedDay = null;
        this.renderDayCards();
    },

    async onWeekChange() {
        await this.renderDayCards();
        // Auto-select first clickable day of the week
        this.autoSelectFirstDay();
    },

    // Genera las cards de días de la semana calendario (Lun-Dom o Lun-Vie)
    async renderDayCards() {
        const container = document.getElementById('diasCards');
        container.innerHTML = '';

        const med = this.selectedMedico;
        if (!med) return;

        const horarioDias = new Map();
        med.horarios.forEach(h => horarioDias.set(h.diaSemana, h));

        // Determine if doctor works weekends
        const worksWeekend = horarioDias.has(0) || horarioDias.has(6);
        const totalDays = worksWeekend ? 7 : 5; // Mon-Sun or Mon-Fri

        // Get selected week's Monday
        const semSel = document.getElementById('semanaSelector');
        if (!semSel.value) return;
        const parts = semSel.value.split('-').map(Number);
        const monday = new Date(parts[0], parts[1] - 1, parts[2]);
        monday.setHours(0, 0, 0, 0);

        const days = [];
        for (let i = 0; i < totalDays; i++) {
            const d = new Date(monday);
            d.setDate(d.getDate() + i);
            const dow = d.getDay();
            const horario = horarioDias.get(dow) || null;
            days.push({ date: new Date(d), dow, horario });
        }

        this.currentDays = days;

        document.getElementById('diasContainer').classList.remove('d-none');

        // Cargar disponibilidad en paralelo solo para días con horario
        const promises = days.filter(day => day.horario).map(async (day) => {
            const fechaStr = this.formatDateISO(day.date);
            try {
                const agenda = await ApiService.get(`/citas/agenda?medicoId=${med.id}&fecha=${fechaStr}`);
                const citasActivas = (agenda.citas || []).filter(c => c.estado !== 'Cancelada');
                day.citasOcupadas = citasActivas;
            } catch {
                day.citasOcupadas = [];
            }
        });
        await Promise.all(promises);

        const today = new Date();
        today.setHours(0, 0, 0, 0);

        days.forEach(day => {
            const dur = this.DURACIONES[med.especialidad] || 30;
            const hasSchedule = !!day.horario;
            const fechaDisplay = day.date.toLocaleDateString('es-MX', { day: '2-digit', month: '2-digit', year: 'numeric' });
            const isToday = day.date.toDateString() === new Date().toDateString();
            const isPast = day.date < today && !isToday;

            let disponibles = 0;
            let badgeColor = 'secondary';
            let badgeText = 'No labora';

            if (hasSchedule && !isPast) {
                const totalSlots = this.calcTotalSlots(day.horario, dur);
                const ocupados = this.calcOcupados(day.citasOcupadas || [], day.horario, dur);
                disponibles = totalSlots - ocupados;
                badgeColor = disponibles === 0 ? 'danger' : disponibles <= 3 ? 'warning' : 'success';
                badgeText = `${disponibles} disponible${disponibles !== 1 ? 's' : ''}`;
            } else if (isPast && hasSchedule) {
                badgeColor = 'secondary';
                badgeText = 'Pasado';
            }

            const clickable = hasSchedule && !isPast;

            const col = document.createElement('div');
            col.className = 'col';
            col.innerHTML = `
                <div class="card day-card ${clickable ? 'hover-card' : ''} border-0 shadow-sm h-100"
                     style="cursor:${clickable ? 'pointer' : 'default'}; ${!hasSchedule || isPast ? 'opacity:0.5' : ''}"
                     data-date="${this.formatDateISO(day.date)}" data-dow="${day.dow}">
                    <div class="card-body text-center p-2">
                        <h6 class="fw-bold text-primary mb-1" style="font-size:0.85rem">${this.DIAS_NOMBRE[day.dow]}</h6>
                        <p class="small text-muted mb-2" style="font-size:0.75rem">${fechaDisplay}${isToday ? ' <span class="badge bg-info" style="font-size:0.6rem">Hoy</span>' : ''}</p>
                        <span class="badge bg-${badgeColor} px-2 py-1" style="font-size:0.75rem">
                            ${badgeText}
                        </span>
                    </div>
                </div>`;

            if (clickable) {
                col.querySelector('.day-card').addEventListener('click', () => {
                    this.selectDay(day);
                });
            }

            container.appendChild(col);
        });
    },

    autoSelectFirstDay() {
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        const firstClickable = this.currentDays.find(day => {
            const isToday = day.date.toDateString() === new Date().toDateString();
            const isPast = day.date < today && !isToday;
            return day.horario && !isPast;
        });
        if (firstClickable) {
            this.selectDay(firstClickable);
        }
    },

    calcTotalSlots(horario, duracion) {
        const inicio = this.tsToMinutes(horario.horaInicio);
        const fin = this.tsToMinutes(horario.horaFin);
        return Math.floor((fin - inicio) / duracion);
    },

    calcOcupados(citas, horario, duracion) {
        const slots = this.generateSlots(horario, duracion);
        let count = 0;
        for (const slot of slots) {
            if (this.isSlotOccupied(slot, duracion, citas, duracion)) count++;
        }
        return count;
    },

    generateSlots(horario, duracion) {
        const inicio = this.tsToMinutes(horario.horaInicio);
        const fin = this.tsToMinutes(horario.horaFin);
        const slots = [];
        for (let m = inicio; m + duracion <= fin; m += duracion) {
            slots.push(m);
        }
        return slots;
    },

    isSlotOccupied(slotMinutes, duracion, citas, medDuracion) {
        const slotEnd = slotMinutes + duracion;
        return citas.some(c => {
            const citaStart = this.tsToMinutes(c.hora);
            const citaEnd = citaStart + (c.duracionMinutos || medDuracion);
            return slotMinutes < citaEnd && slotEnd > citaStart;
        });
    },

    async selectDay(day) {
        this.selectedDay = day;
        const med = this.selectedMedico;
        const dur = this.DURACIONES[med.especialidad] || 30;
        const fechaStr = this.formatDateISO(day.date);

        // Recargar citas del día (frescas)
        try {
            const agenda = await ApiService.get(`/citas/agenda?medicoId=${med.id}&fecha=${fechaStr}`);
            day.citasOcupadas = (agenda.citas || []).filter(c => c.estado !== 'Cancelada');
        } catch { /* keep existing */ }

        const label = document.getElementById('diaSeleccionadoLabel');
        label.innerText = `${this.DIAS_NOMBRE[day.dow]} ${day.date.toLocaleDateString('es-MX')}`;

        const container = document.getElementById('slotsCards');
        container.innerHTML = '';
        this.occupiedSlots.clear();

        const slots = this.generateSlots(day.horario, dur);
        const now = new Date();

        slots.forEach(slotMin => {
            const slotEnd = slotMin + dur;
            const horaStr = this.minutesToHHMM(slotMin);
            const horaEndStr = this.minutesToHHMM(slotEnd);
            const horaDisplay = this.minutesToDisplay(slotMin);
            const horaEndDisplay = this.minutesToDisplay(slotEnd);
            const occupied = this.isSlotOccupied(slotMin, dur, day.citasOcupadas, dur);

            // Also check if slot is in the past
            const slotDateTime = new Date(day.date);
            slotDateTime.setHours(Math.floor(slotMin / 60), slotMin % 60, 0, 0);
            const isPast = slotDateTime <= now;

            const disabled = occupied || isPast;

            if (disabled) this.occupiedSlots.add(horaStr);

            const col = document.createElement('div');
            col.className = 'col-6 col-md-4 col-lg-3';
            col.innerHTML = `
                <div class="card slot-card border-0 shadow-sm ${disabled ? 'slot-occupied' : 'slot-available'}"
                     data-hora="${horaStr}" style="cursor:${disabled ? 'not-allowed' : 'pointer'}">
                    <div class="card-body text-center p-3">
                        <i class="bi ${disabled ? 'bi-x-circle text-white' : 'bi-check-circle text-white'} fs-4"></i>
                        <p class="fw-bold mb-0 mt-1 ${disabled ? 'text-white' : 'text-white'}">${horaDisplay} - ${horaEndDisplay}</p>
                        <small class="${disabled ? 'text-white-50' : 'text-white-50'}">${disabled ? (isPast && !occupied ? 'Pasado' : 'Ocupado') : 'Disponible'}</small>
                    </div>
                </div>`;

            if (!disabled) {
                col.querySelector('.slot-card').addEventListener('click', () => {
                    this.onSlotClick(horaStr, day);
                });
            }

            container.appendChild(col);
        });

        document.getElementById('horariosContainer').classList.remove('d-none');
        document.getElementById('horariosContainer').scrollIntoView({ behavior: 'smooth' });
    },

    async onSlotClick(hora, day) {
        const pacienteId = document.getElementById('pacienteId').value;
        const motivo = document.getElementById('motivo').value;

        if (!pacienteId) {
            return this.showToast('Seleccione un paciente antes de elegir horario.');
        }
        if (!motivo.trim()) {
            return this.showToast('Escriba el motivo de la consulta antes de elegir horario.');
        }

        const dur = this.DURACIONES[this.selectedMedico.especialidad] || 30;
        const slotMin = this.tsToMinutes(hora + ':00');
        const horaDisplay = this.minutesToDisplay(slotMin);
        const horaEndDisplay = this.minutesToDisplay(slotMin + dur);
        const fechaDisplay = day.date.toLocaleDateString('es-MX', { day: '2-digit', month: '2-digit', year: 'numeric' });

        // Show confirmation modal
        this.showConfirmModal(
            `¿Está seguro de querer reservar la cita?<br><br><strong>${this.DIAS_NOMBRE[day.dow]} ${fechaDisplay}</strong><br>${horaDisplay} - ${horaEndDisplay}`,
            async () => {
                const data = {
                    medicoId: this.selectedMedico.id,
                    pacienteId: parseInt(pacienteId),
                    fecha: this.formatDateISO(day.date),
                    hora: hora + ':00',
                    motivo: motivo.trim()
                };

                try {
                    const response = await ApiService.post('/citas', data);
                    let detail = `${this.DIAS_NOMBRE[day.dow]} ${fechaDisplay} de ${horaDisplay} a ${horaEndDisplay}`;
                    if (response.alertaCancelaciones) {
                        detail += `\nAviso: ${response.mensajeAlerta}`;
                    }
                    this.showSuccessInModal(detail);
                    await this.refreshAfterBooking(day);
                } catch (error) {
                    // Close confirm modal first
                    const cmEl = document.getElementById('confirmModal');
                    const cm = bootstrap.Modal.getInstance(cmEl);
                    if (cm) cm.hide();
                    if (error.status === 409) {
                        await this.handleConflictModal(error.data, day);
                    } else {
                        UI.showAlert(error.message, 'danger');
                    }
                }
            }
        );
    },

    showConfirmModal(message, onAccept) {
        // Reset modal to confirm state
        document.getElementById('confirmBody').classList.remove('d-none');
        document.getElementById('confirmSuccess').classList.add('d-none');
        document.getElementById('confirmFooter').classList.remove('d-none');
        document.getElementById('confirmSuccessFooter').classList.add('d-none');
        document.getElementById('confirmHeader').className = 'modal-header bg-info text-dark';
        document.getElementById('confirmTitle').innerHTML = '<i class="bi bi-question-circle me-2"></i>Confirmar Cita';
        document.getElementById('confirmMessage').innerHTML = message;

        const acceptBtn = document.getElementById('confirmAcceptBtn');
        const newBtn = acceptBtn.cloneNode(true);
        acceptBtn.parentNode.replaceChild(newBtn, acceptBtn);
        newBtn.disabled = false;
        newBtn.innerHTML = 'Aceptar';
        newBtn.addEventListener('click', async () => {
            newBtn.disabled = true;
            newBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Verificando...';
            await onAccept();
        });

        const el = document.getElementById('confirmModal');
        const modal = bootstrap.Modal.getInstance(el) || new bootstrap.Modal(el);
        modal.show();
    },

    showSuccessInModal(detail) {
        // Remove focus from the Verificando button before hiding its container
        if (document.activeElement) document.activeElement.blur();
        document.getElementById('confirmBody').classList.add('d-none');
        document.getElementById('confirmSuccess').classList.remove('d-none');
        document.getElementById('confirmFooter').classList.add('d-none');
        document.getElementById('confirmSuccessFooter').classList.remove('d-none');
        document.getElementById('confirmHeader').className = 'modal-header bg-success text-white';
        document.getElementById('confirmTitle').innerHTML = '<i class="bi bi-check-circle me-2"></i>Éxito';
        document.getElementById('confirmSuccessDetail').innerText = detail;
    },

    async refreshAfterBooking(day) {
        // Refrescar slots del día
        await this.selectDay(day);
        // Refrescar day cards
        await this.renderDayCards();
    },

    async handleConflictModal(data, day) {
        const horarios = data.horariosDisponibles || data.HorariosDisponibles || [];
        const mensaje = data.mensaje || data.Mensaje || 'El horario se acaba de ocupar.';

        const sugContainer = document.getElementById('modalSugerencias');
        sugContainer.innerHTML = '';

        // Marcar el slot que falló como rojo
        await this.selectDay(day);

        if (horarios.length > 0) {
            horarios.forEach(h => {
                const hora = (h.hora || h.Hora || '').substring(0, 5);
                const btn = document.createElement('button');
                btn.className = 'btn btn-outline-success btn-sm';
                btn.innerText = this.minutesToDisplay(this.tsToMinutes(hora + ':00'));
                btn.addEventListener('click', async () => {
                    const modal = bootstrap.Modal.getInstance(document.getElementById('conflictoModal'));
                    modal.hide();
                    await this.onSlotClick(hora, day);
                });
                sugContainer.appendChild(btn);
            });
        } else {
            sugContainer.innerHTML = '<em class="text-muted">No hay más horarios disponibles.</em>';
        }

        const conflictoEl = document.getElementById('conflictoModal');
        const modal = bootstrap.Modal.getInstance(conflictoEl) || new bootstrap.Modal(conflictoEl);
        modal.show();
    },

    // --- Utilidades ---
    tsToMinutes(ts) {
        const parts = ts.split(':');
        return parseInt(parts[0]) * 60 + parseInt(parts[1]);
    },

    minutesToHHMM(min) {
        const h = Math.floor(min / 60).toString().padStart(2, '0');
        const m = (min % 60).toString().padStart(2, '0');
        return `${h}:${m}`;
    },

    minutesToDisplay(min) {
        let h = Math.floor(min / 60);
        const m = (min % 60).toString().padStart(2, '0');
        const ampm = h >= 12 ? 'PM' : 'AM';
        if (h === 0) h = 12;
        else if (h > 12) h -= 12;
        return `${h}:${m} ${ampm}`;
    },

    formatDateISO(d) {
        const y = d.getFullYear();
        const m = (d.getMonth() + 1).toString().padStart(2, '0');
        const day = d.getDate().toString().padStart(2, '0');
        return `${y}-${m}-${day}`;
    }
};

document.addEventListener('DOMContentLoaded', () => Citas.init());
