import React, { useState } from 'react';

function ForgotPassword() {
  const [mode, setMode] = useState('email'); // 'email' or 'phone'
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [token, setToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [step, setStep] = useState(1); // 1 = enter contact, 2 = enter token+password
  const [resetLink, setResetLink] = useState('');

  const handleEmailReset = async (e) => {
    e.preventDefault();
    setError('');
    setMessage('');



    try {
      const res = await fetch('https://my-backend-sdbk.onrender.com/api/user/forgot-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email }),
      });

      const data = await res.json();
      if (res.ok) {
        setMessage(data.message || 'Reset link sent!');
      } else {
        setError(data.message || 'Error sending reset link');
      }
      if (res.ok) {
        const token = data.token; // only if your backend sends the token
        const link = `http://localhost:5173/reset-password?token=${token}`;
        setResetLink(link);
        setMessage(data.message || 'Reset link sent!');
      }
    } catch (err) {
      setError('Network error. Try again later.');
    }
  };

  const requestOtp = async (e) => {
    e.preventDefault();
    setError('');
    setMessage('');

    try {
      const res = await fetch('https://my-backend-sdbk.onrender.com/api/user/request-password-reset', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ phoneNumber: phone }),
      });

      const data = await res.json();
      if (res.ok) {
        setMessage(data.message || 'OTP sent!');
        setStep(2);
      } else {
        setError(data.message || 'Error sending OTP');
      }
    } catch (err) {
      setError('Network error.');
    }
  };

  const verifyAndReset = async (e) => {
    e.preventDefault();
    setError('');
    setMessage('');

    try {
      const res = await fetch('https://my-backend-sdbk.onrender.com/api/user/verify-token-reset', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          phoneNumber: phone,
          token,
          newPassword,
        }),
      });

      const data = await res.json();
      if (res.ok) {
        setMessage(data.message || 'Password reset successful!');
        setStep(1);
        setPhone('');
        setToken('');
        setNewPassword('');
      } else {
        setError(data.message || 'Failed to reset password');
      }
    } catch (err) {
      setError('Network error.');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center text-white px-4"
         style={{ backgroundImage: `url('src/assets/closed-padlock-on-digital-background-cyber-security-free-vector.jpg')`, backgroundSize: 'cover', backgroundPosition: 'center' }}
    >
      <div className="absolute inset-0 backdrop-blur-sm bg-black/40"></div>

      <form
        onSubmit={
          mode === 'email' ? handleEmailReset :
          step === 1 ? requestOtp :
          verifyAndReset
        }
        className="bg-black/50 border border-white p-10 rounded-lg shadow-md w-full max-w-xl z-10"
      >
        <h2 className="text-xl text-center font-bold mb-4">Forgot Password</h2>

        <div className="flex justify-center gap-4 mb-4">
          <button type="button" onClick={() => setMode('email')} className={`px-4 py-2 rounded ${mode === 'email' ? 'bg-blue-600' : 'bg-gray-600'}`}>Via Email</button>
          <button type="button" onClick={() => { setMode('phone'); setStep(1); }} className={`px-4 py-2 rounded ${mode === 'phone' ? 'bg-blue-600' : 'bg-gray-600'}`}>Via Phone</button>
        </div>

        {message && <p className="text-green-400 mb-2">{message}</p>}
        {error && <p className="text-red-400 mb-2">{error}</p>}

        {/* Email Mode */}
        {mode === 'email' && (
          <>
            <input
              type="email"
              placeholder="Enter your email"
              className="w-full p-2 mb-3 rounded bg-gray-700 text-white"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
            <button className="w-full bg-blue-600 hover:bg-blue-700 p-2 rounded">
              Send Reset Link
            </button>
          </>
        )}
        {resetLink && (
        <div className="bg-gray-800 border border-blue-500 p-4 rounded-md mt-4">
            <p className="text-blue-400 text-sm break-all">
            Dev Reset Link:{" "}
            <a
                href={resetLink}
                className="underline"
                target="_blank"
                rel="noopener noreferrer"
            >
                {resetLink}
            </a>
            </p>
        </div>
        )}

        {/* Phone Mode Step 1 */}
        {mode === 'phone' && step === 1 && (
          <>
            <input
              type="text"
              placeholder="Enter phone number"
              className="w-full p-2 mb-3 rounded bg-gray-700 text-white"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              required
            />
            <button className="w-full bg-blue-600 hover:bg-blue-700 p-2 rounded">
              Send OTP
            </button>
          </>
        )}

        {/* Phone Mode Step 2 */}
        {mode === 'phone' && step === 2 && (
          <>
            <input
              type="text"
              placeholder="Enter OTP"
              className="w-full p-2 mb-3 rounded bg-gray-700 text-white"
              value={token}
              onChange={(e) => setToken(e.target.value)}
              required
            />
            <input
              type="password"
              placeholder="Enter new password"
              className="w-full p-2 mb-3 rounded bg-gray-700 text-white"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />
            <button className="w-full bg-green-600 hover:bg-green-700 p-2 rounded">
              Reset Password
            </button>
          </>
        )}
      </form>
    </div>
  );
}

export default ForgotPassword;