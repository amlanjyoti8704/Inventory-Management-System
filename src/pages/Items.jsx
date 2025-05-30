import React, { useEffect, useState } from 'react';
import axios from 'axios';
import PurchaseDetailsModal from './PurchaseDetailsModal';

const Items = () => {
  const [items, setItems] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedItem, setSelectedItem] = useState(null);
  const [purchaseDetails, setPurchaseDetails] = useState([]);
  const [showModal, setShowModal] = useState(false);
  const [sortConfig, setSortConfig] = useState({ key: null, direction: 'asc' });

  const [newItem, setNewItem] = useState({
    name: '',
    category_id: '',
    model_no: '',
    brand: '',
    quantity: '',
    storage_loc_l1: '',
    storage_loc_l2: '',
    purchase_quantity: '',
    price: '',
    purchase_date: '',
    item_id: ''
  });

  const [categories, setCategories] = useState([]);

  // Fetch categories on mount
  useEffect(() => {
    fetchCategories();
  }, []);

  // Fetch items on mount and when needed
  useEffect(() => {
    fetchItems();
  }, []);

  const fetchCategories = () => {
    axios.get('http://localhost:5007/api/categories')
      .then(res => setCategories(res.data))
      .catch(err => console.error(err));
  };

  const fetchItems = () => {
    axios.get('http://localhost:5007/api/items')
      .then(res => setItems(res.data))
      .catch(err => console.error(err));
  };

  // Filtered items based on search term
  const filteredItems = items.filter(item =>
    item.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Sorting function applied to filtered items
  const sortedItems = React.useMemo(() => {
    let sortableItems = [...filteredItems];
    if (sortConfig.key !== null) {
      sortableItems.sort((a, b) => {
        let aVal = a[sortConfig.key];
        let bVal = b[sortConfig.key];

        // For string sort (case insensitive)
        if (typeof aVal === 'string') {
          aVal = aVal.toLowerCase();
          bVal = bVal.toLowerCase();
        }

        if (aVal < bVal) {
          return sortConfig.direction === 'asc' ? -1 : 1;
        }
        if (aVal > bVal) {
          return sortConfig.direction === 'asc' ? 1 : -1;
        }
        return 0;
      });
    }
    return sortableItems;
  }, [filteredItems, sortConfig]);

  // Request sorting by column key
  const requestSort = (key) => {
    let direction = 'asc';
    if (sortConfig.key === key && sortConfig.direction === 'asc') {
      direction = 'desc';
    }
    setSortConfig({ key, direction });
  };

  const handleViewMore = (item) => {
    setSelectedItem(item);
    axios.get(`http://localhost:5007/api/purchase-details/${item.item_id}`)
      .then(res => {
        setPurchaseDetails(res.data);
        setShowModal(true);
      })
      .catch(err => console.error(err));
  };

  const handleInputChange = (e) => {
    setNewItem({ ...newItem, [e.target.name]: e.target.value });
  };

  const handleAddItem = (e) => {
    e.preventDefault();

    const payload = {
      name: newItem.name,
      categoryId: newItem.category_id,
      modelNo: newItem.model_no,
      brand: newItem.brand,
      quantity: newItem.quantity,
      storageLocL1: newItem.storage_loc_l1,
      storageLocL2: newItem.storage_loc_l2,
    };

    if (newItem.item_id) {
      // Update existing item
      axios.put(`http://localhost:5007/api/items/${newItem.item_id}`, payload)
        .then(() => {
          fetchItems();
          resetForm();
        })
        .catch(err => console.error(err));
    } else {
      // Create item + purchase details
      const fullPayload = {
        item: payload,
        purchase: {
          quantity: newItem.purchase_quantity,
          price: newItem.price,
          purchaseDate: newItem.purchase_date,
        }
      };

      axios.post('http://localhost:5007/api/items/items-with-purchase', fullPayload)
        .then(() => {
          fetchItems();
          resetForm();
        })
        .catch(err => console.error(err));
    }
  };

  const resetForm = () => {
    setNewItem({
      name: '',
      category_id: '',
      model_no: '',
      brand: '',
      quantity: '',
      storage_loc_l1: '',
      storage_loc_l2: '',
      purchase_quantity: '',
      price: '',
      purchase_date: '',
      item_id: ''
    });
  };

  const handleEdit = (item) => {
    setNewItem({
      ...item,
      purchase_quantity: '',
      price: '',
      purchase_date: ''
    });
  };

  const handleDelete = (itemId) => {
    if (window.confirm("Are you sure you want to delete this item?")) {
      axios.delete(`http://localhost:5007/api/items/${itemId}`)
        .then(() => {
          fetchItems();
        })
        .catch(err => console.error(err));
    }
  };

  return (
    <div className="p-4">
      <h2 className="text-2xl font-bold mb-4">Manage Items</h2>

      {/* Search */}
      <input
        type="text"
        placeholder="Search items..."
        className="border px-2 py-1 mb-4 w-full md:w-1/2"
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
      />

      {/* Add New Item Form */}
      <form onSubmit={handleAddItem} className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
        <input className="border p-2" name="name" value={newItem.name} onChange={handleInputChange} placeholder="Item Name" required />
        <select
          className="border p-2"
          name="category_id"
          value={newItem.category_id}
          onChange={handleInputChange}
          required
        >
          <option value="">Select Category</option>
          {categories.map((cat) => (
            <option key={cat.categoryId} value={cat.categoryId}>
              {cat.name} (ID: {cat.categoryId})
            </option>
          ))}
        </select>
        <input className="border p-2" name="model_no" value={newItem.model_no} onChange={handleInputChange} placeholder="Model No" required />
        <input className="border p-2" name="brand" value={newItem.brand} onChange={handleInputChange} placeholder="Brand" required />
        <input className="border p-2" type="number" name="quantity" value={newItem.quantity} onChange={handleInputChange} placeholder="Quantity" required />
        <input className="border p-2" name="storage_loc_l1" value={newItem.storage_loc_l1} onChange={handleInputChange} placeholder="Storage Loc L1" required />
        <input className="border p-2" name="storage_loc_l2" value={newItem.storage_loc_l2} onChange={handleInputChange} placeholder="Storage Loc L2" required />
        <input className="border p-2" type="number" name="purchase_quantity" value={newItem.purchase_quantity} onChange={handleInputChange} placeholder="Purchase Quantity" required />
        <input className="border p-2" type="number" name="price" value={newItem.price} onChange={handleInputChange} placeholder="Price" required />
        <input className="border p-2" type="date" name="purchase_date" value={newItem.purchase_date} onChange={handleInputChange} placeholder="Purchase Date" required />
        <button type="submit" className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600 col-span-1 md:col-span-3">
          {newItem.item_id ? 'Update Item' : 'Add Item'}
        </button>
      </form>

      {/* Items Table */}
      <p className='text-center text-gray-500'>Click on the column heading to sort acording to that column</p>
      <table className="w-full border border-collapse">
        <thead className="bg-gray-200">
          <tr>
            <th className="border p-2 cursor-pointer" onClick={() => requestSort('name')}>Item Name</th>
            <th className="border p-2 cursor-pointer" onClick={() => requestSort('category_id')}>Category ID</th>
            <th className="border p-2 cursor-pointer" onClick={() => requestSort('model_no')}>Model No</th>
            <th className="border p-2 cursor-pointer" onClick={() => requestSort('brand')}>Brand</th>
            <th className="border p-2 cursor-pointer" onClick={() => requestSort('quantity')}>Quantity</th>
            <th className="border p-2 cursor-pointer" onClick={() => requestSort('storage_loc_l1')}>Storage L1</th>
            <th className="border p-2 cursor-pointer" onClick={() => requestSort('storage_loc_l2')}>Storage L2</th>
            <th className="border p-2">Actions</th>
          </tr>
        </thead>
        <tbody>
          {sortedItems.map(item => (
            <tr key={item.item_id} className="text-center">
              <td className="border p-2">{item.name}</td>
              <td className="border p-2">{item.category_id}</td>
              <td className="border p-2">{item.model_no}</td>
              <td className="border p-2">{item.brand}</td>
              <td className="border p-2">{item.quantity}</td>
              <td className="border p-2">{item.storage_loc_l1}</td>
              <td className="border p-2">{item.storage_loc_l2}</td>
              <td className="border p-2">
                <button
                  onClick={() => handleViewMore(item)}
                  className="bg-blue-500 text-white px-2 py-1 rounded mr-2"
                >
                  View
                </button>
                <button
                  onClick={() => handleEdit(item)}
                  className="bg-yellow-500 text-white px-2 py-1 rounded mr-2"
                >
                  Edit
                </button>
                <button
                  onClick={() => handleDelete(item.item_id)}
                  className="bg-red-500 text-white px-2 py-1 rounded"
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {showModal && selectedItem && (
        <PurchaseDetailsModal
          item={selectedItem}
          purchaseDetails={purchaseDetails}
          onClose={() => setShowModal(false)}
        />
      )}
    </div>
  );
};

export default Items;