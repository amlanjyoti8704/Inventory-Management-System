import React, { useState, useEffect } from 'react';
import axios from 'axios';

function CategoryPage() {
  const [categories, setCategories] = useState([]);
  const [categoryName, setCategoryName] = useState('');
  const [threshold, setThreshold] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [editingId, setEditingId] = useState(null);
  const [sortBy, setSortBy] = useState('id');

  useEffect(() => {
    window.scrollTo(0, 0);
    fetchCategories();
  }, []);

  const fetchCategories = async () => {
    try {
      const res = await axios.get('http://localhost:5007/api/categories');
      setCategories(res.data);
    } catch (err) {
      setError('Failed to fetch categories');
    }
  };

  const handleAddCategory = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (!categoryName || !threshold) {
      setError('Please fill in all fields');
      return;
    }

    const exists = categories.find(
      cat => cat.categoryName.toLowerCase() === categoryName.toLowerCase() && cat.categoryId !== editingId
    );
    if (exists) {
      setError('Category already exists!');
      return;
    }

    try {
      if (editingId !== null) {
        await axios.put(`http://localhost:5007/api/categories/${editingId}`, {
          categoryName: categoryName,
          threshold: parseInt(threshold)
        });
        setSuccess('Category updated successfully!');
      } else {
        await axios.post('http://localhost:5007/api/categories', {
          categoryName: categoryName,
          threshold: parseInt(threshold)
        });
        setSuccess('Category added successfully!');
      }
      setCategoryName('');
      setThreshold('');
      setEditingId(null);
      fetchCategories();
    } catch (err) {
      setError('Operation failed');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this category?')) return;
    try {
      await axios.delete(`http://localhost:5007/api/categories/${id}`);
      setSuccess('Category deleted successfully!');
      fetchCategories();
    } catch (err) {
      setError('Delete failed');
    }
  };

  const handleEdit = (cat) => {
    setCategoryName(cat.categoryName);
    setThreshold(cat.threshold);
    setEditingId(cat.categoryId);
    setError('');
    setSuccess('');
  };

  return (
    <div className='bg-gradient-to-b from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] w-full min-h-screen flex flex-col justify-center items-center'>
      
      <h1 className="text-3xl text-white mt-[70px] font-bold">Manage Categories</h1>
      <div className="p-15 mx-auto m-auto mt-[50px] mb-[90px] w-[60vw] border border-gray-600 rounded-3xl shadow-2xl bg-transparent bg-opacity-30 backdrop-blur-lg">
        <div className="absolute inset-0 bg-black opacity-20 -z-10 rounded-3xl"></div>
        

        <form onSubmit={handleAddCategory} className="space-y-4 bg-transparent p-4 rounded-md shadow-md">
          <div>
            <label className="block mb-1 text-white font-medium">Category Name</label>
            <input
              type="text"
              className="w-full border px-3 py-2 rounded bg-transparent text-slate-300 border-gray-700"
              value={categoryName}
              onChange={(e) => setCategoryName(e.target.value)}
              required
            />
          </div>

          <div>
            <label className="block mb-1 text-white font-medium">Threshold</label>
            <input
              type="number"
              className="w-full border px-3 py-2 rounded bg-transparent text-slate-300 border-gray-700"
              value={threshold}
              onChange={(e) => setThreshold(e.target.value)}
              required
            />
          </div>

          <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
            {editingId !== null ? 'Update Category' : 'Add Category'}
          </button>

          {error && <p className="text-red-500 mt-2">{error}</p>}
          {success && <p className="text-green-600 mt-2">{success}</p>}
        </form>

        <div className="flex items-center justify-between mt-8 mb-2">
          <h2 className="text-xl text-white font-semibold">Existing Categories</h2>
          <div className='text-white'>
            <label className="mr-2 font-medium">Sort by:</label>
            <select
              className="text-slate-300 px-2 py-1 rounded"
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
            >
              <option value="id">ID (Numeric)</option>
              <option value="name">Name (A-Z)</option>
              <option value="threshold">Threshold</option>
            </select>
          </div>
        </div>

        <table className="min-w-full text-slate-200 bg-transparent opacity-90 border">
          <thead className="bg-gray-700 text-slate-200 opacity-100 z-10">
            <tr>
              <th className="py-2 px-4 border border-gray-300">ID</th>
              <th className="py-2 px-4 border border-gray-300">Name</th>
              <th className="py-2 px-4 border border-gray-300">Threshold</th>
              <th className="py-2 px-4 border border-gray-300">Actions</th>
            </tr>
          </thead>
          <tbody>
            {categories
              .slice()
              .sort((a, b) => {
                if (sortBy === 'id') return a.categoryId - b.categoryId;
                if (sortBy === 'name') return a.categoryName.localeCompare(b.categoryName);
                if (sortBy === 'threshold') return a.threshold - b.threshold;
                return 0;
              })
              .map(cat => (
                <tr key={cat.category_id}>
                  <td className="py-2 px-4 border border-gray-300">{cat.categoryId}</td>
                  <td className="py-2 px-4 border border-gray-300">{cat.categoryName}</td>
                  <td className="py-2 px-4 border border-gray-300">{cat.threshold}</td>
                  <td className="py-2 px-4 border border-gray-300">
                    <button
                      onClick={() => handleEdit(cat)}
                      className="text-blue-600 hover:underline mr-3"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => handleDelete(cat.categoryId)}
                      className="text-red-600 hover:underline"
                    >
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            {categories.length === 0 && (
              <tr>
                <td colSpan="4" className="text-center p-4 text-gray-500 border border-white">
                  No categories yet.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default CategoryPage;