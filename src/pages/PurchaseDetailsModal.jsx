import React from 'react';

const PurchaseDetailsModal = ({ item, purchaseDetails, onClose }) => {
  return (
    <div className="fixed inset-0 bg-transparent bg-opacity-50 flex justify-center items-center z-50">
      <div className="bg-gray-100 p-6 rounded-lg w-11/12 md:w-2/3 lg:w-1/2 shadow-lg relative">
        <button
          className="absolute top-2 right-3 text-red-500 text-xl"
          onClick={onClose}
        >
          &times;
        </button>
        <h3 className="text-xl font-bold mb-4">Purchase Details for {item.name}</h3>
        <table className="w-full border">
          <thead className="bg-gray-100">
            <tr>
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
                  <td className="border p-2">{pd.order_id}</td>
                  <td className="border p-2">{pd.quantity}</td>
                  <td className="border p-2">{pd.price}</td>
                  <td className="border p-2">{pd.purchase_date}</td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan="4" className="text-center p-2 text-gray-500">
                  No purchase records found
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default PurchaseDetailsModal;