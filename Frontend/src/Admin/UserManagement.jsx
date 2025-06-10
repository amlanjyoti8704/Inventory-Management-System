import React, { useEffect, useState } from 'react';
import axios from 'axios';

function UserManagement() {
    const [users, setUsers] = useState([]);

    const fetchUsers = async () => {
        try {
            const res = await axios.get('https://my-backend-sdbk.onrender.com/api/user');
            setUsers(res.data);
        } catch (err) {
            console.error('Failed to fetch users:', err);
        }
    };

    const handleRoleChange = async (email, newRole) => {
        try {
            await axios.put('https://my-backend-sdbk.onrender.com/api/user/update-role', { Email: email, Role: newRole });
            // console.log("Role updated:", res.data);
            setUsers(prevUsers =>
                prevUsers.map(user =>
                    user.email === email ? { ...user, role: newRole } : user
                )
            );
        } catch (err) {
            console.error('Failed to update role:', err);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    return (
        <div className="p-6 min-w-70 bg-gradient-to-b from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] text-white w-full h-[80vh] flex flex-col justify-center items-center ">
            <h2 className="text-3xl font-bold">User Management</h2>
            <div className='p-15 mx-auto m-auto mt-[50px] mb-[90px] w-[90vw] border border-gray-600 rounded-3xl shadow-2xl bg-transparent bg-opacity-30 backdrop-blur-lg'>
                <div className="absolute inset-0 bg-black opacity-20 -z-10 rounded-3xl"></div>

                <table className="w-full text-center table-auto border-collapse opacity-80">
                    <thead>
                        <tr className="bg-gray-700 text-white">
                            <th className="px-4 py-2 border-b rounded-tl-lg">Username</th>
                            <th className="px-4 py-2 border-b">Email</th>
                            <th className="px-4 py-2 border-b">Role</th>
                            <th className="px-4 py-2 border-b rounded-tr-lg">Change Role</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map(user => (
                            <tr key={user.email} className="bg-transparent text-white">
                                <td className="px-4 py-2 border-b">{user.username}</td>
                                <td className="px-4 py-2 border-b">{user.email}</td>
                                <td className="px-4 py-2 border-b">{user.role}</td>
                                <td className="px-4 py-2 border-b">
                                    <select
                                        value={user.role}
                                        onChange={(e) => handleRoleChange(user.email, e.target.value)}
                                        className="rounded px-2 py-1"
                                    >
                                        <option value="user">User</option>
                                        {/* <option value="staff">Staff</option> */}
                                        <option value="admin">Admin</option>
                                    </select>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export default UserManagement;