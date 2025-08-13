import React, { useEffect, useState, useMemo } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
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
  const [sortField, setSortField] = useState('');
  const [sortOrder, setSortOrder] = useState('asc'); // 'asc' or 'desc'
  const [pendingIssues, setPendingIssues] = useState(0);
  const [pendingReturns, setPendingReturns] = useState(0);
  const [showOnlyPending, setShowOnlyPending] = useState(false);

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
    
        // const res = await axios.get(`https://my-backend-sdbk.onrender.com/api/user/me?email=${user.email}`);
        const res=await axios.get(`http://localhost:5007/api/user/me?email=${user.email}`);
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

  const filteredIssues = useMemo(() => {
    if (!showOnlyPending) return issueList;
    return issueList.filter(issue =>
      issue.status?.toLowerCase() === 'requested' ||
      issue.status?.toLowerCase() === 'pending' ||
      (issue.return_status?.toLowerCase() === 'requested')
    );
  }, [showOnlyPending, issueList]);

  useEffect(() => {
    const fetchPendingCounts = async () => {
      try {
        // const res = await axios.get('https://my-backend-sdbk.onrender.com/api/issue/pending-requests');
        const res = await axios.get('http://localhost:5007/api/issue/pending-requests');
        const data = res.data || [];
        console.log('API Data:', data);
        const issueCount = data.filter(r => r.status === 'requested' || r.status === 'pending').length;
        const returnCount = data.filter(r =>
            r.returnStatus && r.returnStatus.trim().toLowerCase() === 'requested'
          ).length;
  
        setPendingIssues(issueCount);
        setPendingReturns(returnCount);
      } catch (error) {
        console.error('Error fetching pending counts:', error);
      }
    };
  
    if (userRole === 'admin' || userRole === 'staff') {
      fetchPendingCounts();
      const interval = setInterval(fetchPendingCounts, 30000);
      return () => clearInterval(interval);
    }
  }, [userRole]);

  useEffect(() => {
    if (loading) return;
    if (userRole && userEmail) {
      fetchItems();
      fetchIssueList();
    }
  }, [loading, userRole, userEmail]);

  const fetchItems = async () => {
    try {
      // const res = await axios.get('https://my-backend-sdbk.onrender.com/api/issue/items');
      const res = await axios.get('http://localhost:5007/api/issue/items');
      setItems(Array.isArray(res.data) ? res.data : []);
    } catch (err) {
      console.error("Failed to fetch items:", err);
      setItems([]);
    }
  };

  const fetchIssueList = async () => {
    try {
      // const res = await axios.get('https://my-backend-sdbk.onrender.com/api/issue');
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

  const sortIssueList = (field) => {
    let order = 'asc';
    if (sortField === field && sortOrder === 'asc') {
      order = 'desc';
    }
    setSortField(field);
    setSortOrder(order);
  
    const sorted = [...issueList].sort((a, b) => {
      if (a[field] < b[field]) return order === 'asc' ? -1 : 1;
      if (a[field] > b[field]) return order === 'asc' ? 1 : -1;
      return 0;
    });
  
    setIssueList(sorted);
  };

  const handleRequestIssue = async () => {
    if (!selectedItemId || quantity <= 0 || !username.trim() || !department.trim()) {
      alert('All fields are required and valid');
      return;
    }
  
    try {
      // await axios.post('https://my-backend-sdbk.onrender.com/api/issue', {
      //   item_id: selectedItemId,
      //   issued_to: username.trim(),
      //   quantity: parseInt(quantity),
      //   department: department.trim(),
      //   requested_by: userEmail,
      //   status: 'requested',
      // });
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
      // await axios.put(`https://my-backend-sdbk.onrender.com/api/issue/request-return/${issue_id}`);
      await axios.put(`http://localhost:5007/api/issue/request-return/${issue_id}`);
      alert("Return request sent.");
      fetchIssueList();
    } catch (err) {
      alert('Error sending return request');
    }
  };

  const handleApprove = async (issueId) => {
    try {
      // const res = await axios.put(`https://my-backend-sdbk.onrender.com/api/issue/approve/${issueId}`);
      const res = await axios.put(`http://localhost:5007/api/issue/approve/${issueId}`);
      alert(res.data.message);
      fetchIssueList();
    } catch (err) {
      alert(err.response?.data?.error || "Failed to approve request.");
    }
  };
  
  const handleReject = async (issueId) => {
    try {
      // const res = await axios.put(`https://my-backend-sdbk.onrender.com/api/issue/decline/${issueId}`);
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
      // await axios.delete(`https://my-backend-sdbk.onrender.com/api/issue/${issueId}`);
      await axios.delete(`http://localhost:5007/api/issue/${issueId}`);
      fetchIssueList();
    } catch (err) {
      alert('Error deleting issue record');
    }
  };

  const handleAcceptReturn = async (issueId) => {
    try {
      // await axios.put(`https://my-backend-sdbk.onrender.com/api/issue/approve-return/${issueId}`);
      await axios.put(`http://localhost:5007/api/issue/approve-return/${issueId}`);
      alert("Return approved.");
      fetchIssueList();
    } catch (err) {
      alert("Failed to approve return.");
    }
  };
  
  const handleRejectReturn = async (issue_id) => {
    try {
      console.log("Rejecting return for issue_id:", issue_id); // helpful debug
      // await axios.put(`https://my-backend-sdbk.onrender.com/api/issue/reject-return/${issue_id}`);
      await axios.put(`http://localhost:5007/api/issue/reject-return/${issue_id}`);
      alert("Return rejected.");
      fetchIssueList();
    } catch (err) {
      console.error("Reject return error:", err.response?.data || err.message);
      alert("Failed to reject return: " + (err.response?.data?.error || err.message));
    }
  };

  function safeRender(val) {
    if (val === null || val === undefined) return 'N/A';
    if (typeof val === 'object') return JSON.stringify(val, null, 2); // safely convert
    return val.toString?.() || 'N/A';  // use .toString() if available
  }
  

  // Check if user is loading

  if (loading) return <div className="p-6 text-white flex min-h-screen justify-center items-center text-2xl">Loading...</div>;

  return (
    <div className="p-6 bg-gradient-to-b from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] text-white min-h-screen overflow-scroll flex flex-col justify-center items-center">
      <h2 className="text-3xl font-bold">Issue Management</h2>
      <div className='mx-auto p-15 m-auto mt-[50px] mb-[90px] w-[90vw] border border-gray-600 rounded-3xl shadow-2xl bg-transparent bg-opacity-30 backdrop-blur-lg'>
        <div className="absolute inset-0 bg-black opacity-20 -z-10 rounded-3xl"></div>
          

          {/* USER: Request Item Form */}
          {(userRole === 'user' || userRole ==='admin') && (
            <div className="grid md:grid-row-3 gap-4 mb-6">
              <select
                value={selectedItemId}
                onChange={(e) => setSelectedItemId(e.target.value)}
                className="bg-slate-900 text-white border px-3 py-2 rounded"
              >
                <option value="">Select Item</option>
                {items.map((item) => (
                  <option key={item.item_id} value={item.item_id}>
                    {item.item_name}
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
              <div className='flex justify-between items-center mb-4'>
                <h3 className="text-lg font-semibold mb-2">Issue Requests</h3>
                {userRole === 'admin' && (
                <div className='flex justify-end gap-2.5 mb-2'>
                    <div
                        className="cursor-pointer bg-gray-600 hover:bg-gray-700 transition-colors p-2 rounded"
                        onClick={() => {
                        setShowDropdown(false);
                         
                        }}
                    >
                      Pending Issue Requests: <span className="font-bold">{pendingIssues}</span>
                    </div>
                    <div
                      className="cursor-pointer bg-gray-600 hover:bg-gray-700 transition-colors p-2 rounded"
                      onClick={() => {
                      setShowDropdown(false);
                      
                      }}
                    >
                      Pending Return Requests: <span className="font-bold">{pendingReturns}</span>
                    </div>   
                    <div className="p-2 flex justify-center items-center">
                      <input
                        type="checkbox"
                        id="pendingFilter"
                        checked={showOnlyPending}
                        onChange={() => setShowOnlyPending(!showOnlyPending)}
                        className='cursor-pointer size-5 text-blue-600 border-gray-300 rounded focus:ring-blue-500 focus:ring-2 '
                      />
                      <label className='ml-2' htmlFor="pendingFilter">Show only pending requests</label>
                    </div>             
                </div>
                )}
              </div>

              <table className="w-full text-left opacity-70">
                <thead>
                  <tr className="bg-gray-700 text-center border-b">
                    <th className='p-2 cursor-pointer rounded-tl-lg' onClick={() => sortIssueList('item_name')}>Item Name</th>
                    <th className="p-2 cursor-pointer" onClick={() => sortIssueList('issued_to')}>Issued To</th>
                    <th className="p-2 cursor-pointer" onClick={() => sortIssueList('email')}>Email</th>
                    <th className="p-2 cursor-pointer" onClick={() => sortIssueList('department')}>Department</th>
                    <th className="p-2 cursor-pointer" onClick={() => sortIssueList('quantity')}>Quantity</th>
                    <th className="p-2 cursor-pointer" onClick={() => sortIssueList('date')}>Date</th>
                    <th className="p-2">Issue-Status</th>
                    <th className='p-2'>Return-Status</th>
                    <th className="p-2 rounded-tr-lg">Actions</th>
                  </tr>
                </thead>
                {/* {console.log('ISSUE RECORDS:', issueList)} */}
                <tbody>
                  {issueList.length > 0 ? (
                    filteredIssues.map((record) => {
                      console.log('RECORD:', JSON.stringify(record));
                      console.log("item_name type:", typeof record.requested_by, record.requested_by);
                      console.log('return_status:', record.return_status);
                      
                      return(
                      <tr key={record.issue_id} className="border-b text-center hover:bg-gray-800 transition-colors">
                        {console.log(record.issue_id, record.status)}
                        <td className="p-2">{safeRender(record.item_name)}</td>
                        <td className="p-2">{safeRender(record.issued_to)}</td>
                        <td className="p-2">{safeRender(record.requested_by)}</td>
                        <td className="p-2">{safeRender(record.department)}</td>
                        <td className="p-2">{safeRender(record.quantity)}</td>
                        <td className="p-2">{record.issue_date ? new Date(record.issue_date).toLocaleDateString() : 'N/A'}</td>
                        <td className="p-2 capitalize">{safeRender(record.status)}</td>
                        <td className="p-2 capitalize">{safeRender(record.return_status) || 'none'}</td>

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
                          {userRole==='admin' && ((record.status?.toLowerCase() === 'approved' || record.status?.toLowerCase() ==='declined' || record.return_status?.toLowerCase()==='approved') && record.return_status?.toLowerCase() !=='requested') && (
                            //record.status?.toLowerCase() === 'requested' || record.status?.toLowerCase() === 'pending' || 
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

                          {/* ADMIN: Accept/Reject Return */}
                          {userRole === 'admin' && record.return_status === 'requested' && (
                            <>
                            <div className='flex justify-center items-center gap-2'>
                              <button 
                                onClick={() => handleAcceptReturn(record.issue_id)}
                                className="bg-green-600 text-white px-2 py-1 rounded"
                              >
                                Accept Return
                              </button>
                              <button 
                                onClick={() => handleRejectReturn(record.issue_id)}
                                className="bg-red-600 text-white px-2 py-1 rounded"
                              >
                                Reject Return
                              </button>
                            </div> 
                            </>
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