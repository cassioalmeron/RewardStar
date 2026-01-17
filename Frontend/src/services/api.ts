import axios from 'axios';

// Validate that VITE_API_URL is defined
const apiUrl = import.meta.env.VITE_API_URL;

if (!apiUrl) {
  throw new Error(
    'VITE_API_URL environment variable is not defined. Please check your .env file.'
  );
}

const api = axios.create({
  baseURL: apiUrl,
  timeout: 5000,
  headers: {
    'Content-Type': 'application/json',
    'ngrok-skip-browser-warning': 'true'
  },
});

// Request Interceptor: Automatically inject JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response Interceptor: Handle 401 Unauthorized globally
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid - clear token and redirect to login
      localStorage.removeItem('auth_token');
      window.location.hash = '#/login';
    }
    return Promise.reject(error);
  }
);

export { api };
export default api; 