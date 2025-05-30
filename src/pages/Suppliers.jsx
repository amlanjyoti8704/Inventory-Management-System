import React, { useState } from 'react';

function Suppliers() {
  const [suppliers, setSuppliers] = useState([
    { id: 1, name: 'Tech Supplies Co.', contact: 'tech@supplies.com' },
    { id: 2, name: 'Office Depot', contact: 'office@depot.com' },
  ]);

  const [form, setForm] = useState({ name: '', contact: '' });
  const [isEditing, setIsEditing] = useState(false);
  const [editId, setEditId] = useState(null);

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleAdd = () => {
    if (!form.name || !form.contact) return;

    setSuppliers([
      ...suppliers,
      {
        id: Date.now(),
        name: form.name,
        contact: form.contact,
      },
    ]);
    setForm({ name: '', contact: '' });
  };

  const handleDelete = (id) => {
    setSuppliers(suppliers.filter((s) => s.id !== id));
  };

  const handleEdit = (supplier) => {
    setIsEditing(true);
    setEditId(supplier.id);
    setForm({ name: supplier.name, contact: supplier.contact });
  };

  const handleUpdate = () => {
    if (!form.name || !form.contact) return;

    setSuppliers(
      suppliers.map((s) =>
        s.id === editId ? { ...s, name: form.name, contact: form.contact } : s
      )
    );
    setIsEditing(false);
    setEditId(null);
    setForm({ name: '', contact: '' });
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Suppliers</h2>

      <div className="mb-6 space-y-2">
        <input
          type="text"
          name="name"
          placeholder="Supplier Name"
          value={form.name}
          onChange={handleChange}
          className="border px-3 py-2 rounded w-full"
        />
        <input
          type="text"
          name="contact"
          placeholder="Contact Email/Number"
          value={form.contact}
          onChange={handleChange}
          className="border px-3 py-2 rounded w-full"
        />
        {isEditing ? (
          <button
            onClick={handleUpdate}
            className="bg-yellow-500 text-white px-4 py-2 rounded"
          >
            Update Supplier
          </button>
        ) : (
          <button
            onClick={handleAdd}
            className="bg-blue-600 text-white px-4 py-2 rounded"
          >
            Add Supplier
          </button>
        )}
      </div>

      <table className="w-full text-left border">
        <thead>
          <tr className="bg-gray-200">
            <th className="p-2">#</th>
            <th className="p-2">Name</th>
            <th className="p-2">Contact</th>
            <th className="p-2">Actions</th>
          </tr>
        </thead>
        <tbody>
          {suppliers.map((supplier, index) => (
            <tr key={supplier.id} className="border-t">
              <td className="p-2">{index + 1}</td>
              <td className="p-2">{supplier.name}</td>
              <td className="p-2">{supplier.contact}</td>
              <td className="p-2 space-x-2">
                <button
                  onClick={() => handleEdit(supplier)}
                  className="bg-yellow-500 text-white px-2 py-1 rounded"
                >
                  Edit
                </button>
                <button
                  onClick={() => handleDelete(supplier.id)}
                  className="bg-red-500 text-white px-2 py-1 rounded"
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
          {suppliers.length === 0 && (
            <tr>
              <td colSpan="4" className="text-center p-4">
                No suppliers found.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default Suppliers;