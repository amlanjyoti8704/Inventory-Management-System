import React, { useState } from 'react';
import { useNavigate, Link, NavLink } from 'react-router-dom';
import bgImg from '../assets/Industry_in_deosar_tehsil.jpeg';

function Login() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false); // ✅ New state
  const [error, setError] = useState('');

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
  
    try {
      const response = await fetch("http://localhost:5007/api/user/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ email, password })
      });
  
      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('loggedInUser', JSON.stringify(data));
        localStorage.setItem('email', data.user.email);
        localStorage.setItem('role', data.user.role);
  
        if (data.user.role === 'admin') {
          navigate('/');
        } else {
          navigate('/');
        }
      } else {
        const errorData = await response.json();
        setError(errorData.message || "Login failed");
      }
    } catch (err) {
      setError("Network error. Please try again.");
      console.error("Login error:", err);
    }
  };

  return (
    <div
      className="flex items-center justify-center h-screen bg-cover bg-center relative"
      style={{ backgroundImage: `url(${bgImg})` }}
    >
      <div className="absolute inset-0 bg-black opacity-70 z-0"></div>

      <div className='relative z-10 rounded-2xl p-6 border border-slate-500 w-[90vw] md:w-[50vw] h-auto md:h-[50vh] shadow-lg flex flex-col md:flex-row justify-center items-center'>
        <div className="absolute rounded-2xl inset-0 bg-black opacity-40 z-0"></div>
        <div className="hidden md:block w-1/2"></div>

        <div className='z-10 bg-black opacity-53 rounded-xl p-6'>
          <form onSubmit={handleLogin} className="text-white w-80">
            <h2 className="text-xl font-semibold mb-4 text-center">Login</h2>
            {error && <p className="text-red-500 mb-3 text-sm">{error}</p>}

            <input
              type="email"
              placeholder="Email"
              className="w-full mb-3 px-3 py-2 border border-gray-400 bg-transparent text-white placeholder-gray-300 rounded"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />

            <input
              type={showPassword ? "text" : "password"} // ✅ Toggle input type
              placeholder="Password"
              className="w-full mb-1 px-3 py-2 border border-gray-400 bg-transparent text-white placeholder-gray-300 rounded"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />

            <div className="flex items-center justify-between text-sm mb-3">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  checked={showPassword}
                  onChange={() => setShowPassword(!showPassword)} // ✅ Toggle state
                  className="mr-2"
                />
                Show Password
              </label>
              <Link to="/forgot-password" className="text-blue-400 hover:underline">Forgot password?</Link>            </div>

            <div className='flex items-center justify-between mb-4 text-sm'>
              <label className="flex items-center">
                <input
                  type="checkbox"
                  id="rememberMe"
                  name="rememberMe"
                  className="mr-2"
                />
                Remember me
              </label>
            </div>

            <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded w-full">
              Login
            </button>
          </form>

          <div className="mt-4 text-center text-sm">
            <p>Don't have an account?</p>
            <Link to="/SignUp" className="text-blue-300 font-semibold hover:underline">
              Create account
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Login;