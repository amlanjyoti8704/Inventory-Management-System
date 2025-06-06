import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import axios from 'axios';

function ResetPassword() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [message, setMessage] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setMessage('Passwords do not match');
      return;
    }

    try {
      const res = await axios.post('http://localhost:5007/api/user/reset-password', {
        token,
        newPassword,
      });
      setMessage(res.data.message);
    } catch (err) {
      setMessage(err.response?.data?.message || 'Error resetting password');
    }
  };

  return (
    <div>
       <div className="min-h-screen flex flex-col items-center justify-center text-white px-4"
         style={{ backgroundImage: `url('src/assets/closed-padlock-on-digital-background-cyber-security-free-vector.jpg')`, backgroundSize: 'cover', backgroundPosition: 'center' }}
        >
        <div className="absolute inset-0 backdrop-blur-sm bg-black/40"></div>

        <h2>Reset Your Password</h2>
        {message && <p>{message}</p>}
        <form onSubmit={handleSubmit} className='bg-black/40 h-[20vh] border border-white  p-6 rounded shadow-md w-[30vw] flex flex-col items-center justify-center space-y-4 z-10'>
            <input
            type="password"
            placeholder="New password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
            className='w-full px-3 py-2 border border-gray-400 bg-transparent text-white placeholder-gray-300 rounded'
            required
            />
            <input
            type="password"
            placeholder="Confirm new password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            className='w-full px-3 py-2 border border-gray-400 bg-transparent text-white placeholder-gray-300 rounded'
            required
            />
            <button className='bg-blue-500 text-white w-full rounded-4xl' type="submit">Reset Password</button>
        </form>
      </div>
    </div>
  );
}

export default ResetPassword;