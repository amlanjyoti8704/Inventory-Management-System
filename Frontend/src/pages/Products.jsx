// import React, { useEffect, useState } from 'react';
// import { Link } from 'react-router-dom';
// import axios from 'axios';

// function Products() {
//   const [products, setProducts] = useState([]);

//   useEffect(() => {
//     axios.get('https://localhost:5001/api/products')
//       .then(response => setProducts(response.data))
//       .catch(error => console.error('Error fetching products:', error));
//   }, []);

//   return (
//     <div>
//       <h2 className="text-2xl font-semibold mb-4">Products</h2>
//       <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
//         {products.map(product => (
//           <div key={product.id} className="border p-4 rounded shadow">
//             <h3 className="text-xl font-bold">{product.name}</h3>
//             <p className="mt-2 text-gray-700">{product.description}</p>
//             <p className="mt-2 font-semibold">${product.price}</p>
//             <Link to={`/products/${product.id}`} className="text-blue-500 hover:underline mt-2 inline-block">View Details</Link>
//           </div>
//         ))}
//       </div>
//     </div>
//   );
// }

// export default Products;