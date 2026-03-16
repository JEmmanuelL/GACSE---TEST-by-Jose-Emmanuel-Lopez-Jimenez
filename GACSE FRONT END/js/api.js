/**
 * GACSE API Service
 * Centraliza todas las peticiones fetch a la API .NET 8
 */
// Usar ruta relativa para que funcione tanto en desarrollo local como en Docker
const API_BASE_URL = '/api';

const ApiService = {
    /**
     * Realiza una petición genérica
     */
    async request(endpoint, options = {}) {
        const url = `${API_BASE_URL}${endpoint}`;
        
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
        };

        const config = { ...defaultOptions, ...options };

        try {
            const response = await fetch(url, config);
            const data = await this.parseResponse(response);

            if (!response.ok) {
                // Manejo especial para conflicto de horario (409)
                if (response.status === 409) {
                    throw { status: 409, data };
                }
                
                const errorMessage = data && data.mensaje ? data.mensaje : `Error ${response.status}: ${response.statusText}`;
                throw new Error(errorMessage);
            }

            return data;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },

    async parseResponse(response) {
        const text = await response.text();
        try {
            return text ? JSON.parse(text) : null;
        } catch (e) {
            // Si no es JSON y es un código de éxito, algo está mal configurado
            if (response.ok) {
                console.error('La API devolvió un formato no válido:', text);
                return []; // Retornar arreglo vacío para evitar errores de .forEach
            }
            return text;
        }
    },

    // Métodos específicos
    get: (endpoint) => ApiService.request(endpoint, { method: 'GET' }),
    
    post: (endpoint, body) => ApiService.request(endpoint, {
        method: 'POST',
        body: JSON.stringify(body)
    }),
    
    put: (endpoint, body) => ApiService.request(endpoint, {
        method: 'PUT',
        body: JSON.stringify(body)
    }),
    
    delete: (endpoint) => ApiService.request(endpoint, { method: 'DELETE' })
};

// Utilidad para mostrar alertas usando Bootstrap
const UI = {
    showAlert(message, type = 'info', containerId = 'alertContainer') {
        const container = document.getElementById(containerId);
        if (!container) return;

        const wrapper = document.createElement('div');
        wrapper.innerHTML = `
            <div class="alert alert-${type} alert-dismissible fade show shadow-sm" role="alert">
                <div>${message}</div>
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        container.append(wrapper);
        
        // Auto cerrar después de 5 segundos si es éxito
        if (type === 'success') {
            setTimeout(() => {
                const alert = wrapper.querySelector('.alert');
                if (alert) {
                    const bsAlert = new bootstrap.Alert(alert);
                    bsAlert.close();
                }
            }, 5000);
        }
    },

    showLoading(show = true) {
        let loader = document.getElementById('loadingOverlay');
        if (show) {
            if (!loader) {
                loader = document.createElement('div');
                loader.id = 'loadingOverlay';
                loader.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div>';
                document.body.appendChild(loader);
            }
            loader.style.display = 'flex';
        } else if (loader) {
            loader.style.display = 'none';
        }
    }
};
