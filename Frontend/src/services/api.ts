import axios from 'axios';

const api = axios.create({
  baseURL: localStorage.getItem('apiUrl') || import.meta.env.VITE_API_URL,
  timeout: 5000,
  headers: {
    'Content-Type': 'application/json',
    'ngrok-skip-browser-warning': 'true'
  },
});

export default api; 