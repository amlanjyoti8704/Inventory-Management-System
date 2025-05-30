import React from 'react'
import { Outlet, Navigate, useLocation } from 'react-router-dom'
import Navbar from './components/Navbar.jsx'
import Footer from './components/Footer.jsx'

function Layout() {
  const location = useLocation()
  const isLoggedIn = !!localStorage.getItem('loggedInUser')

  // List of public routes
  const publicRoutes = ['/login']

  // If not logged in and not on a public route, redirect to /login
  if (!isLoggedIn && !publicRoutes.includes(location.pathname)) {
    return <Navigate to="/login" replace />
  }

  return (
    <div className="flex flex-col min-h-screen">
      <Navbar />
      <main className="flex-grow">
        <Outlet />
      </main>
      <Footer />
    </div>
  )
}

export default Layout