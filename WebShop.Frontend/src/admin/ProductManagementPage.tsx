import React, { useState, useEffect, useCallback } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import type { Product, User } from '../types/interfaces';
import './ProductManagementPage.css';

const fetchProductsWithFilters = async (filters: { category?: string; searchTerm?: string; sortBy?: string }): Promise<Product[]> => {
  const queryParams = new URLSearchParams();
  if (filters.category) queryParams.append('category', filters.category);
  if (filters.searchTerm) queryParams.append('searchTerm', filters.searchTerm);
  if (filters.sortBy) queryParams.append('sortBy', filters.sortBy);

  const response = await fetch(`/api/Products/filter?${queryParams.toString()}`);
  if (!response.ok) {
    throw new Error('Failed to fetch products');
  }
  return response.json();
};

const deleteProductApi = async (productId: number): Promise<void> => {
  const token = localStorage.getItem('token');
  const response = await fetch(`/api/Products/${productId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`,
    }
  });
  if (!response.ok) {
    const errorData = await response.text();
    console.error('Помилка при видаленні товару:', errorData);
    throw new Error(`Не вдалося видалити товар: ${response.status} ${errorData || response.statusText}`);
  }
};

const ProductManagementPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [deleteSuccessMessage, setDeleteSuccessMessage] = useState<string | null>(null);
  const [deleteErrorMessage, setDeleteErrorMessage] = useState<string | null>(null);
  const [showConfirmDeleteDialog, setShowConfirmDeleteDialog] = useState<boolean>(false);
  const [productToDeleteId, setProductToDeleteId] = useState<number | null>(null);
  const [categoryFilter, setCategoryFilter] = useState<string>('');
  const [searchTermFilter, setSearchTermFilter] = useState<string>('');
  const [sortByFilter, setSortByFilter] = useState<string>('');
  const [currentUserRole, setCurrentUserRole] = useState<number | null>(null);
  const [basePath, setBasePath] = useState<string>('/admin');

  useEffect(() => {
    const userString = localStorage.getItem('user');
    if (userString) {
      try {
        const user: User = JSON.parse(userString);
        if (user.role === 0 || user.role === 1) {
          setCurrentUserRole(user.role);
          if (location.pathname.startsWith('/manager')) {
            setBasePath('/manager');
          } else {
            setBasePath('/admin');
          }
        } else {
          navigate('/');
        }
      } catch (e) {
        console.error("Не вдалося розпізнати користувача з localStorage", e);
        navigate('/login');
      }
    } else {
      navigate('/login');
    }
  }, [navigate, location.pathname]);

  const loadProducts = useCallback(async () => {
    setLoading(true);
    setError(null);
    setDeleteSuccessMessage(null);
    setDeleteErrorMessage(null);
    try {
      const data = await fetchProductsWithFilters({
        category: categoryFilter || undefined,
        searchTerm: searchTermFilter || undefined,
        sortBy: sortByFilter || undefined,
      });
      setProducts(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unknown error occurred');
      console.error('Error loading products:', err);
    } finally {
      setLoading(false);
    }
  }, [categoryFilter, searchTermFilter, sortByFilter]);

  useEffect(() => {
    if (currentUserRole !== null) {
      loadProducts();
    }
  }, [loadProducts, currentUserRole]);

  const handleFilterChange = () => {
    loadProducts();
  };

  const handleDeleteProduct = async (productId: number) => {
    setDeleteSuccessMessage(null);
    setDeleteErrorMessage(null);
    setProductToDeleteId(productId);
    setShowConfirmDeleteDialog(true);
  };

  const executeDeleteProduct = async () => {
    if (productToDeleteId === null) return;

    if (currentUserRole !== 0) {
      setDeleteErrorMessage("У вас немає прав для видалення товарів.");
      setShowConfirmDeleteDialog(false);
      setProductToDeleteId(null);
      return;
    }

    try {
      await deleteProductApi(productToDeleteId);
      setProducts(prevProducts => prevProducts.filter(p => p.id !== productToDeleteId));
      setDeleteSuccessMessage('Товар успішно видалено!');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Не вдалося видалити товар';
      setDeleteErrorMessage(`Помилка видалення: ${message}`);
      console.error('Error deleting product:', err);
    }
    setShowConfirmDeleteDialog(false);
    setProductToDeleteId(null);
  };

  const cancelDeleteProduct = () => {
    setShowConfirmDeleteDialog(false);
    setProductToDeleteId(null);
  };

  if (currentUserRole === null && loading) {
    return <div className="product-management-loading">Перевірка доступу та завантаження...</div>;
  }

  return (
    <div className="product-management-page">
      <div className="page-header">
        <h2>Управління товарами</h2>
        <Link to={`${basePath}/products/new`} className="add-product-button">Додати товар</Link>
      </div>

      <div className="filters-container">
        <h3>Фільтри</h3>
        <div className="filter-group">
          <label htmlFor="category-filter">Категорія:</label>
          <input
            type="text"
            id="category-filter"
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            placeholder="Наприклад: Men"
          />
        </div>
        <div className="filter-group">
          <label htmlFor="searchTerm-filter">Пошук:</label>
          <input
            type="text"
            id="searchTerm-filter"
            value={searchTermFilter}
            onChange={(e) => setSearchTermFilter(e.target.value)}
            placeholder="Наприклад: Men"
          />
        </div>
        <div className="filter-group">
          <label htmlFor="sortBy-filter">Сортувати за:</label>
          <select
            id="sortBy-filter"
            value={sortByFilter}
            onChange={(e) => setSortByFilter(e.target.value)}
          >
            <option value="">Не сортувати</option>
            <option value="name_asc">Назвою (А-Я)</option>
            <option value="name_desc">Назвою (Я-А)</option>
            <option value="price_asc">Ціною (зростання)</option>
            <option value="price_desc">Ціною (спадання)</option>
          </select>
        </div>
        <button onClick={handleFilterChange} className="apply-filters-button">
          Застосувати фільтри
        </button>
      </div>

      {deleteSuccessMessage && (
        <div className="product-management-message success">
          {deleteSuccessMessage}
        </div>
      )}
      {deleteErrorMessage && (
        <div className="product-management-message error">
          {deleteErrorMessage}
        </div>
      )}

      {showConfirmDeleteDialog && productToDeleteId !== null && (
        <div className="confirm-delete-dialog-overlay">
          <div className="confirm-delete-dialog">
            <p>Ви впевнені, що хочете видалити цей товар?</p>
            <div className="confirm-delete-dialog-actions">
              <button onClick={executeDeleteProduct} className="dialog-button confirm">
                Так, видалити
              </button>
              <button onClick={cancelDeleteProduct} className="dialog-button cancel">
                Скасувати
              </button>
            </div>
          </div>
        </div>
      )}

      {loading && <div className="product-management-loading">Завантаження товарів...</div>}
      
      {error && !loading && (
        <div className="product-management-error">
          Помилка: {error}. <button onClick={loadProducts}>Спробувати ще</button>
        </div>
      )}
      
      {!loading && !error && products.length === 0 && currentUserRole !== null && (
        <p>Товари не знайдено.</p>
      )}

      {!loading && !error && products.length > 0 && currentUserRole !== null && (
        <table className="products-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Назва</th>
              <th>Ціна</th>
              <th>Кількість на складі</th>
              <th>Дії</th>
            </tr>
          </thead>
          <tbody>
            {products.map(product => (
              <tr key={product.id}>
                <td>{product.id}</td>
                <td>{product.name}</td>
                <td>{product.price.toFixed(2)} грн</td>
                <td>{product.stock !== undefined ? product.stock : 'N/A'}</td>
                <td>
                  <Link to={`${basePath}/products/edit/${product.id}`} className="action-button edit-button">Редагувати</Link>
                  {currentUserRole === 0 && (
                    <button onClick={() => handleDeleteProduct(product.id)} className="action-button delete-button">Видалити</button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default ProductManagementPage; 