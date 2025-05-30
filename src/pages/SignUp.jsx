import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { FaGoogle, FaApple, FaFacebookF } from 'react-icons/fa';
import img from '../assets/9686824114_fce65687ac_b.jpg'; // Adjust the path as necessary

function SignUp() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  // const handleSignUp = (e) => {
  //   e.preventDefault();
  //   const users = JSON.parse(localStorage.getItem('users')) || [];
  //   const exists = users.find(u => u.email === email || u.username === username);

  //   if (exists) {
  //     setError('User with same email or username already exists');
  //   } else {
  //     const newUser = { email, username, password };
  //     localStorage.setItem('users', JSON.stringify([...users, newUser]));
  //     localStorage.setItem('loggedInUser', JSON.stringify(newUser));
  //     navigate('/');
  //   }
  // };

  const handleSignUp = async (e) => {
    e.preventDefault();
    setError('');
  
    try {
      const response = await fetch("http://localhost:5007/api/user/signup", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ email, username, password }),
      });
  
      if (response.ok) {
        const data = await response.json();
        console.log("Signup successful:", data);
        console.log("Signup response:", data);
        localStorage.setItem('loggedInUser', JSON.stringify(data));
        navigate('/');
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Signup failed');
      }
    } catch (error) {
      setError("Network error. Please try again.");
      console.error("Signup error:", error);
    }
  };

  return (
    <div className="flex h-screen w-screen bg-cyan-950 text-white">
      {/* Left Panel */}
      <div className="w-full md:w-1/2 flex items-center justify-center bg-gradient-to-bl from-[rgb(7,7,7)] via-[rgb(65,40,10)] to-[rgb(136,58,10)] text-white px-8 py-12">
        <div className="w-full max-w-sm">
          <h1 className="text-3xl font-bold bg-gradient-to-r from-slate-300 via-slate-100 to-slate-400 bg-clip-text text-transparent mb-6 text-center font-serif">IT CONSUMABLES</h1>
          <div className="flex justify-center mb-4 space-x-3">
            <button className="bg-gray-100 p-2 rounded-full"><FaGoogle className="text-black" /></button>
            <button className="bg-gray-100 p-2 rounded-full"><FaFacebookF className="text-black" /></button>
            <button className="bg-gray-100 p-2 rounded-full"><FaApple className="text-black" /></button>
          </div>
          <form onSubmit={handleSignUp} className="space-y-4">
            <input
              type="text"
              placeholder="@_username"
              className="w-full px-4 py-2 border border-gray-300 text-slate-600 rounded bg-white"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
            <input
              type="email"
              placeholder="Email address"
              className="w-full px-4 py-2 border border-gray-300 text-slate-600 rounded bg-white"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
            <input
              type="password"
              placeholder="Password"
              className="w-full px-4 py-2 border border-gray-300 text-slate-600 rounded bg-white"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
            {error && <p className="text-red-500 text-sm">{error}</p>}
            <button type="submit" className="w-full bg-black text-white py-2 rounded hover:bg-gray-900">Sign up</button>
          </form>
          <p className="text-sm text-center mt-4">
            Already have an account?{' '}
            <span onClick={() => navigate('/login')} className="text-blue-600 hover:underline cursor-pointer">Log in</span>
          </p>
        </div>
      </div>

      {/* Right Panel */}
      <div className="hidden md:block w-1/2 relative">
      <img
        src={img}
        alt="Scenic background"
        className="w-full h-full object-cover"
      />
      <div className="absolute inset-0 bg-black opacity-40 z-0"></div>
        <div className="absolute top-0 left-0 right-0 bottom-0 bg-transparent flex items-center justify-center p-6">
          <div className="text-center text-white max-w-sm">
            {/* <p className="text-sm font-semibold uppercase tracking-wider mb-2">Wonder. Explore. Experience.</p>
            <h2 className="text-2xl font-bold mb-2">Escape the Ordinary, Embrace the Journey</h2>
            <p className="text-sm">Begin your next adventure with us.</p> */}
          </div>
        </div>
      </div>
    </div>
  );
}

export default SignUp;