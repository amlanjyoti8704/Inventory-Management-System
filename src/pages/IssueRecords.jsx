import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

function IssueRecord() {
  const [items, setItems] = useState([]);
  const [issueList, setIssueList] = useState([]);
  const [selectedItemId, setSelectedItemId] = useState('');
  const [quantity, setQuantity] = useState('');
  const [department, setDepartment] = useState('');
  const [username, setUsername] = useState('');
  const [userRole, setUserRole] = useState('');
  const [userEmail, setUserEmail] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchCurrentUser = async () => {
      try {
        const storedUserStr = localStorage.getItem('loggedInUser');
        if (!storedUserStr) {
          console.error("No user found in localStorage");
          setLoading(false);
          return;
        }
    
        const storedUser = JSON.parse(storedUserStr);
        const user = storedUser?.user;
    
        if (!user?.email) {
          console.error("No email found in localStorage");
          setLoading(false);
          return;
        }
    
        const res = await axios.get(`http://localhost:5007/api/user/me?email=${user.email}`);
        setUserRole(res.data.role?.toLowerCase());
        setUserEmail(res.data.email);
      } catch (err) {
        console.error("Error fetching user:", err);
        alert("Failed to fetch user information. Please try again.");
      } finally {
        setLoading(false);
      }
    };
  
    fetchCurrentUser();
  }, []);

  useEffect(() => {
    if (loading) return;
    if (userRole && userEmail) {
      fetchItems();
      fetchIssueList();
    }
  }, [loading, userRole, userEmail]);

  const fetchItems = async () => {
    try {
      const res = await axios.get('http://localhost:5007/api/issue/items');
      setItems(Array.isArray(res.data) ? res.data : []);
    } catch (err) {
      console.error("Failed to fetch items:", err);
      setItems([]);
    }
  };

  const fetchIssueList = async () => {
    try {
      const res = await axios.get('http://localhost:5007/api/issue');
      const data = Array.isArray(res.data) ? res.data : [];

      if (userRole === 'admin') {
        setIssueList(data);
      } else {
        const userRequests = data.filter(record => record.requested_by === userEmail);
        setIssueList(userRequests);
      }
    } catch (err) {
      console.error("Failed to fetch issued list:", err);
      setIssueList([]);
    }
  };

  const handleRequestIssue = async () => {
    if (!selectedItemId || quantity <= 0 || !username.trim() || !department.trim()) {
      alert('All fields are required and valid');
      return;
    }
  
    try {
      await axios.post('http://localhost:5007/api/issue', {
        item_id: selectedItemId,
        issued_to: username.trim(),
        quantity: parseInt(quantity),
        department: department.trim(),
        requested_by: userEmail,
        status: 'requested',
      });
  
      setSelectedItemId('');
      setUsername('');
      setQuantity('');
      setDepartment('');
      fetchItems();
      fetchIssueList();
    } catch (err) {
      const errorMessage = err.response?.data?.error || err.message;
      alert('Error requesting item: ' + errorMessage);
    }
  };
  

  const handleReturn = async (issue_id) => {
    try {
      await axios.delete(`http://localhost:5007/api/issue/${issue_id}`);
      fetchIssueList();
    } catch (err) {
      alert('Error returning item');
    }
  };

  const handleApprove = async (issueId) => {
    try {
      const res = await axios.put(`http://localhost:5007/api/issue/approve/${issueId}`);
      alert(res.data.message);
      fetchIssueList();
    } catch (err) {
      alert(err.response?.data?.error || "Failed to approve request.");
    }
  };
  
  const handleReject = async (issueId) => {
    try {
      const res = await axios.put(`http://localhost:5007/api/issue/decline/${issueId}`);
      alert(res.data.message);
      fetchIssueList();
    } catch (err) {
      alert(err.response?.data?.error || "Failed to decline request.");
    }
  };
  const handleDelete = async (issueId) => {
    if (!window.confirm('Are you sure you want to delete this issue record?')) return;
    try {
      await axios.delete(`http://localhost:5007/api/issue/${issueId}`);
      fetchIssueList();
    } catch (err) {
      alert('Error deleting issue record');
    }
  };

  function safeRender(val) {
    if (val === null || val === undefined) return 'N/A';
    if (typeof val === 'object') return JSON.stringify(val, null, 2); // safely convert
    return val.toString?.() || 'N/A';  // use .toString() if available
  }
  

  // Check if user is loading

  if (loading) return <div className="p-6">Loading...</div>;

  return (
    <div className="p-6 bg-gradient-to-b from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] text-white h-[80vh] flex flex-col justify-center items-center">
      <h2 className="text-3xl font-bold">Issue Management</h2>
      <div className='mx-auto p-15 m-auto mt-[50px] mb-[90px] w-[90vw] border border-gray-600 rounded-3xl shadow-2xl bg-transparent bg-opacity-30 backdrop-blur-lg'>
        <div className="absolute inset-0 bg-black opacity-20 -z-10 rounded-3xl"></div>
          

          {/* USER: Request Item Form */}
          {userRole === 'user' && (
            <div className="grid md:grid-row-3 gap-4 mb-6">
              <select
                value={selectedItemId}
                onChange={(e) => setSelectedItemId(e.target.value)}
                className="border px-3 py-2 rounded"
              >
                <option value="">Select Item</option>
                {items.map((item) => (
                  <option key={item.item_id} value={item.item_id}>
                    {item.item_name} (Stock: {item.stock})
                  </option>
                ))}
              </select>
              <input 
                type="username" 
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="Username"
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

              <input 
                type="department" 
                value={department}
                onChange={(e) => setDepartment(e.target.value)}
                placeholder="Department"
                className="border px-3 py-2 rounded"
              />

              <button
                onClick={handleRequestIssue}
                className="bg-blue-600 text-white px-4 py-2 rounded"
              >
                Request Issue
              </button>
            </div>
          )}

          {/* ADMIN and USER: View Issue Records */}
          {(userRole === 'admin' || userRole === 'user') && (
            // <p>This is a test</p>
            <>
              <h3 className="text-lg font-semibold mb-2">Issue Requests</h3>
              <table className="w-full text-left border opacity-70">
                <thead>
                  <tr className="bg-gray-700 text-center">
                    <th className="p-2">Item</th>
                    <th className="p-2">Issued To</th>
                    <th className="p-2">email</th>
                    <th className="p-2">Department</th>
                    <th className="p-2">Quantity</th>
                    <th className="p-2">Date</th>
                    <th className="p-2">Status</th>
                    <th className="p-2">Actions</th>
                  </tr>
                </thead>
                {/* {console.log('ISSUE RECORDS:', issueList)} */}
                <tbody>
                  {issueList.length > 0 ? (
                    issueList.map((record) => {
                      console.log('RECORD:', JSON.stringify(record));
                      console.log("item_name type:", typeof record.requested_by, record.requested_by);
                      return(
                      <tr key={record.issue_id} className="border-t text-center">
                        {console.log(record.issue_id, record.status)}
                        <td className="p-2">{safeRender(record.item_name)}</td>
                        <td className="p-2">{safeRender(record.issued_to)}</td>
                        <td className="p-2">{safeRender(record.requested_by)}</td>
                        <td className="p-2">{safeRender(record.department)}</td>
                        <td className="p-2">{safeRender(record.quantity)}</td>
                        <td className="p-2">{record.issue_date ? new Date(record.issue_date).toLocaleDateString() : 'N/A'}</td>
                        <td className="p-2 capitalize">{safeRender(record.status)}</td>

                        <td className="p-2 space-x-2">
                          {/* ADMIN: Approve / Reject buttons */}
                          {userRole === 'admin' && (record.status?.toLowerCase() === 'requested' || record.status?.toLowerCase() === 'pending') && (
                            <>
                              <button
                                onClick={() => handleApprove(record.issue_id)}
                                className="bg-green-600 text-white px-2 py-1 rounded"
                              >
                                Approve
                              </button>
                              <button
                                onClick={() => handleReject(record.issue_id)}
                                className="bg-red-600 text-white px-2 py-1 rounded"
                              >
                                Reject
                              </button>
                              
                            </>
                          )}
                          {userRole==='admin' && (record.status?.toLowerCase() === 'requested' || record.status?.toLowerCase() === 'pending' || record.status?.toLowerCase() === 'approved' || record.status?.toLowerCase() ==='declined') && (
                            <>
                              <button
                                onClick={()=> handleDelete(record.issue_id)}
                                className="bg-red-600 text-white px-2 py-1 rounded"
                              >
                                Delete
                              </button>
                            </>
                          )}

                          {/* USER: Return button */}
                          {userRole === 'user' && record.status === 'approved' && (
                            <button
                              onClick={() => handleReturn(record.issue_id)}
                              className="bg-yellow-500 text-white px-2 py-1 rounded"
                            >
                              Return
                            </button>
                          )}
                        </td>
                      </tr>
                      )
          })
                  ) : (
                    <tr>
                      <td colSpan="8" className="text-center p-4 text-gray-500">
                        No issue records found.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </>
          )}

      </div>
    </div>
  );
}

export default IssueRecord;