import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import './ProfilePage.css'; 
import '../App.css'; 
import { OrderStatus, PaymentType } from '../types/enums';
import type { User, UserUpdateRequest, ProfileOrderDto } from '../types/interfaces';

const orderStatusToString = (status: OrderStatus): string => {
  switch (status) {
    case OrderStatus.Pending: return 'В обробці';
    case OrderStatus.Processed: return 'Оброблене';
    case OrderStatus.Completed: return 'Завершене';
    case OrderStatus.Cancelled: return 'Скасоване';
    default: return 'Невідомий статус';
  }
};

const ProfilePage: React.FC = () => {
  const navigate = useNavigate();
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [formData, setFormData] = useState<Partial<UserUpdateRequest>>({});
  const [orders, setOrders] = useState<ProfileOrderDto[]>([]);
  const [loadingProfile, setLoadingProfile] = useState<boolean>(true);
  const [loadingOrders, setLoadingOrders] = useState<boolean>(true);
  const [updateError, setUpdateError] = useState<string | null>(null);
  const [updateSuccess, setUpdateSuccess] = useState<string | null>(null);
  const [ordersError, setOrdersError] = useState<string | null>(null);

  const fetchUserProfile = useCallback(() => {
    const storedUser = localStorage.getItem('user');
    const token = localStorage.getItem('token');

    if (storedUser && token) {
      const parsedUser: User = JSON.parse(storedUser);
      setCurrentUser(parsedUser);
      setFormData({
        id: parsedUser.id,
        username: parsedUser.username,
        email: parsedUser.email,
        firstName: parsedUser.firstName || '',
        lastName: parsedUser.lastName || '',
        address: parsedUser.address || '',
        phone: parsedUser.phone || ''
      });
      setLoadingProfile(false);
    } else {
      navigate('/login'); 
    }
  }, [navigate]);

  const fetchUserOrders = useCallback(async () => {
    const storedUser = localStorage.getItem('user');
    const token = localStorage.getItem('token');
    if (!storedUser || !token) {
      setLoadingOrders(false);
      return;
    }
    const parsedUser: User = JSON.parse(storedUser);

    try {
      setLoadingOrders(true);
      setOrdersError(null);
      const response = await fetch(`/api/Orders/user/${parsedUser.id}`, {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      if (!response.ok) {
        throw new Error(`Помилка завантаження замовлень: ${response.status}`);
      }
      const data: ProfileOrderDto[] = await response.json();
      data.sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime());
      setOrders(data);
    } catch (err) {
      console.error('Помилка при завантаженні замовлень:', err);
      setOrdersError(err instanceof Error ? err.message : 'Не вдалося завантажити історію замовлень.');
    } finally {
      setLoadingOrders(false);
    }
  }, []);


  useEffect(() => {
    fetchUserProfile();
    fetchUserOrders();
  }, [fetchUserProfile, fetchUserOrders]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!currentUser || !formData.id) return;

    setUpdateError(null);
    setUpdateSuccess(null);
    setLoadingProfile(true);

    const token = localStorage.getItem('token');
    const payload: UserUpdateRequest = {
        id: formData.id!,
        username: formData.username || currentUser.username, 
        email: formData.email || currentUser.email,       
        firstName: formData.firstName || '',
        lastName: formData.lastName || '',
        address: formData.address || '',
        phone: formData.phone || '',
        password: "",
    };


    try {
      const response = await fetch(`/api/Users/${currentUser.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
            const errorData = await response.json();
            if (errorData && errorData.errors) { 
                errorMessage = Object.values(errorData.errors).flat().join(' ');
            } else if (errorData && (errorData.message || errorData.title)) {
                errorMessage = errorData.message || errorData.title;
            } else if (typeof errorData === 'string') {
                 errorMessage = errorData;
            }
        } catch (jsonError) {
            console.warn('Помилка при парсингу JSON з відповіді на оновлення профілю', jsonError);
        }
        throw new Error(errorMessage);
      }
      
      setUpdateSuccess('Профіль успішно оновлено!');
      const updatedUser: User = { ...currentUser, ...formData };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      setCurrentUser(updatedUser); 

      setTimeout(() => setUpdateSuccess(null), 3000);

    } catch (err) {
      if (err instanceof Error) {
        setUpdateError(err.message);
      } else {
        setUpdateError('Невідома помилка при оновленні профілю.');
      }
       setTimeout(() => setUpdateError(null), 5000);
    } finally {
      setLoadingProfile(false);
    }
  };

  if (!currentUser) {
    return <div className="profile-page">Завантаження профілю...</div>;
  }

  return (
    <div className="profile-page">
      <h2>Мій Профіль</h2>

      <div className="profile-section">
        <h3>Інформація про користувача</h3>
        <form onSubmit={handleSubmit}>
          <div>
            <label htmlFor="username">Ім'я користувача:</label>
            <input type="text" id="username" name="username" value={formData.username || ''} onChange={handleChange} />
          </div>
          <div>
            <label htmlFor="email">Email:</label>
            <input type="email" id="email" name="email" value={formData.email || ''} onChange={handleChange} />
          </div>
          <div>
            <label htmlFor="firstName">Ім'я:</label>
            <input type="text" id="firstName" name="firstName" value={formData.firstName || ''} onChange={handleChange} />
          </div>
          <div>
            <label htmlFor="lastName">Прізвище:</label>
            <input type="text" id="lastName" name="lastName" value={formData.lastName || ''} onChange={handleChange} />
          </div>
          <div>
            <label htmlFor="address">Адреса:</label>
            <input type="text" id="address" name="address" value={formData.address || ''} onChange={handleChange} />
          </div>
          <div>
            <label htmlFor="phone">Телефон:</label>
            <input type="tel" id="phone" name="phone" value={formData.phone || ''} onChange={handleChange} />
          </div>
          {updateError && <p className="error-message">{updateError}</p>}
          {updateSuccess && <p className="success-message">{updateSuccess}</p>}
          <button type="submit" disabled={loadingProfile}>
            {loadingProfile ? 'Оновлення...' : 'Оновити профіль'}
          </button>
        </form>
      </div>

      <div className="profile-section">
        <h3>Історія Замовлень</h3>
        {loadingOrders && <p className="loading-orders">Завантаження історії замовлень...</p>}
        {ordersError && <p className="error-message">{ordersError}</p>}
        {!loadingOrders && !ordersError && orders.length === 0 && (
          <p className="no-orders">У вас ще немає замовлень.</p>
        )}
        {!loadingOrders && !ordersError && orders.length > 0 && (
          <ul className="order-history-list">
            {orders.map(order => (
              <li key={order.id} className="order-item">
                <h4>Замовлення #{order.id} - <span style={{fontSize: '0.8em', color: '#ccc'}}>{new Date(order.orderDate).toLocaleDateString('uk-UA')} {new Date(order.orderDate).toLocaleTimeString('uk-UA')}</span></h4>
                <p><strong>Статус:</strong> {orderStatusToString(order.status)}</p>
                <p><strong>Загальна сума:</strong> {order.totalAmount.toFixed(2)} грн</p>
                {order.deliveryAddress && <p><strong>Адреса доставки:</strong> {order.deliveryAddress}</p>}
                
                {order.status === OrderStatus.Pending && order.paymentType === PaymentType.BankCard && order.paymentDeeplink && (
                  <button 
                    onClick={() => window.open(order.paymentDeeplink, '_blank')}
                    className="monobank-pay-button"
                  >
                    Оплатити Monobank
                  </button>
                )}

                {order.orderItems && order.orderItems.length > 0 && (
                  <div className="order-products">
                    <h5>Товари:</h5>
                    <ul>
                      {order.orderItems.map(item => (
                        <li key={item.id}>
                          ID Товару: {item.productId}, Кількість: {item.quantity}, Ціна за од.: {item.unitPrice.toFixed(2)} грн
                        </li>
                      ))}
                    </ul>
                  </div>
                )}
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
};

export default ProfilePage; 