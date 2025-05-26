import { useState, useEffect } from 'react';
import './App.css';
import { Routes, Route, Link } from 'react-router-dom';
import HomePage from './pages/HomePage';
import ProductsPage from './pages/ProductsPage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import CatalogModal from './components/CatalogModal';
import ProductDetailPage from './pages/ProductDetailPage';
import ProfilePage from './pages/ProfilePage';
import { useNavigate } from 'react-router-dom';
import AdminPage from './admin/AdminPage';
import ProductManagementPage from './admin/ProductManagementPage';
import ProductForm from './admin/components/ProductForm';
import OrderManagementPage from './admin/OrderManagementPage';
import AdminDashboardHomePage from './admin/AdminDashboardHomePage';
import StatisticsPage from './admin/StatisticsPage';
import ManagerPage from './manager/ManagerPage';
import type { User } from './types/interfaces';

function App() {
  const [isCatalogOpen, setIsCatalogOpen] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isManager, setIsManager] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    const userString = localStorage.getItem('user');
    setIsAuthenticated(!!token);
    if (userString) {
      try {
        const user: User = JSON.parse(userString);
        setIsAdmin(user.role === 0);
        setIsManager(user.role === 1);
      } catch (e) {
        setIsAdmin(false);
        setIsManager(false);
      }
    } else {
      setIsAdmin(false);
      setIsManager(false);
    }
  }, []);

  useEffect(() => {
    const handleStorageChange = () => {
      const token = localStorage.getItem('token');
      const userString = localStorage.getItem('user');
      setIsAuthenticated(!!token);
      if (userString) {
        try {
          const user: User = JSON.parse(userString);
          setIsAdmin(user.role === 0);
          setIsManager(user.role === 1);
        } catch (e) {
          setIsAdmin(false);
          setIsManager(false);
        }
      } else {
        setIsAdmin(false);
        setIsManager(false);
      }
    };

    window.addEventListener('storage', handleStorageChange);
    handleStorageChange();

    return () => {
      window.removeEventListener('storage', handleStorageChange);
    };
  }, [navigate]);

  const toggleCatalog = () => {
    setIsCatalogOpen(!isCatalogOpen);
  };

  const handleLogout = () => {
    // Повністю очищаємо localStorage при виході з системи
    // Це гарантує, що не залишиться ніяких даних попереднього користувача
    localStorage.clear();
    
    // Скидаємо стан автентифікації та ролі
    setIsAuthenticated(false);
    setIsAdmin(false);
    setIsManager(false);
    
    // Перенаправляємо користувача на сторінку входу
    navigate('/login');
  };

  return (
    <div className="app">
      <header className="app-header">
        <div className="logo"><Link to="/">WebShop</Link></div>
        <nav className="navigation">
          <button onClick={toggleCatalog} className="nav-button">Каталог</button>
          <Link to="/products/women">Жінкам</Link>
          <Link to="/products/men">Чоловікам</Link>
          <Link to="/products/kids">Дітям</Link>
          <Link to="/products?category=accessories">Аксесуари</Link>
          <Link to="/products">Усі товари</Link>
          {isAdmin && <Link to="/admin" style={{ marginLeft: '15px', fontWeight: 'bold' }}>Адмін</Link>}
          {isManager && <Link to="/manager" style={{ marginLeft: '15px', fontWeight: 'bold' }}>Менеджер</Link>}
        </nav>
        <div className="actions">
          {isAuthenticated ? (
            <>
              <Link to="/profile">Профіль</Link>
              <span onClick={handleLogout} style={{ cursor: 'pointer', marginLeft: '15px' }}>Вийти</span>
            </>
          ) : (
            <Link to="/login">Акаунт</Link>
          )}
        </div>
      </header>

      <main className="app-main">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/products/:category" element={<ProductsPage />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/product/:productId" element={<ProductDetailPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/admin" element={<AdminPage />}>
            <Route index element={<AdminDashboardHomePage />} />
            <Route path="products" element={<ProductManagementPage />} />
            <Route path="products/new" element={<ProductForm />} />
            <Route path="products/edit/:productId" element={<ProductForm />} />
            <Route path="orders" element={<OrderManagementPage />} />
            <Route path="statistics" element={<StatisticsPage />} />
          </Route>
          <Route path="/manager" element={<ManagerPage />}>
            <Route index element={<AdminDashboardHomePage />} />
            <Route path="products" element={<ProductManagementPage />} />
            <Route path="products/new" element={<ProductForm />} />
            <Route path="products/edit/:productId" element={<ProductForm />} />
            <Route path="orders" element={<OrderManagementPage />} />
          </Route>
        </Routes>
      </main>


      <CatalogModal isOpen={isCatalogOpen} onClose={toggleCatalog} />
    </div>
  );
}

export default App;
