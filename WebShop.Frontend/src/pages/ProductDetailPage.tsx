import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import './ProductDetailPage.css';
import StarRating from '../components/StarRating';
import OrderModal from '../components/OrderModal';
import { PaymentType, DeliveryType } from '../types/enums';
import type { User, FetchedComment, CommentSubmitDto, RatingSubmitDto, FetchedRating, ProductDetail, ProductDetailOrderItemDto, ProductDetailOrderDto } from '../types/interfaces';

const ProductDetailPage: React.FC = () => {
  const { productId } = useParams<{ productId: string }>();
  const navigate = useNavigate();
  
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [selectedImageUrl, setSelectedImageUrl] = useState<string | null>(null);
  
  const [comments, setComments] = useState<FetchedComment[]>([]);
  const [averageRating, setAverageRating] = useState<number>(0);
  const [newCommentText, setNewCommentText] = useState<string>("");
  const [newRatingValue, setNewRatingValue] = useState<number>(0); 
  const [reviewError, setReviewError] = useState<string | null>(null);
  const [reviewSuccess, setReviewSuccess] = useState<string | null>(null);
  const [isSubmittingReview, setIsSubmittingReview] = useState<boolean>(false);

  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [pageError, setPageError] = useState<string | null>(null);
  const [orderError] = useState<string | null>(null);
  const [isOrdering] = useState<boolean>(false);
  const [isDeletingComment, setIsDeletingComment] = useState<boolean>(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);
  const [isOrderModalOpen, setIsOrderModalOpen] = useState<boolean>(false);
  const [orderStatusMessage, setOrderStatusMessage] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const storedUser = localStorage.getItem('user');
    if (token && storedUser) {
      setIsAuthenticated(true);
      try { setCurrentUser(JSON.parse(storedUser)); }
      catch (e) { 
        console.error("Помилка при парсингу збереженого користувача:", e); 
        setIsAuthenticated(false); 
        setCurrentUser(null);
      }
    } else {
      setIsAuthenticated(false);
      setCurrentUser(null);
    }
  }, []);

  const fetchProductDataInternal = useCallback(async () => {
    if (!productId) throw new Error("ID товару відсутнє для завантаження даних.");
    const response = await fetch(`/api/Products/${productId}`);
    if (!response.ok) throw new Error(`Помилка завантаження товару: ${response.status} ${response.statusText}`);
    return response.json() as Promise<ProductDetail>;
  }, [productId]);

  const fetchCommentsAndRatingsInternal = useCallback(async () => {
    if (!productId) return { comments: [], ratings: [] }; 
    let fetchedComments: FetchedComment[] = [];
    let fetchedRatings: FetchedRating[] = [];
    try {
      const commentsResponse = await fetch(`/api/Comments/product/${productId}`);
      if (commentsResponse.ok) {
        const rawComments: any[] = await commentsResponse.json();
        fetchedComments = rawComments.map(c => ({
          id: c.id,
          productId: c.productId,
          userId: c.userId,
          text: c.text,
          date: c.date, 
          userName: c.username || c.Username || (c.user && c.user.username) || `User ${c.userId}` 
        })).sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
      } else { console.error('Не вдалося завантажити відгуки:', commentsResponse.status, await commentsResponse.text()); }
    } catch (e) { console.error('Помилка при обробці завантаження відгуків:', e); }

    try {
      const ratingsResponse = await fetch(`/api/Ratings/product/${productId}`);
      if (ratingsResponse.ok) {
        fetchedRatings = await ratingsResponse.json() as FetchedRating[];
      } else { console.error('Не вдалося завантажити оцінки:', ratingsResponse.status, await ratingsResponse.text()); }
    } catch (e) { console.error('Помилка при обробці завантаження оцінок:', e); }
    return { comments: fetchedComments, ratings: fetchedRatings };
  }, [productId]);

 useEffect(() => {
    if (!productId) {
      setIsLoading(false);
      setPageError("Товар не знайдено.");
      return;
    }
    setIsLoading(true);
    setPageError(null); 

    Promise.all([
        fetchProductDataInternal(),
        fetchCommentsAndRatingsInternal()
    ]).then(([productData, reviewsData]) => {
        if (productData) {
            setProduct(productData);
            if (productData.imageUrls && productData.imageUrls.length > 0) {
                setSelectedImageUrl(productData.imageUrls[0]);
            }
        } else {
            if (!pageError) setPageError("Не вдалося завантажити дані товару.");
        }
        setComments(reviewsData.comments);
        if (reviewsData.ratings.length > 0) {
            const totalRating = reviewsData.ratings.reduce((acc, r) => acc + r.value, 0);
            setAverageRating(totalRating / reviewsData.ratings.length);
        } else {
            setAverageRating(0);
        }
    }).catch(err => {
        console.error("Не вдалося завантажити дані сторінки:", err);
        if (!pageError) { 
            setPageError(err instanceof Error ? err.message : "Сталася помилка при завантаженні сторінки.");
        }
    }).finally(() => {
        setIsLoading(false);
    });
}, [productId, fetchProductDataInternal, fetchCommentsAndRatingsInternal, pageError]); 
  
  const handleOrderClick = () => {
    if (!isAuthenticated || !currentUser) {
      navigate(`/login`, { state: { from: `/product/${productId}` } });
      return;
    }

    if (product && product.stock > 0) {
      setIsOrderModalOpen(true);
    }
  };

  const handleOrderSubmit = async (orderData: {
    paymentType: PaymentType;
    deliveryType: DeliveryType;
    deliveryAddress: string;
    firstName: string;
    lastName: string;
    phone: string;
  }) => {
    if (!product || !currentUser) return;

    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/login');
      return;
    }

    const orderItems: ProductDetailOrderItemDto[] = [
      { ProductId: product.id, Quantity: 1, UnitPrice: product.price }
    ];

    const orderDto: ProductDetailOrderDto = {
      OrderItems: orderItems,
      PaymentType: orderData.paymentType,
      DeliveryType: orderData.deliveryType,
      DeliveryAddress: orderData.deliveryAddress,
      FirstName: orderData.firstName,
      LastName: orderData.lastName,
      Phone: orderData.phone
    };

    try {
      const response = await fetch('/api/Orders', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(orderDto)
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        setOrderStatusMessage(errorData.title || errorData.message || `Помилка створення замовлення: ${response.status}`);
        throw new Error(errorData.title || errorData.message || `Помилка створення замовлення: ${response.status}`);
      }

      const createdOrder: ProductDetailOrderDto = await response.json();
      if (createdOrder.PaymentDeeplink) {
        setOrderStatusMessage('Замовлення створено!Оплату можна здійснити за посиланням.');
        window.open(createdOrder.PaymentDeeplink, '_blank');
      } else {
        setOrderStatusMessage('Замовлення успішно створено! Менеджер зв\'яжеться з вами.');
      }
    } catch (err) {
      console.error('Помилка при створенні замовлення:', err);
      throw err;
    }
  };

  const handleReviewSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    
    if (!productId) {
      setReviewError("Помилка: ID товару не знайдено");
      return;
    }

    if (!isAuthenticated || !currentUser) {
      setReviewError("Для відправки відгуку потрібно увійти в систему.");
      return;
    }

    if (!newCommentText.trim() && newRatingValue === 0) {
      setReviewError("Нічого не було надіслано. Будь ласка, введіть текст або оцінку.");
      return;
    }

    setIsSubmittingReview(true);
    setReviewError(null);
    setReviewSuccess(null);

    const token = localStorage.getItem('token');
    if (!token) {
      setReviewError("Помилка автентифікації. Будь ласка, увійдіть знову.");
      setIsSubmittingReview(false);
      return;
    }

    try {
      if (newCommentText.trim()) {
        const payload: CommentSubmitDto = {
          productId: parseInt(productId),
          userId: currentUser.id,
          text: newCommentText.trim()
        };
        const res = await fetch('/api/Comments', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(payload)
        });
        
        if (!res.ok) {
          const errorData = await res.json().catch(() => ({}));
          throw new Error(errorData.message || `Помилка додавання коментаря: ${res.status}`);
        }
      }

      if (newRatingValue > 0) {
        const payload: RatingSubmitDto = {
          productId: parseInt(productId),
          userId: currentUser.id,
          value: newRatingValue
        };
        const res = await fetch('/api/Ratings', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(payload)
        });
        
        if (!res.ok) {
          const errorData = await res.json().catch(() => ({}));
          throw new Error(errorData.message || `Помилка додавання оцінки: ${res.status}`);
        }
      }

      setReviewSuccess("Дякуємо за ваш відгук!");
      setNewCommentText("");
      setNewRatingValue(0);
      
      const updatedReviewsData = await fetchCommentsAndRatingsInternal();
      if (updatedReviewsData) {
          setComments(updatedReviewsData.comments);
          if (updatedReviewsData.ratings.length > 0) {
              const totalRating = updatedReviewsData.ratings.reduce((acc, r) => acc + r.value, 0);
              setAverageRating(totalRating / updatedReviewsData.ratings.length);
          } else {
              setAverageRating(0);
          }
      }

      setTimeout(() => setReviewSuccess(null), 3000);
    } catch (err) {
      console.error("Помилка при відправці відгуку:", err);
      setReviewError(err instanceof Error ? err.message : "Не вдалося відправити відгук.");
      setTimeout(() => setReviewError(null), 5000);
    } finally {
      setIsSubmittingReview(false);
    }
  };

  const handleThumbnailClick = (imageUrl: string) => {
    setSelectedImageUrl(imageUrl);
  };

  const handleDeleteComment = async (commentId: number) => {
    console.log(`Спроба видалити відгук з id: ${commentId}`);
    if (!isAuthenticated || !currentUser) {
      setDeleteError("Для видалення відгуку потрібно увійти в систему.");
      return;
    }

    setIsDeletingComment(true);
    setDeleteError(null);
    const token = localStorage.getItem('token');

    try {
      const response = await fetch(`/api/Comments/${commentId}`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${token}`,
        },
      });
      
      console.log('Delete response status:', response.status);
      const responseText = await response.text();
      console.log('Delete response text:', responseText);

      if (!response.ok) {
        try {
            const parsedError = JSON.parse(responseText);
            throw new Error(parsedError.message || parsedError.title || `Помилка видалення: ${response.status}`);
        } catch (e) {
            throw new Error(responseText || `Помилка видалення: ${response.status}`);
        }
      }

      setComments(prevComments => prevComments.filter(comment => comment.id !== commentId));

      fetchCommentsAndRatingsInternal().then(data => {
          setComments(data.comments);
          if (data.ratings.length > 0) {
              const totalRatingVal = data.ratings.reduce((acc, r) => acc + r.value, 0);
              setAverageRating(totalRatingVal / data.ratings.length);
          } else {
              setAverageRating(0);
          }
      }).catch(err => {
          console.error("Помилка оновлення відгуків після видалення:", err);
          setDeleteError("Відгук видалено, але не вдалося оновити список.");
      });

    } catch (err) {
      console.error("Видалення відгуку не вдалося:", err);
      setDeleteError(err instanceof Error ? err.message : "Не вдалося видалити відгук.");
      setTimeout(() => setDeleteError(null), 7000);
    } finally {
      setIsDeletingComment(false);
    }
  };

  if (isLoading) return <p className="status-message">Завантаження...</p>;
  if (pageError) return <p className="status-message error-message">{pageError}</p>;
  if (!product) return <p className="status-message">Товар не знайдено.</p>;

  return (
    <div className="product-detail-page">
      
      <div className="product-detail-layout">
        <div className="product-gallery">
          {selectedImageUrl ? (
            <img src={selectedImageUrl} alt={product.name} className="main-product-image" />
          ) : product.imageUrls && product.imageUrls.length > 0 ? (
            <img src={product.imageUrls[0]} alt={product.name} className="main-product-image" />
          ) : (
            <div className="image-placeholder">Зображення недоступне</div>
          )}
          {product.imageUrls && product.imageUrls.length > 1 && (
            <div className="thumbnail-gallery">
              {product.imageUrls.map((url, index) => (
                <img 
                  key={index} 
                  src={url} 
                  alt={`${product.name} thumbnail ${index + 1}`}
                  className={`thumbnail-image ${url === (selectedImageUrl || product.imageUrls[0]) ? 'active' : ''}`}
                  onClick={() => handleThumbnailClick(url)}
                />
              ))}
            </div>
          )}
        </div>

        <div className="product-info-details">
          <h1 className="product-title-detail">{product.name}</h1>
          <div className="product-id">ID товару: {product.id}</div>
          <div className="average-rating-section">
            {averageRating > 0 ? <StarRating rating={averageRating} /> : <p>Ще немає оцінок</p>}
            <span>({averageRating > 0 ? averageRating.toFixed(1) : '0'}, {comments.length} відгуків)</span>
          </div>
          <p className="product-price-detail">{product.price.toFixed(2)} грн</p>
          <p className="product-description-detail">{product.description}</p>
          <p>Категорія: {product.category}</p>
          <p>В наявності: {product.stock > 0 ? `${product.stock} од.` : 'Немає в наявності'}</p>
          <button className="order-button" onClick={handleOrderClick} disabled={isOrdering || product.stock === 0}>
            {product.stock === 0 ? 'Немає в наявності' : (isOrdering ? 'Обробка...' : 'Замовити')}
          </button>
          {orderError && <p className="error-message order-error-message">{orderError}</p>} 
          {product.stock <= 5 && product.stock > 0 && 
            <p className="low-stock-message">Залишилось мало!</p> 
          }
        </div>
      </div>


      <div className="reviews-section">
        <h3>Відгуки ({comments.length})</h3>
        {deleteError && <p className="error-message delete-error-message">{deleteError}</p>}
        {comments.length > 0 ? (
          <div className="comments-list">
            {comments.map((comment) => (
              <div key={comment.id} className="comment-item">
                <div className="comment-header">
                  <span className="comment-author">{comment.userName}</span>
                  <div>
                    <span className="comment-date">{new Date(comment.date).toLocaleDateString('uk-UA')}</span>
                    {isAuthenticated && currentUser && currentUser.id === comment.userId && (
                      <button 
                        onClick={() => handleDeleteComment(comment.id)} 
                        className="delete-comment-button"
                        title="Видалити відгук"
                        disabled={isDeletingComment}
                      >
                        &#x1F5D1;
                      </button>
                    )}
                  </div>
                </div>
                <p className="comment-text">{comment.text}</p>
              </div>
            ))}
          </div>
        ) : (
          <p>Відгуків поки що немає. Будьте першим!</p>
        )}
      </div>

      {isAuthenticated && product && (
        <div className="review-form-section">
          <h3>Залишити відгук</h3>
          <form onSubmit={handleReviewSubmit} className="review-form">
            <div className="form-group">
              <label htmlFor="rating">Оцінка (1-5):</label>
              <StarRating 
                rating={newRatingValue} 
                onRatingChange={setNewRatingValue} 
                interactive={true} 
                maxStars={5} 
              />
            </div>
            <div className="form-group">
              <label htmlFor="commentText">Ваш коментар:</label>
              <textarea 
                id="commentText" 
                name="commentText" 
                rows={4} 
                value={newCommentText} 
                onChange={(e) => setNewCommentText(e.target.value)}
                placeholder="Відгук (необов'язково)"
                className="form-control"
              />
            </div>
            {reviewError && <p className="error-message">{reviewError}</p>}
            {reviewSuccess && <p className="success-message">{reviewSuccess}</p>}
            <button 
                type="submit" 
                className="btn btn-primary" 
                disabled={isSubmittingReview || (!newCommentText.trim() && newRatingValue === 0)} >
              {isSubmittingReview ? 'Відправка...' : 'Надіслати відгук'}
            </button>
          </form>
        </div>
      )}
      {!isAuthenticated && product && (
        <div className="review-login-prompt">
          <p>Будь ласка, <Link to={`/login?returnTo=/product/${productId}`}>увійдіть</Link> або <Link to="/register">зареєструйтесь</Link>, щоб залишити відгук.</p>
        </div>
      )}

      <OrderModal
        isOpen={isOrderModalOpen}
        onClose={() => { setIsOrderModalOpen(false); setOrderStatusMessage(null); }}
        product={product ? {
          id: product.id,
          name: product.name,
          price: product.price
        } : {
          id: 0,
          name: '',
          price: 0
        }}
        userProfile={currentUser ? {
          firstName: currentUser.firstName,
          lastName: currentUser.lastName,
          address: currentUser.address,
          phone: currentUser.phone
        } : {}}
        onSubmit={handleOrderSubmit}
        orderStatusMessage={orderStatusMessage}
        onStatusMessageChange={setOrderStatusMessage}
      />
    </div>
  );
};

export default ProductDetailPage; 