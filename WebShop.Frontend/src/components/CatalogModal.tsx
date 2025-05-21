import React from 'react';
import './CatalogModal.css';

interface CatalogModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const CatalogModal: React.FC<CatalogModalProps> = ({ isOpen, onClose }) => {
  if (!isOpen) {
    return null;
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close-button" onClick={onClose}>X</button>
        <h2>Каталог товарів</h2>
        <ul>
          <li><a href="/products/women" onClick={onClose}>Жіночий одяг</a></li>
          <li><a href="/products/men" onClick={onClose}>Чоловічий одяг</a></li>
          <li><a href="/products/kids" onClick={onClose}>Дитячий одяг</a></li>
          <li><a href="/products/accessories" onClick={onClose}>Аксесуари</a></li>
          <li><a href="/products" onClick={onClose}>Усі товари</a></li>
        </ul>
      </div>
    </div>
  );
};

export default CatalogModal; 