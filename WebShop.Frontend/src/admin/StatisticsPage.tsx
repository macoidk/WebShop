import React, { useEffect, useState } from 'react';
import type { ProductStatisticsDto } from '../types/interfaces';
import './StatisticsPage.css';

const StatisticsPage: React.FC = () => {
  const [statistics, setStatistics] = useState<ProductStatisticsDto | null>(null);
  const [productId, setProductId] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [userToken, setUserToken] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('token');

    if (token) {
      setUserToken(token);
    } else {
      setError("Помилка автентифікації: токен не знайдено. Будь ласка, увійдіть знову.");
    }
    
  }, []);

  const fetchStatistics = async () => {
    if (!productId) {
      setError('Будь ласка, введіть ID товару.');
      return;
    }
    if (!userToken) {
      setError("Помилка автентифікації. Неможливо отримати статистику.");
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`/api/statistics/product/${productId}`, {
        headers: {
          'Authorization': `Bearer ${userToken}`,
        },
      });
      if (!response.ok) {
        if (response.status === 401 || response.status === 403) {
          throw new Error('Помилка авторизації. Перевірте свої права доступу.');
        }
        if (response.status === 404) {
            throw new Error('Товар з таким ID не знайдено.');
        }
        throw new Error('Не вдалося завантажити статистику. Спробуйте пізніше.');
      }
      const data: ProductStatisticsDto = await response.json();
      setStatistics(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Сталася невідома помилка');
      setStatistics(null);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="statistics-page">
      <h2>Статистика по товару</h2>
      <div className="statistics-form">
        <input
          type="text"
          value={productId}
          onChange={(e) => setProductId(e.target.value)}
          placeholder="Введіть ID товару"
        />
        <button onClick={fetchStatistics} disabled={loading || !userToken}>
          {loading ? 'Завантаження...' : 'Отримати статистику'}
        </button>
      </div>

      {error && <p className="statistics-error">{error}</p>}

      {statistics && (
        <div className="statistics-results">
          <h3>Результати для товару ID: {statistics.productId}</h3>
          {statistics.productName && <p>Назва товару: {statistics.productName}</p>}
          <p>Продано одиниць: {statistics.unitsSold}</p>
          <p>Дохід: {statistics.revenue.toFixed(2)} грн</p>
        </div>
      )}
    </div>
  );
};

export default StatisticsPage; 