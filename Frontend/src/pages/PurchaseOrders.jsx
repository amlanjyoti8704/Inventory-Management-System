// import React, { useState } from 'react';

// const initialPOs = [
//   {
//     id: 1,
//     supplier: 'ABC Supplies',
//     date: '2025-05-20',
//     items: [
//       { itemName: 'USB Cable', quantity: 10 },
//       { itemName: 'HDMI Adapter', quantity: 5 },
//     ],
//   },
// ];

// const PurchaseOrders = () => {
//   const [purchaseOrders, setPurchaseOrders] = useState(initialPOs);
//   const [newPO, setNewPO] = useState({ supplier: '', items: [] });
//   const [itemInput, setItemInput] = useState({ itemName: '', quantity: '' });
//   const [showForm, setShowForm] = useState(false);

//   const handleItemChange = (e) => {
//     const { name, value } = e.target;
//     setItemInput((prev) => ({ ...prev, [name]: value }));
//   };

//   const addItemToPO = () => {
//     if (itemInput.itemName && itemInput.quantity) {
//       setNewPO((prev) => ({ ...prev, items: [...prev.items, itemInput] }));
//       setItemInput({ itemName: '', quantity: '' });
//     }
//   };

//   const savePO = () => {
//     if (newPO.supplier && newPO.items.length > 0) {
//       setPurchaseOrders((prev) => [
//         ...prev,
//         { ...newPO, id: Date.now(), date: new Date().toISOString().split('T')[0] },
//       ]);
//       setNewPO({ supplier: '', items: [] });
//       setShowForm(false);
//     }
//   };

//   return (
//     <div className="p-4">
//       <h2 className="text-xl font-bold mb-4">Purchase Orders</h2>

//       <button
//         onClick={() => setShowForm(true)}
//         className="bg-blue-500 text-white px-4 py-2 rounded mb-4"
//       >
//         Create Purchase Order
//       </button>

//       {showForm && (
//         <div className="bg-gray-100 p-4 rounded mb-4">
//           <label className="block mb-2">
//             Supplier:
//             <input
//               type="text"
//               value={newPO.supplier}
//               onChange={(e) => setNewPO({ ...newPO, supplier: e.target.value })}
//               className="border p-2 rounded w-full"
//             />
//           </label>

//           <div className="flex gap-2 mb-2">
//             <input
//               type="text"
//               name="itemName"
//               placeholder="Item name"
//               value={itemInput.itemName}
//               onChange={handleItemChange}
//               className="border p-2 rounded w-full"
//             />
//             <input
//               type="number"
//               name="quantity"
//               placeholder="Quantity"
//               value={itemInput.quantity}
//               onChange={handleItemChange}
//               className="border p-2 rounded w-24"
//             />
//             <button onClick={addItemToPO} className="bg-green-500 text-white px-4 py-2 rounded">
//               Add Item
//             </button>
//           </div>

//           <ul className="mb-4">
//             {newPO.items.map((item, index) => (
//               <li key={index} className="text-sm">
//                 {item.itemName} - {item.quantity}
//               </li>
//             ))}
//           </ul>

//           <button onClick={savePO} className="bg-blue-600 text-white px-4 py-2 rounded">
//             Save PO
//           </button>
//         </div>
//       )}

//       <table className="w-full table-auto border">
//         <thead className="bg-gray-200">
//           <tr>
//             <th className="border px-4 py-2">ID</th>
//             <th className="border px-4 py-2">Supplier</th>
//             <th className="border px-4 py-2">Date</th>
//             <th className="border px-4 py-2">Items</th>
//           </tr>
//         </thead>
//         <tbody>
//           {purchaseOrders.map((po) => (
//             <tr key={po.id}>
//               <td className="border px-4 py-2">{po.id}</td>
//               <td className="border px-4 py-2">{po.supplier}</td>
//               <td className="border px-4 py-2">{po.date}</td>
//               <td className="border px-4 py-2">
//                 <ul className="list-disc pl-4">
//                   {po.items.map((item, idx) => (
//                     <li key={idx}>
//                       {item.itemName} - {item.quantity}
//                     </li>
//                   ))}
//                 </ul>
//               </td>
//             </tr>
//           ))}
//         </tbody>
//       </table>
//     </div>
//   );
// };

// export default PurchaseOrders;
