// main.jsx or index.jsx
import React from 'react';
import { SpeedInsights } from '@vercel/speed-insights/react';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import {
  createBrowserRouter,
  createRoutesFromElements,
  Route,
  RouterProvider
} from 'react-router-dom';
import './index.css';

import Layout from './Layout.jsx';
import Home from './pages/Home.jsx';
import CategoryPage from './pages/Categories.jsx';
import Items from './pages/Items.jsx';
import IssueRecord from './pages/IssueRecords.jsx';
import Login from './pages/Login.jsx';
import SignUp from './pages/SignUp.jsx';
import ProtectedRoute from './components/ProtectedRoute.jsx';
import UserManagement from './Admin/UserManagement.jsx';
import Alert_log from './pages/Alert_log.jsx';
import ForgotPassword from './pages/ForgotPassword.jsx';
import ResetPassword from './pages/ResetPassword.jsx';

const router = createBrowserRouter(
  createRoutesFromElements(
    <>
      <Route path="/signup" element={<SignUp />} />
      <Route path="/login" element={<Login />} />
      <Route path="/forgot-password" element={<ForgotPassword />} />
      <Route path="/reset-password" element={<ResetPassword />} />
      <Route path="/" element={<Layout />}>
        <Route
          index
          element={
            <ProtectedRoute>
              <Home />
            </ProtectedRoute>
          }
        />
        <Route
          path="categories"
          element={
            <ProtectedRoute allowedRoles={['admin', 'staff']}>
              <CategoryPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="items"
          element={
            <ProtectedRoute allowedRoles={['admin', 'staff']}>
              <Items />
            </ProtectedRoute>
          }
        />
        <Route
          path="issuerecords"
          element={
            <ProtectedRoute allowedRoles={['admin', 'user']}>
              <IssueRecord />
            </ProtectedRoute>
          }
        />
        <Route
          path="alertlog"
          element={
            <ProtectedRoute allowedRoles={['admin']}>
              <Alert_log />
            </ProtectedRoute>
          }
        />
        <Route
          path="usermanagement"
          element={
            <ProtectedRoute allowedRoles={['admin']}>
              <UserManagement />
            </ProtectedRoute>
          }
        />
      </Route>
      <SpeedInsights />
    </>
  )
);

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>
);