import React, { useState, useEffect, useRef } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { FiMenu, FiX } from 'react-icons/fi';
import DarkModeToggle from './DarkModeToggle.jsx';

function Navbar() {
    const navigate = useNavigate();
    const loggedInUser = JSON.parse(localStorage.getItem('loggedInUser'));
    const role = loggedInUser?.user?.role;

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
        <header className='shadow sticky h-[85px] md:h-auto top-0 z-10 bg-gradient-to-t from-[rgb(0,6,22)] to-[rgb(17,50,100)] backdrop-blur-md border-b border-gray-600'>
            <nav className='lg-px-8 px-4 py-2 pl-0 w-full'>
                <div className="text-white p-4 flex flex-wrap justify-between items-center relative">
                    <button
                        onClick={() => setIsOpen(!isOpen)}
                        className="md:hidden text-white text-3xl focus:outline-none order-first flex items-center ml-4"
                        aria-expanded={isOpen}
                    >
                        {isOpen ? <FiX /> : <FiMenu />}
                    </button>

                    <div className='pl-5 text-3xl font-semibold bg-gradient-to-r from-gray-400 via-gray-300 to-white bg-clip-text text-transparent'>
                        IT Consumables
                    </div>

                    <div
                        ref={menuRef}
                        className={`md:flex flex-col md:flex-row md:space-x-10 md:items-center 
                            absolute md:relative md:right-0 md:ml-auto top-[80px] md:top-0 right-0 left-0 w-[100vw] md:w-auto 
                            h-[30vh] overflow-hidden md:overflow-visible md:h-auto bg-blue-300 md:bg-transparent z-0 md:p-0 
                            transition-transform duration-300 ease-in-out 
                            ${isOpen ? 'max-h-[300px] p-4' : 'max-h-0 p-0'}`}
                    >
                        <ul className={`flex flex-col font-medium md:flex-row md:space-x-10 md:mt-0 md:items-center md:mr-5 transition-all duration-300 ease-in-out ${isOpen ? 'mt-4' : 'mt-0'}`}>
                            <li>
                                <NavLink to="/" className={({ isActive }) => `${isActive ? "text-white border-b md:border-0 md:scale-120 md:hover:border-b-0 md:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent md:hover:border-b lg:p-0`}>
                                    Home
                                </NavLink>
                            </li>

                            {(role === 'admin' || role === 'staff') && (
                                <li>
                                    <NavLink to='/categories' className={({ isActive }) => `${isActive ? "text-white border-b md:border-0 md:scale-120 md:hover:border-b-0 md:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent md:hover:border-b lg:p-0`}>
                                        Categories
                                    </NavLink>
                                </li>
                            )}

                            {(role === 'admin' || role === 'staff') && (
                                <li>
                                    <NavLink to="/items" className={({ isActive }) => `${isActive ? "text-white border-b md:border-0 md:scale-120 md:hover:border-b-0 md:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent md:hover:border-b lg:p-0`}>
                                        Items
                                    </NavLink>
                                </li>
                            )}

                            {(role === 'admin' || role === 'user') && (
                                <li>
                                    <NavLink to="/issuerecords" className={({ isActive }) => `${isActive ? "text-white border-b md:border-0 md:scale-120 md:hover:border-b-0 md:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent md:hover:border-b lg:p-0`}>
                                        Issue Record
                                    </NavLink>
                                </li>
                            )}

                            {role === 'admin' && (
                                <li>
                                    <NavLink to="/alertlog" className={({ isActive }) => `${isActive ? "text-white border-b md:border-0 md:scale-120 md:hover:border-b-0 md:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent md:hover:border-b lg:p-0`}>
                                        Alert Log
                                    </NavLink>
                                </li>
                            )}
                            {role === 'admin' && (
                                <li>
                                    <NavLink to="/usermanagement" className={({ isActive }) => `${isActive ? "text-white border-b md:border-0 md:scale-120 md:hover:border-b-0 md:text-blue-200" : "lg:border-0 text-gray-100"} block py-2 pr-2 pl-1 duration-200 hover:bg-transparent md:hover:border-b lg:p-0`}>
                                        Users
                                    </NavLink>
                                </li>
                            )}
                        </ul>
                    </div>

                    {loggedInUser && (
                        <button onClick={handleLogout} className="bg-red-500 px-3 py-1 ml-10 rounded">
                            Logout
                        </button>
                    )}
                </div>
            </nav>
        </header>
    );
}

export default Navbar;