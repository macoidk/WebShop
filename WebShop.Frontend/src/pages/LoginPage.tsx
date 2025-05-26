import React, { useState /*, useContext */ } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import '../App.css'; 
import type { LoginModel, LoginResponse } from '../types/interfaces';

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState<LoginModel>({
    username: '',
    password: '',
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

    if (!formData.username || !formData.password) {
      setError("Ім'я користувача та пароль є обов'язковими.");
      setLoading(false);
      return;
    }

    try {
      const response = await fetch('/api/Users/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: "Неправильне ім'я користувача або пароль." }));
        throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
      }

      const loginResponse = await response.json() as LoginResponse;
      localStorage.clear();
      
      localStorage.setItem('token', loginResponse.token);
      
      const userToStore = {
        ...loginResponse.user,
        role: typeof loginResponse.user.role === 'number' ? loginResponse.user.role : parseInt(loginResponse.user.role as any, 10) || 2
      };
      
      localStorage.setItem('user', JSON.stringify(userToStore));

      setSuccessMessage('Вхід успішний!');
      setTimeout(() => {
        navigate('/'); 
      }, 2000);

    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Невідома помилка під час входу.');
      }
      console.error('Login failed:', err);
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <h2>Вхід</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label htmlFor="username">Ім'я користувача:</label>
          <input type="text" id="username" name="username" value={formData.username} onChange={handleChange} required />
        </div>
        <div>
          <label htmlFor="password">Пароль:</label>
          <input type="password" id="password" name="password" value={formData.password} onChange={handleChange} required />
        </div>
        {error && <p className="error-message">{error}</p>}
        {successMessage && <p className="success-message">{successMessage}</p>}
        <button type="submit" disabled={loading || !!successMessage}>
          {loading ? 'Вхід...' : 'Увійти'}
        </button>
      </form>
      <p>Немає акаунта? <Link to="/register">Зареєструватися</Link></p>
    </div>
  );
};

export default LoginPage; 