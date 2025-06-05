import React, { useEffect, useState } from 'react';
import axios from 'axios';
import PurchaseDetailsModal from './PurchaseDetailsModal';
import { toast } from 'react-toastify';

const Items = () => {
  const [items, setItems] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedItem, setSelectedItem] = useState(null);
  const [purchaseDetails, setPurchaseDetails] = useState([]);
  const [showModal, setShowModal] = useState(false);
  const [filterCategoryId, setFilterCategoryId] = useState('');
  const [filterQuantity, setFilterQuantity] = useState('');
  const [filterStorageLoc, setFilterStorageLoc] = useState('');
  const [sortConfig, setSortConfig] = useState({ key: null, direction: 'asc' });
  const [showFilters, setShowFilters] = useState(false);
  const [showPurchaseFormForItem, setShowPurchaseFormForItem] = useState(null);
  const [purchaseFormData, setPurchaseFormData] = useState({
    quantity: '',
    price: '',
    purchaseDate: ''
  });
  const [newItem, setNewItem] = useState({
    name: '',
    category_id: '',
    model_no: '',
    brand: '',
    quantity: '',
    storage_loc_l1: '',
    storage_loc_l2: '',
    warrenty_expiration: '',
    purchase_quantity: '',
    price: '',
    purchase_date: '',
    item_id: ''
  });

  const [categories, setCategories] = useState([]);

  const filteredItems = items.filter(item => {
    const matchesSearch = item.name.toLowerCase().includes(searchTerm.toLowerCase());
  
    const matchesCategory = filterCategoryId ? item.category_id.toString() === filterCategoryId : true;
  
    const matchesQuantity = filterQuantity ? item.quantity === parseInt(filterQuantity, 10) : true;
  
    const matchesStorageLoc = filterStorageLoc
      ? (item.storage_loc_l1.toLowerCase().includes(filterStorageLoc.toLowerCase()) ||
         item.storage_loc_l2.toLowerCase().includes(filterStorageLoc.toLowerCase()))
      : true;
  
    return matchesSearch && matchesCategory && matchesQuantity && matchesStorageLoc;
  });

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
  // const filteredItems = items.filter(item =>
  //   item.name.toLowerCase().includes(searchTerm.toLowerCase())
  // );

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

  const handlePurchaseFormChange = (e) => {
    setPurchaseFormData({
      ...purchaseFormData,
      [e.target.name]: e.target.value
    });
  };

const handleNewPurchaseSubmit = async (itemId) => {
  try {
    const formattedDate = new Date(purchaseFormData.purchaseDate).toISOString().split('T')[0]; // "2025-06-03"

    await axios.post(`http://localhost:5007/api/purchase-details/${itemId}`, {
      quantity: Number(purchaseFormData.quantity),
      price: Number(purchaseFormData.price),
      purchaseDate: formattedDate
    });

    toast.success('Purchase added successfully!');
    fetchItems(); // refresh list
    setShowPurchaseFormForItem(null);
    setPurchaseFormData({ quantity: '', price: '', purchaseDate: '' });
  } catch (err) {
    console.error(err);
    toast.error('Failed to add purchase');
  }
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
      Name: newItem.name,
      CategoryId: newItem.category_id,
      ModelNo: newItem.model_no,
      Brand: newItem.brand,
      Quantity: newItem.quantity,
      StorageLocL1: newItem.storage_loc_l1,
      StorageLocL2: newItem.storage_loc_l2,
      WarrentyExpiration: newItem.warrenty_expiration,
    };

    if (newItem.item_id) {
      // Update existing item
      axios.put(`http://localhost:5007/api/items/update-item-with-purchase/${newItem.item_id}`, payload)
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
          Quantity: newItem.purchase_quantity,
          Price: newItem.price,
          PurchaseDate: newItem.purchase_date,
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
      warrenty_expiration: '',
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
    <div className="p-4 bg-gradient-to-b from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] text-white flex flex-col items-center justify-center min-h-screen">
     
     <h2 className="text-3xl mt-[60px] font-bold">Manage Items</h2>
     <div className='p-15 mx-auto m-auto mt-[50px] mb-[90px] w-[90vw] border border-gray-600 rounded-3xl shadow-2xl bg-transparent bg-opacity-30 backdrop-blur-lg'>
          <div className="absolute inset-0 bg-black opacity-20 -z-10 rounded-3xl"></div>
          

          {/* Search */}
          <div className='flex flex-col'>
            <input
              type="text"
              placeholder="Search items..."
              className="border px-2 py-1 mb-4 w-full md:w-1/2"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />

            <button
              className="mb-4 px-4 py-2 w-[10vw] bg-indigo-600 text-white rounded hover:bg-indigo-700"
              onClick={() => setShowFilters(!showFilters)}
            >
              {showFilters ? 'Hide Filters' : 'Show Filters'}
            </button>
          </div>

          {/* // In the JSX part, add filters UI above the table */}
          {showFilters && (
          <div className="mb-4 flex flex-wrap gap-4">
            <select
              className="border p-2"
              value={filterCategoryId}
              onChange={(e) => setFilterCategoryId(e.target.value)}
            >
              <option value="">All Categories</option>
              {categories.map((cat) => (
                <option
                key={cat.categoryId} 
                value={cat.categoryId}
                className='text-gray-700'
                >
                  {cat.categoryName}
                  </option>
              ))}
            </select>

            <input
              type="number"
              placeholder="Min Quantity"
              className="border p-2"
              value={filterQuantity}
              onChange={(e) => setFilterQuantity(e.target.value)}
            />

            <input
              type="text"
              placeholder="Storage Location"
              className="border p-2"
              value={filterStorageLoc}
              onChange={(e) => setFilterStorageLoc(e.target.value)}
            />
          </div>
          )}

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
                  {cat.categoryName} (ID: {cat.categoryId})
                </option>
              ))}
            </select>
            <input className="border p-2" name="model_no" value={newItem.model_no} onChange={handleInputChange} placeholder="Model No" required />
            <input className="border p-2" name="brand" value={newItem.brand} onChange={handleInputChange} placeholder="Brand" required />
            <input className="border p-2" type="number" name="quantity" value={newItem.quantity} onChange={handleInputChange} placeholder="Quantity" required />
            <input className="border p-2" name="storage_loc_l1" value={newItem.storage_loc_l1} onChange={handleInputChange} placeholder="Storage Loc L1" required />
            <input className="border p-2" name="storage_loc_l2" value={newItem.storage_loc_l2} onChange={handleInputChange} placeholder="Storage Loc L2" required />
            {/* <input className="border p-2" type="date" name="warrenty_expiration" value={newItem.warrenty_expiration} onChange={handleInputChange} placeholder="Warranty Expiration" /> */}
            <input className="border p-2" type="number" name="purchase_quantity" value={newItem.purchase_quantity} onChange={handleInputChange} placeholder="Purchase Quantity" required />
            <input className="border p-2" type="number" name="price" value={newItem.price} onChange={handleInputChange} placeholder="Price" required />
            <input className="border p-2" type="date" name="purchase_date" value={newItem.purchase_date} onChange={handleInputChange} placeholder="Purchase Date" required />
            <button type="submit" className="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600 col-span-1 md:col-span-3">
              {newItem.item_id ? 'Update Item' : 'Add Item'}
            </button>
          </form>

          {/* Items Table */}
          <p className='text-center text-gray-200 pb-5 underline'>Click on the column heading to sort acording to that column</p>
          <table className="w-full border border-collapse opacity-70">
            <thead className="bg-gray-700">
              <tr>
                <th className="border p-2 cursor-pointer" onClick={() => requestSort('name')}>Item Name</th>
                <th className="border p-2 cursor-pointer" onClick={() => requestSort('category_id')}>Category ID</th>
                <th className="border p-2 cursor-pointer" onClick={() => requestSort('model_no')}>Model No</th>
                <th className="border p-2 cursor-pointer" onClick={() => requestSort('brand')}>Brand</th>
                <th className="border p-2 cursor-pointer" onClick={() => requestSort('quantity')}>Quantity</th>
                <th className="border p-2 cursor-pointer" onClick={() => requestSort('storage_loc_l1')}>Storage L1</th>
                <th className="border p-2 cursor-pointer" onClick={() => requestSort('storage_loc_l2')}>Storage L2</th>
                {/* <th className='border p-2 cursor-pointer' onClick={() => requestSort('warrenty_expiration')}>Warranty Expiration</th> */}
                <th className="border p-2">Actions</th>
              </tr>
            </thead>
            <tbody>
          {sortedItems.map((item) => {
          const category = categories.find(cat => cat.categoryId === item.category_id);
          const threshold = category ? category.threshold : 0;
          const isBelowThreshold = item.quantity < threshold;

          return (
          <React.Fragment key={item.item_id}>
            <tr className={`text-center ${isBelowThreshold ? 'bg-red-200 text-red-800' : ''}`}>
              <td className="border p-2">{String(item.name)}</td>
              <td className="border p-2">{String(item.category_id)}</td>
              <td className="border p-2">{String(item.model_no)}</td>
              <td className="border p-2">{String(item.brand)}</td>
              <td className="border p-2">{String(item.quantity)}</td>
              <td className="border p-2">{String(item.storage_loc_l1)}</td>
              <td className="border p-2">{String(item.storage_loc_l2)}</td>
              <td className="border p-2">
                <button
                  className="bg-green-600 text-white px-3 py-1 rounded mr-2"
                  onClick={() => setShowPurchaseFormForItem(item.item_id)}
                >
                  New Purchase
                </button>
                <button
                  onClick={() => handleViewMore(item)}
                  className="bg-blue-500 text-white px-2 py-1 rounded mr-2"
                >
                  View
                </button>
                {/* Uncomment if edit functionality is needed */}
                {/* <button
                  onClick={() => handleEdit(item)}
                  className="bg-yellow-500 text-white px-2 py-1 rounded mr-2"
                >
                  Edit
                </button> */}
                <button
                  onClick={() => handleDelete(item.item_id)}
                  className="bg-red-500 text-white px-2 py-1 rounded"
                >
                  Delete
                </button>
              </td>
            </tr>

            {showPurchaseFormForItem === item.item_id && (
              <tr>
                <td colSpan="8" className="bg-transparent p-4">
                  <div className="flex flex-col md:flex-row gap-2">
                    <input
                      type="number"
                      name="quantity"
                      value={purchaseFormData.quantity}
                      onChange={handlePurchaseFormChange}
                      placeholder="Quantity"
                      className="border p-2"
                    />
                    <input
                      type="number"
                      name="price"
                      value={purchaseFormData.price}
                      onChange={handlePurchaseFormChange}
                      placeholder="Price"
                      className="border p-2"
                    />
                    <input
                      type="date"
                      name="purchaseDate"
                      value={purchaseFormData.purchaseDate}
                      onChange={handlePurchaseFormChange}
                      className="border p-2"
                    />
                    <button
                      onClick={() => handleNewPurchaseSubmit(item.item_id)}
                      className="bg-green-600 text-white px-4 py-2 rounded"
                    >
                      Submit Purchase
                    </button>
                    <button
                      onClick={() => setShowPurchaseFormForItem(null)}
                      className="bg-red-500 text-white px-3 py-2 rounded"
                    >
                      Cancel
                    </button>
                  </div>
                </td>
              </tr>
            )}
          </React.Fragment>
          );
          })}
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
    </div>
  );
};

export default Items;