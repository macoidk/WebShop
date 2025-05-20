import React, { useEffect, useState, useCallback } from 'react';
import type { OrderDto, User as UserInterface } from '../types/interfaces';
import { OrderStatus, PaymentType } from '../types/enums';
import './OrderManagementPage.css';

const getOrderStatusString = (status: OrderStatus): string => {
  return OrderStatus[status];
};

const getPaymentTypeString = (paymentType?: PaymentType): string => {
  if (paymentType === undefined || paymentType === null) return 'N/A';
  return PaymentType[paymentType];
};

const OrderManagementPage: React.FC = () => {
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [filterStatus, setFilterStatus] = useState<OrderStatus | ''>('');
  const [filterUserId, setFilterUserId] = useState<string>('');
  const [currentUserIdInput, setCurrentUserIdInput] = useState<string>('');

  const fetchOrders = useCallback(async () => {
    setLoading(true);
    setError(null);
    let url = '/api/orders/all';

    if (filterUserId) {
      url = `/api/orders/user/${filterUserId}`;
    } else if (filterStatus !== '' && filterStatus !== null && filterStatus !== undefined) {
      const statusString = OrderStatus[filterStatus as OrderStatus];
      url = `/api/orders/status/${statusString}`;
    }

    try {
      const token = localStorage.getItem('token');
      if (!token) {
        throw new Error('Токен не знайдено.');
      }
      const response = await fetch(url, {
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      if (!response.ok) {
        if (filterUserId && response.status === 404) {
          setOrders([]);
          throw new Error(`Користувача з ID ${filterUserId} не знайдено, або для нього немає замовлень.`);
        } else if (filterStatus !== '' && response.status === 404) {
          setOrders([]);
          throw new Error(`Замовлень зі статусом ${OrderStatus[filterStatus as OrderStatus]} не знайдено.`);
        }
        throw new Error(`Не вдалося отримати замовлення: ${response.statusText} (URL: ${url})`);
      }
      const data: OrderDto[] = await response.json();
      
      const ordersWithUserDetails = await Promise.all(
        data.map(async (order) => {
          if (order.user || (order.firstName && order.email)) { 
            return order;
          }
          try {
            const userResponse = await fetch(`/api/users/${order.userId}`, { 
              headers: { 'Authorization': `Bearer ${token}` }
            });
            if (userResponse.ok) {
              const userData: UserInterface = await userResponse.json();
              return { ...order, user: userData };
            }
            console.warn(`Не вдалося отримати дані користувача для замовлення ${order.id},ID користувача: ${order.userId}: ${userResponse.statusText}`);
            return order; 
          } catch (userError) {
            console.error(`Не вдалося отримати дані користувача для замовлення ${order.id}:`, userError);
            return order; 
          }
        })
      );
      setOrders(ordersWithUserDetails);
    } catch (err) {
      if (!(err instanceof Error && orders.length === 0 && (filterUserId || filterStatus !== ''))) {
        setError(err instanceof Error ? err.message : 'Сталася невідома помилка при отриманні замовлень');
      } else if (err instanceof Error) {
        setError(err.message);
      }
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [filterStatus, filterUserId]);

  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  const handleFilterStatusChange = (newStatus: OrderStatus | '') => {
    setFilterStatus(newStatus);
    setFilterUserId('');
    setCurrentUserIdInput('');
  };

  const handleUserIdInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setCurrentUserIdInput(e.target.value);
  };

  const applyUserIdFilter = () => {
    const newUserId = currentUserIdInput.trim();
    if (newUserId && /^[0-9]+$/.test(newUserId)) {
      setFilterUserId(newUserId);
      setFilterStatus('');
    } else if (!newUserId) {
      setFilterUserId('');
    } else {
      setError("Будь ласка, введіть дійсний ID користувача.");
    }
  };

  const clearFilters = () => {
    setFilterStatus('');
    setFilterUserId('');
    setCurrentUserIdInput('');
  };

  const displayedOrders = orders;

  const handleStatusChange = async (orderId: number, newStatus: OrderStatus) => {
    try {
      const token = localStorage.getItem('token');
      if (!token) throw new Error('Токен не знайдено');
      const response = await fetch(`/api/orders/${orderId}/status`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(OrderStatus[newStatus]),
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(`Не вдалося оновити статус замовлення: ${response.statusText} - ${errorData}`);
      }
      fetchOrders();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не вдалося оновити статус');
      console.error(err);
    }
  };

  const handlePaymentTypeChange = async (orderId: number, newPaymentType: PaymentType) => {
    try {
      const token = localStorage.getItem('token');
      if (!token) throw new Error('Токен не знайдено');
      const response = await fetch(`/api/orders/${orderId}/payment-type`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(PaymentType[newPaymentType]), 
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(`Не вдалося оновити тип оплати: ${response.statusText} - ${errorData}`);
      }
      fetchOrders();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Не вдалося оновити тип оплати');
      console.error(err);
    }
  };

  if (loading) return <p className="loading-message">Завантаження замовлень...</p>;

  return (
    <div className="order-management-page">
      <h2>Управління замовленнями</h2>

      {error && !loading && <p className="error-message">Помилка: {error}</p>}

      <div className="filters-and-sort">
        <select 
          value={filterStatus} 
          onChange={(e) => handleFilterStatusChange(e.target.value === '' ? '' : parseInt(e.target.value) as OrderStatus)}
          className="filter-select"
        >
          <option value="">Всі статуси</option>
          {Object.keys(OrderStatus)
            .filter(key => !isNaN(Number(OrderStatus[key as keyof typeof OrderStatus])))
            .map(key => (
              <option key={OrderStatus[key as keyof typeof OrderStatus]} value={OrderStatus[key as keyof typeof OrderStatus]}>
                {key}
              </option>
          ))}
        </select>

        <input
          type="text"
          placeholder="ID Користувача..."
          value={currentUserIdInput}
          onChange={handleUserIdInputChange}
          className="user-id-input"
        />
        <button onClick={applyUserIdFilter} className="filter-button">Фільтрувати за ID</button>
        
        {(filterStatus !== '' || filterUserId !== '') && (
          <button onClick={clearFilters} className="clear-filter-button">Скинути фільтри</button>
        )}
      </div>

      {!loading && !error && displayedOrders.length === 0 && (
        <p>Немає доступних замовлень за вашими критеріями.</p>
      )}
      {!loading && displayedOrders.length > 0 && (
        <table className="orders-table">
          <thead>
            <tr>
              <th>ID Замовлення</th>
              <th>Дата</th>
              <th>Замовник</th>
              <th>Контактні дані</th>
              <th>Адреса доставки</th>
              <th>Товари</th>
              <th>Сума</th>
              <th>Тип оплати</th>
              <th>Статус</th>
              <th>Дії</th>
            </tr>
          </thead>
          <tbody>
            {displayedOrders.map((order) => (
              <tr key={order.id}>
                <td>{order.id}</td>
                <td>{new Date(order.orderDate).toLocaleString('uk-UA')}</td>
                <td className="customer-details">
                  <p>{order.user?.firstName || order.firstName || 'N/A'} {order.user?.lastName || order.lastName || ''}</p>
                </td>
                <td className="customer-details">
                  <p>Email: {order.user?.email || order.email || 'N/A'}</p>
                  <p>Телефон: {order.user?.phone || order.phone || 'N/A'}</p>
                </td>
                <td>{order.user?.address || order.deliveryAddress || 'N/A'}</td>
                <td>
                  <ul className="order-items-list">
                    {order.orderItems.map((item) => (
                      <li key={item.id}>
                        {item.productName || `ID товару: ${item.productId}`} - {item.quantity} шт. x {item.unitPrice.toFixed(2)} грн
                      </li>
                    ))}
                  </ul>
                </td>
                <td>{order.totalAmount.toFixed(2)} грн</td>
                <td>{getPaymentTypeString(order.paymentType)}</td>
                <td>{getOrderStatusString(order.status)}</td>
                <td className="actions-cell">
                  <div>
                    <select
                      className="status-select"
                      value={order.status}
                      onChange={(e) => handleStatusChange(order.id, parseInt(e.target.value) as OrderStatus)}
                    >
                      {Object.keys(OrderStatus)
                        .filter(key => !isNaN(Number(OrderStatus[key as keyof typeof OrderStatus])))
                        .map((key) => (
                          <option key={OrderStatus[key as keyof typeof OrderStatus]} value={OrderStatus[key as keyof typeof OrderStatus]}>
                            {key}
                          </option>
                      ))}
                    </select>
                  </div>
                  <div style={{ marginTop: '5px' }}>
                    <select
                      className="status-select"
                      value={order.paymentType !== undefined && order.paymentType !== null ? order.paymentType : ''}
                      onChange={(e) => handlePaymentTypeChange(order.id, parseInt(e.target.value) as PaymentType)}
                    >
                      <option value="" disabled>Змінити тип оплати</option>
                      {Object.keys(PaymentType)
                        .filter(key => !isNaN(Number(PaymentType[key as keyof typeof PaymentType])))
                        .map((key) => (
                          <option key={PaymentType[key as keyof typeof PaymentType]} value={PaymentType[key as keyof typeof PaymentType]}>
                            {key}
                          </option>
                      ))}
                    </select>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default OrderManagementPage; 