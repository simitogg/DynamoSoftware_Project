import axios from 'axios';

const API_URL = 'http://localhost:5013/api/crypto';

export const uploadFile = async (file) => {
    try {
        const formData = new FormData();
        formData.append('file', file);

        const response = await axios.post(`${API_URL}/upload`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            },
            withCredentials: true,   // Ensure cookies are sent with the request
        });

        return response;
    } catch (error) {
        console.error('Error uploading file:', error);
        throw error;
    }
};

export const fetchPortfolioValue = async () => {
    try {
        const response = await axios.get(`${API_URL}/value`, {
            withCredentials: true  // Ensure cookies are sent with the request
        });
        return response.data;
    } catch (error) {
        console.error('Error fetching portfolio value:', error);
        throw error;
    }
};

export const setUpdateInterval = async (interval) => {
    try {
        const response = await axios.post(`${API_URL}/intervalUpdate`, interval, {
            headers: {
                'Content-Type': 'application/json', // Ensure JSON content type
            },
            withCredentials: true  // Ensure cookies are sent with the request
        });
        return response.data;
    } catch (error) {
        console.error('Error setting update interval:', error);
        throw error;
    }
};