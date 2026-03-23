// src/api.js
import axios from "axios";

const BASE_URL = `${import.meta.env.VITE_BACKEND_URI}/api/user`;

export const signupUser = async (userData) => {
  try {
    const response = await axios.post(`${BASE_URL}/signup`, userData);
    return response.data;
  } catch (error) {
    throw error.response?.data || { message: "Signup failed" };
  }
};