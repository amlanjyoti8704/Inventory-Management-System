import React, { useEffect, useState } from 'react';
import { TypeAnimation } from 'react-type-animation';
import background_image from '../assets/background_img.png';
import bg_img from '../assets/6BEF965A-A37E-451D-9714-9F140F7C94B9_1_201_a.jpeg';
import { ChevronDownIcon } from '@heroicons/react/24/outline';
import axios from 'axios';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer
} from 'recharts';

function Home() {
  const [userRole, setUserRole] = useState('');
  const [userEmail, setUserEmail] = useState('');

  useEffect(() => {
    window.scrollTo(0, 0);
    const fetchCurrentUser = async () => {
      try {
        const storedUserStr = localStorage.getItem('loggedInUser');
        if (!storedUserStr) {
          console.error("No user found in localStorage");
          setLoading(false);
          return;
        }
    
        const storedUser = JSON.parse(storedUserStr);
        const user = storedUser?.user;
    
        if (!user?.email) {
          console.error("No email found in localStorage");
          setLoading(false);
          return;
        }
    
        const res = await axios.get(`https://my-backend-sdbk.onrender.com/api/user/me?email=${user.email}`);
        setUserRole(res.data.role?.toLowerCase());
        setUserEmail(res.data.email);
      } catch (err) {
        console.error("Error fetching user:", err);
        alert("Failed to fetch user information. Please try again.");
      } finally {
        setLoading(false);
      }
    };
  
    fetchCurrentUser();
    fetchDashboardData();
  }, []);

  const [summary, setSummary] = useState({
    totalCategories: 0,
    totalItems: 0,
    lowStockItems: 0,
    // totalSuppliers: 0,
    totalPurchaseOrders: 0,
    totalIssues: 0,
    pendingIssues: 0,
    returnRequests: 0,
  });

  const chartData = [
    { name: 'Categories', value: summary.totalCategories },
    { name: 'Items', value: summary.totalItems },
    { name: 'Low Stock', value: summary.lowStockItems },
    { name: 'Purchases', value: summary.totalPurchaseOrders },
    { name: 'Issued', value: summary.totalIssues },
    { name: 'Pending Issues', value: summary.pendingIssues },
    { name: 'Return Requests', value: summary.returnRequests },
  ];
  
  // useEffect(() => {
  //   window.scrollTo(0, 0);
  //   fetchDashboardData();
  // }, []);
  
  const fetchDashboardData = async () => {
    try {
      const res = await axios.get('https://my-backend-sdbk.onrender.com/api/dashboard/summary');
      setSummary(res.data);
    } catch (err) {
      console.error('Failed to fetch dashboard summary:', err);
    }
  };

  return (
    <div className='bg-gradient-to-b from-black to-[rgb(7,7,33)] min-h-screen text-white'>
      
      {/* HERO SECTION */}
      <div
        className="p-6 w-full h-screen relative bg-cover bg-center"
        style={{ backgroundImage: `url(${bg_img})` }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className='space-y-30 flex flex-col justify-center items-center h-full relative'>
          <div className='text-center'>
            <h1 className="text-9xl font-bold text-white mb-4">IT</h1>
            <h1 className='text-8xl font-bold text-white mb-4'>CONSUMABLES</h1>
          </div>
          <div className='text-white text-xl text-center'>
            <h3>
              <TypeAnimation
                sequence={[
                  'Smart Management of IT Consumables – Know What, Where, and Who.',
                  2000,
                  'Efficient IT Inventory Tracking – Simplifying Your Supply Chain.',
                  2000]}
                wrapper="span"
                speed={50}
                repeat={Infinity}
              />
            </h3>
          </div>
          {userRole === 'admin' && (
            <>
              <div className="absolute bottom-24 left-1/2 transform -translate-x-1/2 ">
                <button
                  onClick={() => document.getElementById('dashboard').scrollIntoView({ behavior: 'smooth' })}
                  className="text-white animate-bounce"
                >
                  <ChevronDownIcon className="h-10 w-10" />
                </button>
              </div>
            </>
          )} 
        </div>
      </div>


      { userRole==='admin' &&(
        <>
      {/* FIX: Add spacer to allow scroll */}
      <div className="h-[20px]"></div>

      {/* DASHBOARD SECTION */}
      <div id='dashboard' className='relative px-6 py-8 bg-transparent shadow-lg max-w-7xl mx-auto min-h-screen flex flex-col justify-center items-center'>
        <h2 className="text-2xl text-white font-bold mb-6">Inventory Dashboard</h2>

      
        <div className="flex flex-col items-center justify-center lg:flex-row lg:items-center lg:justify-center gap-6 mb-8 w-[80vw]">
          <DashboardCard title="Total Categories" value={summary.totalCategories} />
          <DashboardCard title="Total Items" value={summary.totalItems} />
          <DashboardCard title="Low Stock Category Items" value={summary.lowStockItems} highlight/>
          {/* <DashboardCard title="Suppliers" value={summary.totalSuppliers} /> */}
          <DashboardCard title="Purchase Orders" value={summary.totalPurchaseOrders} />
          <DashboardCard title="Issued Records" value={summary.totalIssues} />
          <DashboardCard title="Pending Issues" value={summary.pendingIssues} />
          <DashboardCard title="Return Requests" value={summary.returnRequests}  />
        </div>

        <div className="bg-transparent border border-white text-black shadow p-4 rounded-xl w-[70vw]">
          <h3 className="text-xl text-white font-semibold mb-2">Inventory Trends</h3>
          {summary.totalItems === 0 ? (
            <div className="text-center text-gray-400">Loading summary...</div>
          ) : (
            <ResponsiveContainer width="100%" height={300}>
            
              <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip/>
                <Bar dataKey="value" fill="#818386" className='opacity-70' radius={[10, 10, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          )}
          
        </div>
      </div>
      </>
      )}
    </div>
  );
}

function DashboardCard({ title, value, highlight }) {
  return (
    <div className={`p-4 w-3xl rounded-xl shadow ${highlight && value>=10 ? 'bg-red-950 text-white' : 'bg-gray-400 text-black'} flex flex-col items-center justify-center transition-transform transform hover:scale-105`}>
      <h4 className="text-lg font-medium">{title}</h4>
      <p className="text-2xl font-bold mt-1">{value}</p>
    </div>
  );
}

export default Home;