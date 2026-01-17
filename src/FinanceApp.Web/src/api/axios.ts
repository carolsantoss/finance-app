/// <reference types="vite/client" />
import axios from 'axios';
import { setupMock } from './mock';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || '/api',
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

import { useUIStore } from '../stores/ui';

api.interceptors.request.use(config => {
    const ui = useUIStore();
    ui.startLoading();

    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, error => {
    const ui = useUIStore();
    ui.stopLoading();
    return Promise.reject(error);
});

api.interceptors.response.use(response => {
    const ui = useUIStore();
    ui.stopLoading();
    return response;
}, error => {
    const ui = useUIStore();
    ui.stopLoading();
    return Promise.reject(error);
});

export default api;
