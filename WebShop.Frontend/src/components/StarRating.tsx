import React, { useState } from 'react';
import './StarRating.css'; 

interface StarRatingProps {
  rating: number; 
  maxStars?: number;
  onRatingChange?: (rating: number) => void;
  interactive?: boolean;
}

const StarRating: React.FC<StarRatingProps> = ({ rating, maxStars = 5, onRatingChange, interactive = false }) => {
  const [hoverRating, setHoverRating] = useState(0);

  const handleClick = (newRating: number) => {
    if (interactive && onRatingChange) {
      onRatingChange(newRating);
    }
  };

  const handleMouseEnter = (newRating: number) => {
    if (interactive) {
      setHoverRating(newRating);
    }
  };

  const handleMouseLeave = () => {
    if (interactive) {
      setHoverRating(0);
    }
  };

  const currentRating = hoverRating > 0 ? hoverRating : rating;

  const fullStars = Math.floor(currentRating);
  const emptyStars = maxStars - fullStars;

  return (
    <div className={`star-rating ${interactive ? 'interactive' : ''}`}>
      {[...Array(maxStars)].map((_, index) => {
        const starValue = index + 1;
        let starClass = 'empty-star';
        if (starValue <= currentRating) {
          starClass = 'full-star';
        }
        
        return (
          <span 
            key={`star-${index}`} 
            className={`star ${starClass} ${interactive ? 'interactive-star' : ''}`}
            onClick={() => handleClick(starValue)}
            onMouseEnter={() => handleMouseEnter(starValue)}
            onMouseLeave={handleMouseLeave}
          >
            {starValue <= currentRating ? '\u2605' : '\u2606'} 
          </span> 
        );
      })}
    </div>
  );
};

export default StarRating; 