import axios from 'axios';
import { setupMock } from './mock';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
    headers: {
        'Content-Type': 'application/json'
    }
});

// Conditionally enable Mock Adapter
// Forcing true for now as per user request to "not connect to backend"
const useMock = true; // import.meta.env.VITE_USE_MOCK === 'true';

if (useMock) {
    setupMock(api);
}

api.interceptors.request.use(config => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export default api;
