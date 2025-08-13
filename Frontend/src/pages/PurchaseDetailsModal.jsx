import React, { useState } from 'react';
import axios from 'axios';

const PurchaseDetailsModal = ({ item, purchaseDetails, onClose, onDeleted }) => {
  const [selectedOrders, setSelectedOrders] = useState([]);

  const toggleSelection = (orderId) => {
    setSelectedOrders((prev) =>
      prev.includes(orderId)
        ? prev.filter((id) => id !== orderId)
        : [...prev, orderId]
    );
  };

  const handleDelete = async () => {
    if (selectedOrders.length === 0) {
      alert("Please select at least one purchase to delete.");
      return;
    }

    if (!window.confirm(`Are you sure you want to delete ${selectedOrders.length} purchase(s)?`)) {
      return;
    }

    try {
      // await axios.post('https://my-backend-sdbk.onrender.com/api/purchase-details/delete', {
      //   itemId: item.item_id,
      //   orderIds: selectedOrders,
      // });
      await axios.post('http://localhost:5007/api/purchase-details/delete', {
        itemId: item.item_id,
        orderIds: selectedOrders,
      });

      alert("Selected purchases deleted.");

      // Optional: Tell parent to refresh
      if (onDeleted) onDeleted();

      setSelectedOrders([]);
    } catch (err) {
      console.error("Delete failed:", err);
      alert("Failed to delete purchases.");
    }
  };

  return (
    <div className="fixed inset-0 bg-transparent bg-opacity-50 flex justify-center items-center z-50">
      <div className="bg-gradient-to-br from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] p-6 rounded-lg w-11/12 md:w-2/3 lg:w-1/2 shadow-lg relative border border-white">
        <div className="absolute inset-0 bg-white opacity-10 z-10 rounded-lg"></div>
        <button
          className="absolute top-2 right-3 text-red-500 text-xl z-20 hover:text-red-700 transition-colors duration-200"
          onClick={onClose}
        >
          &times;
        </button>
        <h3 className="text-xl font-bold mb-4 relative z-20">
          Purchase Details for {item.name}
        </h3>

        <div className="z-20 relative">
          <button
            className="mb-3 bg-red-600 hover:bg-red-700 text-white px-4 py-1 rounded disabled:opacity-50"
            onClick={handleDelete}
            disabled={selectedOrders.length === 0}
          >
            Delete Selected
          </button>

          <table className="w-full border">
            <thead className="bg-gray-700">
              <tr>
                <th className="border p-2">Select</th>
                <th className="border p-2">Order ID</th>
                <th className="border p-2">Quantity</th>
                <th className="border p-2">Price</th>
                <th className="border p-2">Date</th>
              </tr>
            </thead>
            <tbody>
              {purchaseDetails.length > 0 ? (
                purchaseDetails.map((pd, index) => (
                  <tr key={index}>
                    <td className="border p-2 text-center">
                      <input
                        type="checkbox"
                        checked={selectedOrders.includes(pd.orderId)}
                        onChange={() => toggleSelection(pd.orderId)}
                      />
                    </td>
                    <td className="border p-2">{pd.orderId}</td>
                    <td className="border p-2">{pd.quantity}</td>
                    <td className="border p-2">{pd.price}</td>
                    <td className="border p-2">{pd.purchaseDate}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan="5" className="text-center p-2 text-gray-500">
                    No purchase records found
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default PurchaseDetailsModal;
