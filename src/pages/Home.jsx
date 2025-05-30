import React, { useEffect } from 'react';
import { TypeAnimation } from 'react-type-animation';
import background_image from '../assets/background_img.png';
import bg_img from '../assets/6BEF965A-A37E-451D-9714-9F140F7C94B9_1_201_a.jpeg';
import { ChevronDownIcon } from '@heroicons/react/24/outline';

function Home() {
  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  const summary = {
    totalCategories: 5,
    totalItems: 40,
    lowStockItems: 3,
    totalSuppliers: 6,
    totalPurchaseOrders: 12,
    totalIssues: 28,
  };

  return (
    <div className='bg-gradient-to-b from-black to-[rgb(7,7,33)] min-h-screen text-white'>
      
      {/* HERO SECTION */}
      <div
        className="p-6 w-full h-screen relative bg-cover bg-center"
        style={{ backgroundImage: `url(${bg_img})` }}
      >
        <div className="absolute inset-0 bg-black opacity-50"></div>
        <div className='space-y-30 flex flex-col justify-center items-center h-full relative z-10'>
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
          <div className="absolute bottom-24 left-1/2 transform -translate-x-1/2 z-20">
            <button
              onClick={() => document.getElementById('dashboard').scrollIntoView({ behavior: 'smooth' })}
              className="text-white animate-bounce"
            >
              <ChevronDownIcon className="h-10 w-10" />
            </button>
          </div>
        </div>
      </div>

      {/* FIX: Add spacer to allow scroll */}
      <div className="h-[20px]"></div>

      {/* DASHBOARD SECTION */}
      <div id='dashboard' className='relative px-6 py-8 bg-transparent shadow-lg max-w-7xl mx-auto min-h-screen flex flex-col justify-center items-center'>
        <h2 className="text-2xl text-white font-bold mb-6">Inventory Dashboard</h2>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 mb-8 w-[70vw] z-10">
          <DashboardCard title="Total Categories" value={summary.totalCategories} />
          <DashboardCard title="Total Items" value={summary.totalItems} />
          <DashboardCard title="Low Stock Items" value={summary.lowStockItems} highlight />
          <DashboardCard title="Suppliers" value={summary.totalSuppliers} />
          <DashboardCard title="Purchase Orders" value={summary.totalPurchaseOrders} />
          <DashboardCard title="Issued Records" value={summary.totalIssues} />
        </div>

        <div className="bg-white text-black shadow p-4 rounded-xl w-[70vw] z-10">
          <h3 className="text-xl font-semibold mb-2">Inventory Trends</h3>
          <div className="text-gray-500">[Insert graph or recent activity feed here]</div>
        </div>
      </div>
    </div>
  );
}

function DashboardCard({ title, value, highlight }) {
  return (
    <div className={`p-4 rounded-xl shadow ${highlight ? 'bg-red-950 text-white' : 'bg-gray-400 text-black'} flex flex-col items-center justify-center z-10`}>
      <h4 className="text-lg font-medium">{title}</h4>
      <p className="text-2xl font-bold mt-1">{value}</p>
    </div>
  );
}

export default Home;