import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import '../App.css'; 
import type { UserRegistrationRequest } from '../types/interfaces';

const RegisterPage: React.FC = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState<UserRegistrationRequest>({
    username: '',
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    address: '',
    phone: ''
  });
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState<boolean>(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    if (!formData.username || !formData.email || !formData.password) {
      setError("Ім'я користувача, email та пароль є обов'язковими.");
      setLoading(false);
      return;
    }

    const payload: Omit<UserRegistrationRequest, 'role'> = {
        username: formData.username,
        email: formData.email,
        password: formData.password,
        firstName: formData.firstName || '',
        lastName: formData.lastName || '',
        address: formData.address,
        phone: formData.phone
    };

    try {
      const response = await fetch('/api/Users/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
            const errorData = await response.json();
            if (errorData && errorData.errors) {
                const validationErrors = Object.values(errorData.errors).flat().join(' ');
                errorMessage = validationErrors || errorData.title || errorMessage;
            } else if (errorData && errorData.message) {
                errorMessage = errorData.message;
            } else if (typeof errorData === 'string') {
                errorMessage = errorData;
            }
        } catch (jsonError) {
            console.warn('Не вдалося отримати повідомлення про помилку:', jsonError);
        }
        throw new Error(errorMessage);
      }

      setSuccessMessage('Реєстрація успішна!');
      setTimeout(() => {
        navigate('/login'); 
      }, 3000);

    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Невідома помилка під час реєстрації.');
      }
      console.error('Помилка при реєстрації:', err);
      setLoading(false);
    }
  };

  return (
    <div className="auth-page"> 
      <h2>Реєстрація</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="username">Ім'я користувача:</label>
          <input type="text" id="username" name="username" value={formData.username} onChange={handleChange} required />
        </div>
        <div>
          <label htmlFor="email">Email:</label>
          <input type="email" id="email" name="email" value={formData.email} onChange={handleChange} required />
        </div>
        <div>
          <label htmlFor="password">Пароль:</label>
          <input type="password" id="password" name="password" value={formData.password} onChange={handleChange} required />
        </div>
        <div>
          <label htmlFor="firstName">Ім'я (необов'язково):</label>
          <input type="text" id="firstName" name="firstName" value={formData.firstName} onChange={handleChange} />
        </div>
        <div>
          <label htmlFor="lastName">Прізвище (необов'язково):</label>
          <input type="text" id="lastName" name="lastName" value={formData.lastName} onChange={handleChange} />
        </div>
        {error && <p className="error-message">{error}</p>}
        {successMessage && <p className="success-message">{successMessage}</p>}
        <button type="submit" disabled={loading || !!successMessage}>
          {loading ? 'Реєстрація...' : 'Зареєструватися'}
        </button>
      </form>
      <p>Вже маєте акаунт? <Link to="/login">Увійти</Link></p>
    </div>
  );
};

export default RegisterPage; 