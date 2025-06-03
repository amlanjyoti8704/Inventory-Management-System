import React, { useEffect, useState } from 'react';
import axios from 'axios';

function UserManagement() {
    const [users, setUsers] = useState([]);

    const fetchUsers = async () => {
        try {
            const res = await axios.get('http://localhost:5007/api/user');
            setUsers(res.data);
        } catch (err) {
            console.error('Failed to fetch users:', err);
        }
    };

    const handleRoleChange = async (email, newRole) => {
        try {
            await axios.put('http://localhost:5007/api/user/update-role', { Email: email, Role: newRole });
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
        <div className="p-4 text-white">
            <h2 className="text-2xl font-bold mb-4">User Management</h2>
            <table className="w-full text-left table-auto border-collapse">
                <thead>
                    <tr className="bg-blue-800 text-white">
                        <th className="px-4 py-2 border">Username</th>
                        <th className="px-4 py-2 border">Email</th>
                        <th className="px-4 py-2 border">Role</th>
                        <th className="px-4 py-2 border">Change Role</th>
                    </tr>
                </thead>
                <tbody>
                    {users.map(user => (
                        <tr key={user.email} className="bg-blue-200 text-black">
                            <td className="px-4 py-2 border">{user.username}</td>
                            <td className="px-4 py-2 border">{user.email}</td>
                            <td className="px-4 py-2 border">{user.role}</td>
                            <td className="px-4 py-2 border">
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
    );
}

export default UserManagement;