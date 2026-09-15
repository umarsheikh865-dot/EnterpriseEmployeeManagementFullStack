import axios from 'axios';

const apiClient = axios.create({
    baseURL: 'http://localhost:5094/api',
    headers: {
        'Content-Type': 'application/json'
    }
});

// Request interceptor to attach token
apiClient.interceptors.request.use(config => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
}, error => Promise.reject(error));

// Response interceptor to catch unauthorized errors globally
apiClient.interceptors.response.use(
    response => response,
    error => {
        if (error.response && error.response.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export default apiClient;