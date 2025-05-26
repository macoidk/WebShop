import React, { useEffect, useState, useCallback } from 'react';
import type { OrderDto, User as UserInterface, Product } from '../types/interfaces';
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
  const [sortOrder, setSortOrder] = useState<'newest' | 'oldest'>('newest');
  const [updatingOrderId, setUpdatingOrderId] = useState<number | null>(null);

  const fetchOrders = useCallback(async () => {
    setLoading(true);
    setError(null);
    let url = '/api/Orders/all';

    if (filterUserId) {
      url = `/api/Orders/user/${filterUserId}`;
    } else if (filterStatus !== '' && filterStatus !== null && filterStatus !== undefined) {
      const statusString = OrderStatus[filterStatus as OrderStatus];
      url = `/api/Orders/status/${statusString}`;
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
            const userResponse = await fetch(`/api/Users/${order.userId}`, { 
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
      const ordersWithProductDetails = await Promise.all(
        ordersWithUserDetails.map(async (order) => {
          const orderItemsWithDetails = await Promise.all(
            order.orderItems.map(async (item) => {
              if (item.productName) {
                return item;
              }
              try {
                const productResponse = await fetch(`/api/Products/${item.productId}`, {
                  headers: { 'Authorization': `Bearer ${token}` }
                });
                if (productResponse.ok) {
                  const productData: Product = await productResponse.json();
                  return { ...item, productName: productData.name };
                }
                return item;
              } catch (productError) {
                console.error(`Не вдалося отримати дані товару для замовлення ${order.id}, товар ${item.productId}:`, productError);
                return item;
              }
            })
          );
          return { ...order, orderItems: orderItemsWithDetails };
        })
      );
      
      setOrders(ordersWithProductDetails);
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
  
  const handleSortOrderChange = (newSortOrder: 'newest' | 'oldest') => {
    setSortOrder(newSortOrder);
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
    setSortOrder('newest');
  };

  const displayedOrders = [...orders].sort((a, b) => {
    const dateA = new Date(a.orderDate).getTime();
    const dateB = new Date(b.orderDate).getTime();
    return sortOrder === 'newest' ? dateB - dateA : dateA - dateB;
  });

  const handleStatusChange = async (orderId: number, newStatus: OrderStatus) => {
    try {
      setUpdatingOrderId(orderId);
      const token = localStorage.getItem('token');
      if (!token) {
        throw new Error('Токен не знайдено.');
      }
      const statusString = OrderStatus[newStatus];
      const response = await fetch(`/api/Orders/${orderId}/status`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(statusString),
      });
      if (!response.ok) {
        throw new Error(`Не вдалося оновити статус замовлення: ${response.statusText}`);
      }
      setOrders(prevOrders => 
        prevOrders.map(order => 
          order.id === orderId ? { ...order, status: newStatus } : order
        )
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Сталася невідома помилка при оновленні статусу замовлення');
      console.error(err);
    } finally {
      setUpdatingOrderId(null);
    }
  };

  const handlePaymentTypeChange = async (orderId: number, newPaymentType: PaymentType) => {
    try {
      setUpdatingOrderId(orderId);
      const token = localStorage.getItem('token');
      if (!token) {
        throw new Error('Токен не знайдено.');
      }
      const paymentTypeString = PaymentType[newPaymentType];
      const response = await fetch(`/api/Orders/${orderId}/payment-type`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(paymentTypeString),
      });
      if (!response.ok) {
        throw new Error(`Не вдалося оновити тип оплати замовлення: ${response.statusText}`);
      }
      setOrders(prevOrders => 
        prevOrders.map(order => 
          order.id === orderId ? { ...order, paymentType: newPaymentType } : order
        )
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Сталася невідома помилка при оновленні типу оплати замовлення');
      console.error(err);
    } finally {
      setUpdatingOrderId(null);
    }
  };

  if (loading) return <p className="loading-message">Завантаження замовлень...</p>;

  return (
    <div className="order-management-page">
      <h2>Управління замовленнями</h2>

      {error && !loading && <p className="error-message">Помилка: {error}</p>}

      <div className="filters-container">
        <div className="filter-group">
          <label>Фільтр за статусом:</label>
          <select
            value={filterStatus}
            onChange={(e) => handleFilterStatusChange(e.target.value === '' ? '' : parseInt(e.target.value) as OrderStatus)}
          >
            <option value="">Всі статуси</option>
            {Object.keys(OrderStatus)
              .filter(key => !isNaN(Number(OrderStatus[key as keyof typeof OrderStatus])))
              .map((key) => (
                <option key={OrderStatus[key as keyof typeof OrderStatus]} value={OrderStatus[key as keyof typeof OrderStatus]}>
                  {key}
                </option>
            ))}
          </select>
        </div>
        <div className="filter-group">
          <label>Фільтр за ID користувача:</label>
          <input
            type="text"
            value={currentUserIdInput}
            onChange={handleUserIdInputChange}
            placeholder="Введіть ID користувача"
          />
          <button onClick={applyUserIdFilter}>Застосувати</button>
        </div>
        <div className="filter-group">
          <label>Сортування за датою:</label>
          <select
            value={sortOrder}
            onChange={(e) => handleSortOrderChange(e.target.value as 'newest' | 'oldest')}
          >
            <option value="newest">Найновіші спочатку</option>
            <option value="oldest">Найстаріші спочатку</option>
          </select>
        </div>
        <button onClick={clearFilters} className="clear-filters-button">Очистити фільтри</button>
      </div>

      {!loading && !error && displayedOrders.length === 0 && (
        <p>Немає доступних замовлень за вашими критеріями.</p>
      )}
      {!loading && displayedOrders.length > 0 && (
        <table className="orders-table">
          <thead>
            <tr>
              <th>ID Замовлення / ID Користувача</th>
              <th>Дата</th>
              <th>Замовник</th>
              <th>Контактні дані</th>
              <th>Адреса доставки</th>

              <th>Сума</th>
              <th>Тип оплати</th>
              <th>Статус</th>
              <th>Дії</th>
            </tr>
          </thead>
          <tbody>
            {displayedOrders.map((order) => (
              <tr key={order.id}>
                <td>{order.id} / {order.userId}</td>
                <td>{new Date(order.orderDate).toLocaleString('uk-UA')}</td>
                <td className="customer-details">
                  <p>{order.user?.firstName || order.firstName || 'N/A'} {order.user?.lastName || order.lastName || ''}</p>
                </td>
                <td className="customer-details">
                  <p>Email: {order.user?.email || order.email || 'N/A'}</p>
                  <p>Телефон: {order.user?.phone || order.phone || 'N/A'}</p>
                </td>
                <td>{order.user?.address || order.deliveryAddress || 'N/A'}</td>

                <td>{order.totalAmount.toFixed(2)} грн</td>
                <td>{getPaymentTypeString(order.paymentType)}</td>
                <td>{getOrderStatusString(order.status)}</td>
                <td className="actions-cell">
                  <div>
                    <select
                      className="status-select"
                      value={order.status}
                      onChange={(e) => handleStatusChange(order.id, parseInt(e.target.value) as OrderStatus)}
                      disabled={updatingOrderId === order.id}
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