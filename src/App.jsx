import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Home from './pages/Home.jsx';
import Products from './pages/Products.jsx';
import Categories from './pages/Categories.jsx';
import Items from './pages/Items.jsx';
import ProductDetails from './pages/ProductDetails.jsx';
import Navbar from './components/Navbar.jsx';
import PurchaseOrders from './pages/PurchaseOrders.jsx';
import IssueRecords from './pages/IssueRecords.jsx';
import Suppliers from './pages/Suppliers.jsx';


function App() {
  return (
   
      <Router>
      <Navbar />
      <div className="p-4">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/categories" element={<Categories />} />
          <Route path="/categories/:id" element={<Categories />} />
          <Route path="/items" element={<Items />} />
          <Route path="/items/:id" element={<Items />} />
          <Route path="/products" element={<Products />} />
          <Route path="/products/:id" element={<ProductDetails />} />
          <Route path="/purchaseorders" element={<PurchaseOrders />} />
          <Route path="/purchaseorders/:id" element={<PurchaseOrders />} />
          <Route path="/issuerecords" element={<IssueRecords />} />
          <Route path="/issuerecords/:id" element={<IssueRecords />} />
          <Route path="/suppliers" element={<Suppliers />} />
          <Route path="/suppliers/:id" element={<Suppliers />} />
          {/* Add more routes as needed */}
        </Routes>
      </div>
    </Router>
    
  );
}

export default App;