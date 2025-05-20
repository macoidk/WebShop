import React from 'react';
import type { Category } from '../data/categoriesData';
import './CategorySection.css';

interface CategorySectionProps {
  category: Category;
}

const CategorySection: React.FC<CategorySectionProps> = ({ category }) => {
  const bannerStyle = {
    backgroundImage: `url(${category.bannerImage})`,
  };

  return (
    <section className="category-detailed-section" id={`category-${category.id}`}>
      <div 
        className="category-banner"
        style={bannerStyle}
      >
        <h2 className="category-banner-title">{category.title}</h2>
      </div>
    </section>
  );
};

export default CategorySection; 