import React, { useEffect, useState } from 'react';
import axios from 'axios';

function IssueRecord() {
  const [items, setItems] = useState([]);
  const [issueList, setIssueList] = useState([]);
  const [selectedItemId, setSelectedItemId] = useState('');
  const [issuedTo, setIssuedTo] = useState('');
  const [department, setDepartment] = useState('');
  const [quantity, setQuantity] = useState('');

  useEffect(() => {
    fetchItems();
    fetchIssueList();
  }, []);

  const fetchItems = async () => {
    try {
      const res = await axios.get('http://localhost:5007/api/issue/items');
      console.log("Fetched items response:", res.data);
      setItems(Array.isArray(res.data) ? res.data : []);
    } catch (err) {
      console.error("Failed to fetch items:", err);
      setItems([]);
    }
  };

  const fetchIssueList = async () => {
    try {
      const res = await axios.get('http://localhost:5007/api/issue');
      console.log("Fetched issueList response:", res.data);
      setIssueList(Array.isArray(res.data) ? res.data : []);
    } catch (err) {
      console.error("Failed to fetch issued list:", err);
      setIssueList([]);
    }
  };

  const handleIssue = async () => {
    if (!selectedItemId || isNaN(selectedItemId) || !issuedTo || !department || quantity <= 0) {
      alert('All fields are required and valid');
      return;
    }

    try {
      await axios.post('http://localhost:5007/api/issue', {
        item_id: selectedItemId,
        issued_to: issuedTo,
        department,
        quantity: parseInt(quantity),
      });
      fetchItems();
      fetchIssueList();
      setSelectedItemId('');
      setIssuedTo('');
      setDepartment('');
      setQuantity('');
    } catch (err) {
      const errorMessage = err.response?.data?.error || err.message;
      alert('Error issuing item: ' + errorMessage);
    }
  };

  const handleRevoke = async (issue_id) => {
    try {
      await axios.delete(`http://localhost:5007/api/issue/${issue_id}`);
      fetchItems();
      fetchIssueList();
    } catch (err) {
      alert('Error revoking issue');
    }
  };

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">Issue Record</h2>

      <div className="grid md:grid-cols-5 gap-4 mb-6">
        <select
          value={selectedItemId}
          onChange={(e) => setSelectedItemId(parseInt(e.target.value))}
          className="border px-3 py-2 rounded"
        >
          <option value="">Select Item</option>
          {Array.isArray(items) && items.map((item) => (
            <option key={item.item_id} value={item.item_id}>
              {item.item_name} (Stock: {item.stock})
            </option>
          ))}
        </select>

        <input
          type="text"
          value={issuedTo}
          onChange={(e) => setIssuedTo(e.target.value)}
          placeholder="Issued To"
          className="border px-3 py-2 rounded"
        />

        <input
          type="text"
          value={department}
          onChange={(e) => setDepartment(e.target.value)}
          placeholder="Department"
          className="border px-3 py-2 rounded"
        />

        <input
          type="number"
          min="1"
          value={quantity}
          onChange={(e) => setQuantity(e.target.value)}
          placeholder="Quantity"
          className="border px-3 py-2 rounded"
        />

        <button
          onClick={handleIssue}
          className="bg-blue-600 text-white px-4 py-2 rounded"
        >
          Issue
        </button>
      </div>

      <h3 className="text-lg font-semibold mb-2">Issued Items</h3>
      <table className="w-full text-left border">
        <thead>
          <tr className="bg-gray-200">
            <th className="p-2">Item</th>
            <th className="p-2">Issued To</th>
            <th className="p-2">Department</th>
            <th className="p-2">Quantity</th>
            <th className="p-2">Date</th>
            <th className="p-2">Actions</th>
          </tr>
        </thead>
        <tbody>
          {Array.isArray(issueList) && issueList.length > 0 ? (
            issueList.map((record) => (
              <tr key={record.issue_id} className="border-t">
                <td className="p-2">{record.item_name}</td>
                <td className="p-2">{record.issued_to}</td>
                <td className="p-2">{record.department}</td>
                <td className="p-2">{record.quantity}</td>
                <td className="p-2">{new Date(record.issue_date).toLocaleDateString()}</td>
                <td className="p-2">
                  <button
                    onClick={() => handleRevoke(record.issue_id)}
                    className="bg-red-500 text-white px-2 py-1 rounded"
                  >
                    Revoke
                  </button>
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="6" className="text-center p-4 text-gray-500">
                No items issued yet.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

export default IssueRecord;