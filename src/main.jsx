import React from 'react'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { createBrowserRouter, createRoutesFromElements, Route, RouterProvider } from 'react-router-dom'
import './index.css'

import Layout from './Layout.jsx'
import Home from './pages/Home.jsx'
import CategoryPage from './pages/Categories.jsx'
import Items from './pages/Items.jsx'
import IssueRecord from './pages/IssueRecords.jsx'
import Login from './pages/Login.jsx'
import SignUp from './pages/SignUp.jsx'
import ProtectedRoute from './components/ProtectedRoute.jsx'

const router = createBrowserRouter(
  createRoutesFromElements(
    <>
      <Route path='/signup' element={<SignUp />} />
      <Route path='/login' element={<Login />} />
      <Route path='/' element={<Layout />}>
        <Route index element={
          <ProtectedRoute>
            <Home />
          </ProtectedRoute>
        } />
        <Route path='categories' element={
          <ProtectedRoute>
            <CategoryPage />
          </ProtectedRoute>
        } />
        <Route path='items' element={
          <ProtectedRoute>
            <Items />
          </ProtectedRoute>
        } />
        {/* <Route path='productdetails' element={
          <ProtectedRoute>
            <ProductDetails />
          </ProtectedRoute>
        } /> */}
        <Route path='issuerecords' element={
          <ProtectedRoute>
            <IssueRecord />
          </ProtectedRoute>
        } />
      </Route>
    </>
  )
)

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>
)