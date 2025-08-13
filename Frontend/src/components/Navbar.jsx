import React, { useState, useEffect, useRef } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { FiMenu, FiX } from 'react-icons/fi';
import DarkModeToggle from './DarkModeToggle.jsx';
import axios from 'axios';
import { FiBell } from 'react-icons/fi';

function Navbar() {
    const [pendingCount, setPendingCount] = useState(0);
    const [pendingIssues, setPendingIssues] = useState(0);
    const [pendingReturns, setPendingReturns] = useState(0);
    const [showDropdown, setShowDropdown] = useState(false);
    const navigate = useNavigate();
    const loggedInUser = JSON.parse(localStorage.getItem('loggedInUser'));
    const role = loggedInUser?.user?.role;

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
      
        if (role === 'admin' || role === 'staff') {
          fetchPendingCounts();
          const interval = setInterval(fetchPendingCounts, 30000);
          return () => clearInterval(interval);
        }
    }, [role]);

    const handleLogout = () => {
        localStorage.removeItem('loggedInUser');
        navigate('/login');
    };

    const [isOpen, setIsOpen] = useState(false);
    const menuRef = useRef(null);

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (menuRef.current && !menuRef.current.contains(event.target)) {
                setIsOpen(false);
            }
        };

        if (isOpen) {
            document.addEventListener('mousedown', handleClickOutside);
        } else {
            document.removeEventListener('mousedown', handleClickOutside);
        }

        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [isOpen]);

    return (
        <header className='shadow sticky h-[85px] xl:h-auto top-0 z-10 bg-gradient-to-t from-[rgb(0,6,22)] to-[rgb(17,50,100)] backdrop-blur-md border-b border-gray-600'>
            <nav className='lg-px-8 px-4 py-2 pl-0 w-full'>
                <div className="text-white p-4 flex flex-wrap justify-between items-center relative">
                    <div className='flex items-center space-x-4 xl:space-x-10'>
                        <button
                            onClick={() => setIsOpen(!isOpen)}
                            className="xl:hidden text-white text-3xl focus:outline-none order-first flex items-center ml-4"
                            aria-expanded={isOpen}
                        >
                            {isOpen ? <FiX /> : <FiMenu />}
                        </button>

                        <div className='pl-5 text-3xl font-semibold bg-gradient-to-r from-gray-400 via-gray-300 to-white bg-clip-text text-transparent'>
                            IT Consumables
                        </div>
                    </div>
                
                    {/* <div
                        ref={menuRef}
                        className={`xl:flex flex-col xl:flex-row xl:space-x-10 xl:items-center 
                            absolute xl:relative xl:right-0 xl:ml-auto top-[80px] xl:top-0 right-0 left-0 w-[100vw] xl:w-auto 
                            h-[30vh] overflow-hidden xl:overflow-visible xl:h-auto bg-gradient-to-tl from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] xl:bg-transparent z-0 xl:p-0 
                            transition-transform duration-300 ease-in-out 
                            ${isOpen ? 'max-h-[380px] p-4' : 'max-h-0 p-0'}`}
                    > */}
                    <div
                        ref={menuRef}
                        className={`xl:flex flex-col xl:flex-row xl:space-x-10 xl:items-center 
                            absolute xl:relative xl:right-0 xl:ml-auto top-[80px] xl:top-0 right-0 left-0 w-[100vw] xl:w-auto
                            overflow-y-auto xl:overflow-visible bg-gradient-to-tl from-[rgb(0,6,22)] via-[rgb(8,46,66)] to-[rgb(7,7,33)] xl:bg-transparent z-0 xl:p-0 
                            transition-[max-height,padding] duration-300 ease-in-out
                            ${isOpen ? 'max-h-[70vh] p-4' : 'max-h-0 p-0'}
                        `}
                    >
                        {/* <div className='bg-gray-200 opacity-15 h-full w-full'></div> */}
                        <ul className={`flex flex-col font-medium xl:flex-row xl:space-x-10 gap-4 sm:gap-4 lg:gap-8 xl:gap-0 xl:mt-0 xl:items-center xl:mr-5 transition-all duration-300 ease-in-out ${isOpen ? 'mt-4' : 'mt-0'}`}>
                            <li>
                                <NavLink to="/" className={({ isActive }) => `${isActive ? "text-white border-b xl:border-0 xl:scale-120 xl:hover:border-b-0 xl:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent xl:hover:border-b lg:p-0`}>
                                    Home
                                </NavLink>
                            </li>

                            {(role === 'admin' || role === 'staff') && (
                                <li>
                                    <NavLink to='/categories' className={({ isActive }) => `${isActive ? "text-white border-b xl:border-0 xl:scale-120 xl:hover:border-b-0 xl:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent xl:hover:border-b lg:p-0`}>
                                        Categories
                                    </NavLink>
                                </li>
                            )}

                            {(role === 'admin' || role === 'staff') && (
                                <li>
                                    <NavLink to="/items" className={({ isActive }) => `${isActive ? "text-white border-b xl:border-0 xl:scale-120 xl:hover:border-b-0 xl:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent xl:hover:border-b lg:p-0`}>
                                        Items
                                    </NavLink>
                                </li>
                            )}

                            {(role === 'admin' || role === 'user') && (
                                <li>
                                    <NavLink to="/issuerecords" className={({ isActive }) => `${isActive ? "text-white border-b xl:border-0 xl:scale-120 xl:hover:border-b-0 xl:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent xl:hover:border-b lg:p-0`}>
                                        Issue Record
                                    </NavLink>
                                </li>
                            )}

                            {role === 'admin' && (
                                <li>
                                    <NavLink to="/alertlog" className={({ isActive }) => `${isActive ? "text-white border-b xl:border-0 xl:scale-120 xl:hover:border-b-0 xl:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent xl:hover:border-b lg:p-0`}>
                                        Alert Log
                                    </NavLink>
                                </li>
                            )}
                            {role === 'admin' && (
                                <li>
                                    <NavLink to="/usermanagement" className={({ isActive }) => `${isActive ? "text-white border-b xl:border-0 xl:scale-120 xl:hover:border-b-0 xl:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent xl:hover:border-b lg:p-0`}>
                                        Users
                                    </NavLink>
                                </li>
                            )}
                        </ul>
                    </div>
                    <div className='flex items-center justify-between'>
                        <div className='flex items-center gap-4 xl:gap-6'>
                        {(role === 'admin') && (
                                <div className="relative xl:mx-4">
                                    <div
                                    className="cursor-pointer relative"
                                    onClick={() => setShowDropdown(prev => !prev)}
                                    
                                    >
                                    <FiBell className="text-xl text-white hover:text-blue-400" />
                                    {(pendingIssues + pendingReturns > 0) && (
                                        <span className="absolute -top-2 -right-2 bg-red-600 text-white text-xs font-bold px-1.5 py-0.5 rounded-full">
                                        {pendingIssues + pendingReturns}
                                        </span>
                                    )}
                                    </div>

                                    {showDropdown && (
                                    <div className="absolute right-0 top-13 mt-2 w-[17vw] text-white rounded shadow-lg z-50 p-5 bg-gray-500/50">
                                        <div className="mb-2 text-center text-xl font-semibold">Notifications</div>
                                        <div className='rounded px-1 gap-2 flex flex-col'>
                                            <div
                                            className="cursor-pointer bg-gray-600 hover:bg-gray-700 transition-colors p-2 rounded"
                                            onClick={() => {
                                                setShowDropdown(false);
                                                navigate('/issuerecords');
                                            }}
                                            >
                                            Pending Issue Requests: <span className="font-bold">{pendingIssues}</span>
                                            </div>
                                            <div
                                            className="cursor-pointer bg-gray-600 hover:bg-gray-700 transition-colors p-2 rounded"
                                            onClick={() => {
                                                setShowDropdown(false);
                                                navigate('/issuerecords');
                                            }}
                                            >
                                            Pending Return Requests: <span className="font-bold">{pendingReturns}</span>
                                            </div>
                                        </div>
                                    </div>
                                    )}
                                </div>
                        )} 
                        </div> 
                        {/* {loggedInUser && (
                            <div className="flex ml-4 items-center space-x-4">
                                <Link to="/profile" className="text-white hover:text-blue-400">
                                    {loggedInUser.user.role === 'admin' ? 'Admin' : 'User'}
                                </Link>
                            </div>
                        )}                   */}

                        {loggedInUser && (
                            <button onClick={handleLogout} className="bg-red-500 px-3 py-1 ml-10 rounded">
                                Logout
                            </button>
                        )}
                    </div>
                </div>
            </nav>
        </header>
    );
}

export default Navbar;