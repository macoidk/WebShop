import React from 'react';
import '../App.css'; 
import womenCategoryImage from '../assets/women-category.png';
import menCategoryImage from '../assets/men-category.png';
import kidsCategoryImage from '../assets/kids-category.png';
import accessoriesCategoryImage from '../assets/accessories-category.png';
import { Link } from 'react-router-dom'; 

const HomePage: React.FC = () => {
  return (
    <>
      <section className="hero-section">
        <h1>Літня Колекція 2025</h1>
        <p>КУПЛЯЙТЕ ДАЙТЕ НАМ ГРОШЕЙ!</p>
        <Link to="/products" className="cta-button">До каталогу</Link>
      </section>

      <section className="promo-categories">
        <h2>Популярні категорії</h2>
        <div className="categories-grid">
          <Link to="/products/women" className="category-item">
            <img src={womenCategoryImage} alt="Жіночий одяг" />
            <h3>Жіночий одяг</h3>
          </Link>
          <Link to="/products/men" className="category-item">
            <img src={menCategoryImage} alt="Чоловічий одяг" />
            <h3>Чоловічий одяг</h3>
          </Link>
          <Link to="/products/kids" className="category-item">
            <img src={kidsCategoryImage} alt="Дитячий одяг" />
            <h3>Дитячий одяг</h3>
          </Link>
          <Link to="/products/accessories" className="category-item">
            <img src={accessoriesCategoryImage} alt="Аксесуари" />
            <h3>Аксесуари</h3>
          </Link>
        </div>
      </section>
    </>
  );
};

export default HomePage; 