/// <reference types="vite/client" />
import axios from 'axios';
import { setupMock } from './mock';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5097/api',
    headers: {
        'Content-Type': 'application/json'
    }
});

// Conditionally enable Mock Adapter
// Set to false to use real backend
const useMock = false; // import.meta.env.VITE_USE_MOCK === 'true';

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
