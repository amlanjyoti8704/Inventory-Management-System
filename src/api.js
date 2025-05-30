// src/api.js
import axios from "axios";

const BASE_URL = "http://localhost:5007/api/user";

export const signupUser = async (userData) => {
  try {
    const response = await axios.post(`${BASE_URL}/signup`, userData);
    return response.data;
  } catch (error) {
    throw error.response?.data || { message: "Signup failed" };
  }
};