import React, { useEffect, useState } from 'react';
import axios from 'axios';

export default function Alerts() {
  const [alerts, setAlerts] = useState([]);
  const [itemIdFilter, setItemIdFilter] = useState('');
  const [startDateFilter, setStartDateFilter] = useState('');
  const [endDateFilter, setEndDateFilter] = useState('');

  const fetchAlerts = async () => {
    try {
      const params = {};
      if (itemIdFilter) params.item_id = itemIdFilter;
      if (startDateFilter) params.startDate = startDateFilter;
      if (endDateFilter) params.endDate = endDateFilter;

      const response = await axios.get('http://localhost:5007/api/alert', { params });
      setAlerts(response.data);
    } catch (err) {
      console.error('Error fetching alerts:', err);
    }
  };

  useEffect(() => {
    fetchAlerts();
  }, []);

  const handleFilterSubmit = (e) => {
    e.preventDefault();
    fetchAlerts();
  };

  return (
    <div className="p-6 bg-gradient-to-b from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] text-white w-full min-h-screen flex flex-col justify-center items-center ">
      <h2 className="text-3xl font-bold ">Alert Logs</h2>
      <div className='p-15 mx-auto m-auto mt-[50px] mb-[90px] w-[90vw] border border-gray-600 rounded-3xl shadow-2xl bg-transparent bg-opacity-30 backdrop-blur-lg'>
      <div className="absolute inset-0 bg-black opacity-20 -z-10 rounded-3xl"></div>
          

          <form onSubmit={handleFilterSubmit} className="mb-4 flex flex-wrap gap-4 items-end">
            <div>
              <label className="block text-sm font-medium">Item ID</label>
              <input
                type="number"
                value={itemIdFilter}
                onChange={(e) => setItemIdFilter(e.target.value)}
                className="border px-2 py-1 rounded w-32"
              />
            </div>
            <div>
              <label className="block text-sm font-medium">Start Date</label>
              <input
                type="date"
                value={startDateFilter}
                onChange={(e) => setStartDateFilter(e.target.value)}
                className="border px-2 py-1 rounded"
              />
            </div>
            <div>
              <label className="block text-sm font-medium">End Date</label>
              <input
                type="date"
                value={endDateFilter}
                onChange={(e) => setEndDateFilter(e.target.value)}
                className="border px-2 py-1 rounded"
              />
            </div>
            <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
              Filter
            </button>
          </form>

          <div className="overflow-x-auto">
            <table className="min-w-full opacity-90">
              <thead className="bg-gray-700">
                <tr>
                  <th className="border-b px-4 py-2 rounded-tl-lg">Log ID</th>
                  <th className="border-b px-4 py-2">Item ID</th>
                  <th className="border-b px-4 py-2">Item Name</th>
                  <th className="border-b px-4 py-2">Current Quantity</th>
                  <th className="border-b px-4 py-2">Message</th>
                  <th className="border-b px-4 py-2 rounded-tr-lg">Time</th>
                </tr>
              </thead>
              <tbody>
                {alerts.map((alert) => (
                  <tr key={alert.log_id} className="hover:bg-gray-800 transition-colors text-center">
                    <td className="border-b px-4 py-2">{alert.log_id}</td>
                    <td className="border-b px-4 py-2">{alert.item_id}</td>
                    <td className="border-b px-4 py-2">{alert.name}</td>
                    <td className="border-b px-4 py-2">{alert.current_quantity}</td>
                    <td className="border border-white px-4 py-2 text-red-500">{alert.alert_message}</td>
                    <td className="border-b px-4 py-2">{alert.alert_time}</td>
                  </tr>
                ))}
                {alerts.length === 0 && (
                  <tr>
                    <td colSpan="6" className="text-center py-4 text-gray-500">No alerts found.</td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
      </div>
    </div>
  );
}