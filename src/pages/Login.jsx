import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';

function Login() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  // const handleLogin = (e) => {
  //   e.preventDefault();

  //   const users = JSON.parse(localStorage.getItem('users')) || [];
  //   const user = users.find(u => u.email === email);

  //   if (user && user.password === password) {
  //     localStorage.setItem('loggedInUser', JSON.stringify(user));
  //     navigate('/'); // redirect to home/dashboard
  //   } else {
  //     setError('Invalid email or password');
  //   }
  // };

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
        console.log("Login successful:", data);
        localStorage.setItem('loggedInUser', JSON.stringify(data)); // store the user object directly
        navigate('/');
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
    <div className="flex items-center justify-center h-screen relative"
      style={{ backgroundImage: "url('src/assets/Industry_in_deosar_tehsil.jpeg')", backgroundSize: 'cover', backgroundPosition: 'center' }}
    >
      <div className="absolute inset-0 bg-black opacity-70 z-0"></div>

      <div className='relative rounded-2xl p-6 border border-slate-500 w-[50vw] h-[50vh] shadow-lg mx-auto flex flex-row justify-center items-center'>
      <div className="absolute rounded-2xl inset-0 bg-black opacity-40 z-0"></div>
        <div className="w-1/2 hidden md:block">
        </div>

        <div className='z-10 bg-black opacity-53 rounded-xl '>
            <form onSubmit={handleLogin} className="bg-transparent p-6 rounded-2xl shadow-md w-80 text-white">
            <h2 className="text-xl font-semibold mb-4 text-center">Login</h2>
            {error && <p className="text-red-500 mb-2 text-sm">{error}</p>}
            <input
                type="email"
                placeholder="Email"
                className="w-full mb-3 px-3 py-2 border border-gray-400 bg-transparent text-white placeholder-gray-300 rounded"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
            />
            <input
                type="password"
                placeholder="Password"
                className="w-full mb-3 px-3 py-2 border border-gray-400 bg-transparent text-white placeholder-gray-300 rounded"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
            />
            <div className='flex items-center justify-between mb-4'>
                <label className="flex items-center text-sm text-white">
                    <input 
                    type="checkbox"
                    id="rememberMe"
                    name="rememberMe"
                    className="mr-2"
                    />
                    Remember me
                </label>
                <a href="#" className="text-sm text-blue-400 hover:underline">Forgot password?</a>
            </div>
            <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded w-full">
                Login
            </button>
            </form>
            <div className="mb-2.5 text-sm text-center">
                <p className='text-white'>Don't have an account?</p>
                <Link to="/SignUp" className="text-white font-bold hover:underline">Create account</Link>
            </div>
        </div>
       
      </div>
    </div>
  );
}

export default Login;