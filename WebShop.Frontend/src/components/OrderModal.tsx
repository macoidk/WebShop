import React, { useState, useEffect } from 'react';
import './OrderModal.css';
import { PaymentType, DeliveryType } from '../types/enums';

interface OrderModalProps {
  isOpen: boolean;
  onClose: () => void;
  product: {
    id: number;
    name: string;
    price: number;
  };
  userProfile: {
    firstName?: string;
    lastName?: string;
    address?: string;
    phone?: string;
  };
  onSubmit: (orderData: {
    paymentType: PaymentType;
    deliveryType: DeliveryType;
    deliveryAddress: string;
    firstName: string;
    lastName: string;
    phone: string;
  }) => Promise<void>;
  orderStatusMessage?: string | null;
  onStatusMessageChange?: (msg: string | null) => void;
}

const OrderModal: React.FC<OrderModalProps> = ({ isOpen, onClose, product, userProfile, onSubmit, orderStatusMessage }) => {
  const [paymentType, setPaymentType] = useState<PaymentType>(PaymentType.BankCard);
  const [deliveryType, setDeliveryType] = useState<DeliveryType>(DeliveryType.PostOffice);
  const [deliveryAddress, setDeliveryAddress] = useState<string>(userProfile.address || '');
  const [address, setAddress] = useState<string>(userProfile.address || '');
  const [firstName, setFirstName] = useState<string>(userProfile.firstName || '');
  const [lastName, setLastName] = useState<string>(userProfile.lastName || '');
  const [phone, setPhone] = useState<string>(userProfile.phone || '');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setDeliveryAddress(userProfile.address || '');
      setAddress(userProfile.address || '');
      setFirstName(userProfile.firstName || '');
      setLastName(userProfile.lastName || '');
      setPhone(userProfile.phone || '');
    }
  }, [isOpen, userProfile]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!firstName || !lastName || !phone || !address) {
      setError('Будь ласка, заповніть всі обов\'язкові поля');
      return;
    }
    
    if (!/^[0-9]+$/.test(phone)) {
      setError('Номер телефону повинен містити тільки цифри');
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit({
        paymentType,
        deliveryType,
        deliveryAddress: address,
        firstName,
        lastName,
        phone,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Помилка при створенні замовлення');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content order-modal" onClick={e => e.stopPropagation()}>
        <button className="modal-close-button" onClick={onClose}>×</button>
        <h2>Оформлення замовлення</h2>
        
        <div className="order-product-info">
          <h3>Товар</h3>
          <p>{product.name}</p>
          <p className="product-price">{product.price.toFixed(2)} грн</p>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-section">
            <h3>Контактна інформація</h3>
            <div className="form-group">
              <label htmlFor="firstName">Ім'я *</label>
              <input
                type="text"
                id="firstName"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="lastName">Прізвище *</label>
              <input
                type="text"
                id="lastName"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="phone">Телефон *</label>
              <input
                type="tel"
                id="phone"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="address">Адреса *</label>
              <input
                type="text"
                id="address"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                required
                placeholder="Введіть адресу доставки або самовивозу"
              />
            </div>
          </div>

          <div className="form-section">
            <h3>Спосіб доставки</h3>
            <div className="radio-group">
              <label>
                <input
                  type="radio"
                  checked={deliveryType === DeliveryType.Pickup}
                  onChange={() => setDeliveryType(DeliveryType.Pickup)}
                />
                Самовивіз
              </label>
              <label>
                <input
                  type="radio"
                  checked={deliveryType === DeliveryType.PostOffice}
                  onChange={() => setDeliveryType(DeliveryType.PostOffice)}
                />
                Нова Пошта
              </label>
            </div>
            {deliveryType === DeliveryType.PostOffice && (
              <div className="form-group">
                <label htmlFor="deliveryAddress">Адреса доставки *</label>
                <input
                  type="text"
                  id="deliveryAddress"
                  value={deliveryAddress}
                  onChange={(e) => setDeliveryAddress(e.target.value)}
                  required
                  placeholder="Введіть адресу відділення Нової Пошти"
                />
              </div>
            )}
          </div>

          <div className="form-section">
            <h3>Спосіб оплати</h3>
            <div className="radio-group">
              <label>
                <input
                  type="radio"
                  checked={paymentType === PaymentType.CashOnPickup}
                  onChange={() => setPaymentType(PaymentType.CashOnPickup)}
                />
                Готівкою при отриманні
              </label>
              <label>
                <input
                  type="radio"
                  checked={paymentType === PaymentType.CashOnDelivery}
                  onChange={() => setPaymentType(PaymentType.CashOnDelivery)}
                />
                Готівкою при доставці
              </label>
              <label>
                <input
                  type="radio"
                  checked={paymentType === PaymentType.BankCard}
                  onChange={() => setPaymentType(PaymentType.BankCard)}
                />
                Оплата картою
              </label>
            </div>
          </div>

          {error && <div className="error-message">{error}</div>}
          
          <button 
            type="submit" 
            className="submit-order-button"
            disabled={isSubmitting || !firstName || !lastName || !phone || !address}
          >
            {isSubmitting ? 'Обробка...' : (paymentType === PaymentType.BankCard ? 'Оформити та перейти до оплати' : 'Оформити замовлення')}
          </button>
          {orderStatusMessage && (
            <div className="order-status-message">{orderStatusMessage}</div>
          )}
        </form>
      </div>
    </div>
  );
};

export default OrderModal; 