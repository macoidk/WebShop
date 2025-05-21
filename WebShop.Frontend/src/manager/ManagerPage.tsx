import React, { useEffect, useState } from 'react';
import { Link, useNavigate, Outlet } from 'react-router-dom';
import type { User } from '../types/interfaces';
import './ManagerPage.css';

const ManagerPage: React.FC = () => {
  const navigate = useNavigate();
  const [isManager, setIsManager] = useState<boolean | null>(null);

  useEffect(() => {
    const userString = localStorage.getItem('user');
    if (userString) {
      try {
        const user: User = JSON.parse(userString);
        if (user.role === 1) {
          setIsManager(true);
        } else {
          setIsManager(false);
          navigate('/');
        }
      } catch (error) {
        console.error("Не вдалося розпізнати користувача", error);
        setIsManager(false);
        navigate('/login');
      }
    } else {
      setIsManager(false);
      navigate('/login');
    }
  }, [navigate]);

  if (isManager === null) {
    return <div className="manager-page-loading">Перевірка прав доступу...</div>;
  }

  if (!isManager) {
    return <div className="manager-page-forbidden">Доступ заборонено.</div>;
  }

  return (
    <div className="manager-page">
      <header className="manager-page-header">
        <h1>Панель менеджера</h1>
      </header>
      <nav className="manager-page-nav">
        <ul>
          <li>
            <Link to="/manager/products">Управління товарами</Link>
          </li>
          <li>
            <Link to="/manager/orders">Замовлення</Link>
          </li>
        </ul>
      </nav>
      <main className="manager-page-content">
        <Outlet />
      </main>
    </div>
  );
};

export default ManagerPage; 