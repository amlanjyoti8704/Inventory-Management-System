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
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Alert Logs</h2>

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
        <table className="min-w-full border border-gray-300">
          <thead className="bg-gray-100">
            <tr>
              <th className="border px-4 py-2">Log ID</th>
              <th className="border px-4 py-2">Item ID</th>
              <th className="border px-4 py-2">Item Name</th>
              <th className="border px-4 py-2">Current Quantity</th>
              <th className="border px-4 py-2">Message</th>
              <th className="border px-4 py-2">Time</th>
            </tr>
          </thead>
          <tbody>
            {alerts.map((alert) => (
              <tr key={alert.log_id} className="hover:bg-gray-50">
                <td className="border px-4 py-2">{alert.log_id}</td>
                <td className="border px-4 py-2">{alert.item_id}</td>
                <td className="border px-4 py-2">{alert.name}</td>
                <td className="border px-4 py-2">{alert.current_quantity}</td>
                <td className="border px-4 py-2 text-red-600">{alert.alert_message}</td>
                <td className="border px-4 py-2">{alert.alert_time}</td>
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
  );
}