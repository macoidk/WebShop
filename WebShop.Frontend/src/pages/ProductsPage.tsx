import React, { useEffect, useState } from 'react';
import { useParams, useLocation, Link } from 'react-router-dom';
import './ProductsPage.css';
import type { Product } from '../types/interfaces';

const ProductsPage: React.FC = () => {
  const { category } = useParams<{ category?: string }>();
  const location = useLocation();
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [pageTitle, setPageTitle] = useState<string>('Каталог товарів');

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      setError(null);
      let url = '/api/Products'; 
      let queryCategory = category;

      const queryParams = new URLSearchParams(location.search);
      const categoryFromQuery = queryParams.get('category');

      if (categoryFromQuery) {
        queryCategory = categoryFromQuery;
      }

      if (queryCategory) {
        url = `/api/Products/category/${queryCategory}`;
        setPageTitle(`Товари категорії: ${queryCategory.charAt(0).toUpperCase() + queryCategory.slice(1)}`);
      } else {
        setPageTitle('Усі товари');
      }

      try {
        const response = await fetch(url);
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        setProducts(data as Product[]);
      } catch (e) {
        if (e instanceof Error) {
            setError(e.message);
        } else {
            setError('Виникла невідома помилка');
        }
        console.error("Не вдалося завантажити товари:", e);
      }
      setLoading(false);
    };

    fetchProducts();
  }, [category, location.search]);

  if (loading) return <p>Завантаження товарів...</p>;
  if (error) return <p>Помилка завантаження товарів: {error}</p>;

  return (
    <div className="products-page">
      <h1>{pageTitle}</h1>
      {products.length === 0 && !loading && <p>Товари не знайдено.</p>}
      <div className="products-grid">
        {products.map((product) => {
          const displayImageUrl = product.imageUrls && product.imageUrls.length > 0 ? product.imageUrls[0] : null;
          
          return (
            <Link to={`/product/${product.id}`} key={product.id} className="product-card-link">
              <div className="product-card">
                {displayImageUrl ? (
                  <img src={displayImageUrl} alt={product.name} className="product-image"/>
                ) : (
                  <div className="product-image-placeholder">No Image</div>
                )}
                <div className="product-info">
                  <h2 className="product-name">{product.name}</h2>
                  <p className="product-price">Ціна: {product.price} грн</p>
                </div>
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
};

export default ProductsPage; 