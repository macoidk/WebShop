import React, { useEffect, useState } from 'react';
import { Link, useNavigate, Outlet } from 'react-router-dom';
import type { User } from '../types/interfaces';
import './AdminPage.css';

const AdminPage: React.FC = () => {
  const navigate = useNavigate();
  const [isAdmin, setIsAdmin] = useState<boolean | null>(null);

  useEffect(() => {
    const userString = localStorage.getItem('user');
    if (userString) {
      try {
        const user: User = JSON.parse(userString);
        if (user.role === 0) {
          setIsAdmin(true);
        } else {
          setIsAdmin(false);
          navigate('/');
        }
      } catch (error) {
        console.error("Failed to parse user data from localStorage", error);
        setIsAdmin(false);
        navigate('/login');
      }
    } else {
      setIsAdmin(false);
      navigate('/login');
    }
  }, [navigate]);

  if (isAdmin === null) {
    return <div className="admin-page-loading">Перевірка прав доступу...</div>;
  }

  if (!isAdmin) {
    return <div className="admin-page-forbidden">Доступ заборонено.</div>;
  }

  return (
    <div className="admin-page">
      <header className="admin-page-header">
        <h1>Адміністративна панель</h1>
      </header>
      <nav className="admin-page-nav">
        <ul>
          <li>
            <Link to="/admin/products">Управління товарами</Link>
          </li>
          <li>
            <Link to="/admin/orders">Замовлення</Link>
          </li>
          <li>
            <Link to="/admin/statistics">Статистика</Link>
          </li>
        </ul>
      </nav>
      <main className="admin-page-content">
        <Outlet />
      </main>
    </div>
  );
};

export default AdminPage; 