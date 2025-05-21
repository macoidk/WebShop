import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import type { User, ProductFormData, Category, ProductDetail } from '../../types/interfaces';
import './ProductForm.css';

const ProductForm: React.FC = () => {
  const navigate = useNavigate();
  const { productId } = useParams<{ productId: string }>();
  const isEditMode = Boolean(productId);

  const [formData, setFormData] = useState<ProductFormData>({
    name: '',
    description: '',
    price: '',
    stockQuantity: '',
    imageUrls: '',
    categoryId: ''
  });
  const [imageFiles, setImageFiles] = useState<FileList | null>(null);
  const [existingImageUrls, setExistingImageUrls] = useState<string[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    const userString = localStorage.getItem('user');
    if (userString) {
      const user: User = JSON.parse(userString);
      if (user.role !== 0) {
        navigate('/');
      }
    } else {
      navigate('/login');
    }
  }, [navigate]);


  const fetchProductDetails = useCallback(async (id: string, allCategories: Category[]) => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`/api/Products/${id}`);
      if (!response.ok) {
        throw new Error('Failed to fetch product details');
      }
      const productDetail: ProductDetail = await response.json();
      setExistingImageUrls(productDetail.imageUrls || []);

      let fetchedCategoryId = '';
      if (productDetail.category && allCategories.length > 0) {
        const foundCategory = allCategories.find(
          (cat) => cat.title.toLowerCase() === productDetail.category.toLowerCase()
        );
        if (foundCategory) {
          fetchedCategoryId = foundCategory.id.toString();
        } else {
          console.warn(`Категорія з назвою "${productDetail.category}" не знайдена в списку категорій.`);
          if ((productDetail as any).categoryId !== undefined) {
            fetchedCategoryId = (productDetail as any).categoryId.toString();
          }
        }
      } else if ((productDetail as any).categoryId !== undefined) {
         fetchedCategoryId = (productDetail as any).categoryId.toString();
      }

      setFormData({
        name: productDetail.name,
        description: productDetail.description,
        price: productDetail.price.toString(),
        stockQuantity: productDetail.stock?.toString() || '0',
        imageUrls: productDetail.imageUrls?.join(', ') || '',
        categoryId: fetchedCategoryId,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unknown error occurred');
      console.error('Error fetching product details:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    const staticCategoriesData: Category[] = [
      { id: 1, title: 'men', bannerImage: '' },
      { id: 2, title: 'women', bannerImage: '' },
      { id: 3, title: 'kids', bannerImage: '' },
      { id: 4, title: 'accessories', bannerImage: '' },
    ];
    setCategories(staticCategoriesData);
    if (isEditMode && productId) {
      fetchProductDetails(productId, staticCategoriesData);
    }
  }, [isEditMode, productId, fetchProductDetails]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    if (e.target.type === 'file') {
      const files = (e.target as HTMLInputElement).files;
      setImageFiles(files);
    } else {
      setFormData(prev => ({ ...prev, [name]: value }));
    }
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccessMessage(null);

    const token = localStorage.getItem('token');
    if (!token) {
        setError("Authentication required.");
        setLoading(false);
        navigate('/login');
        return;
    }

    if (!formData.name || !formData.price || !formData.stockQuantity) {
        setError("Назва, ціна та кількість є обов\'язковими полями.");
        setLoading(false);
        return;
    }
    
    const price = parseFloat(formData.price);
    const stock = parseInt(formData.stockQuantity, 10);

    let selectedCategoryTitle: string | undefined = undefined;
    if (formData.categoryId && formData.categoryId !== '') { 
      const selectedCatIdNum = parseInt(formData.categoryId, 10);
      if (!isNaN(selectedCatIdNum)) {
        const foundCategory = categories.find(cat => cat.id === selectedCatIdNum);
        if (foundCategory) {
          selectedCategoryTitle = foundCategory.title;
        } else {
          setError("Обрано недійсну категорію. Будь ласка, оновіть сторінку та спробуйте знову.");
          setLoading(false);
          return;
        }
      } else {
        setError("Невірний формат ID категорії. Будь ласка, оновіть сторінку та спробуйте знову.");
        setLoading(false);
        return;
      }
    }
    if (!selectedCategoryTitle) {
        setError("Категорія є обов\'язковим полем. Будь ласка, виберіть категорію.");
        setLoading(false);
        return;
    }

    if (isNaN(price) || price <= 0) {
        setError("Ціна повинна бути позитивним числом.");
        setLoading(false);
        return;
    }

    if (isNaN(stock) || stock < 0) {
        setError("Кількість на складі повинна бути невід\'ємним числом.");
        setLoading(false);
        return;
    }
    
    const productDataForApi: any = {
      name: formData.name,
      description: formData.description,
      price: price,
      stock: stock,
      Category: selectedCategoryTitle,
    };

    if (isEditMode && productId) {
      productDataForApi.id = parseInt(productId, 10);
    }

    const formDataPayload = new FormData();

    if (imageFiles && imageFiles.length > 0) {
      productDataForApi.imageUrls = [];
      for (let i = 0; i < imageFiles.length; i++) {
        formDataPayload.append('images', imageFiles[i]);
      }
    } else if (isEditMode) {
      productDataForApi.imageUrls = existingImageUrls;
    } else {
      productDataForApi.imageUrls = [];
    }

    formDataPayload.append('productDtoJson', JSON.stringify(productDataForApi));
    
    const url = isEditMode ? `/api/Products/${productId}` : '/api/Products';
    const method = isEditMode ? 'PUT' : 'POST';
    
    const requestHeaders: HeadersInit = {
      'Authorization': `Bearer ${token}`,
    };
    
    let requestBody: BodyInit | undefined = formDataPayload;

    try {
      const response = await fetch(url, {
        method: method,
        headers: requestHeaders, 
        body: requestBody,       
      });

      if (!response.ok) {
        const errorData = await response.text(); 
        throw new Error(`Failed to ${isEditMode ? 'update' : 'create'} product: ${response.status} ${errorData}`);
      }
      
      setSuccessMessage(`Товар успішно ${isEditMode ? 'оновлено' : 'створено'}!`);
      setTimeout(() => {
        navigate('/admin/products');
      }, 1500);

    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unknown error occurred while saving the product');
      console.error('Error saving product:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading && isEditMode && !formData.name) {
    return <div className="product-form-loading">Завантаження даних товару...</div>;
  }
  if (loading && categories.length === 0 && !isEditMode) {
     return <div className="product-form-loading">Завантаження категорій...</div>;
  }

  return (
    <div className="product-form-container">
      <h3>{isEditMode ? 'Редагувати товар' : 'Додати новий товар'}</h3>
      {error && <p className="error-message">{error}</p>}
      {successMessage && <p className="success-message">{successMessage}</p>}
      <form onSubmit={handleSubmit} className="product-form">
        <div className="form-group">
          <label htmlFor="name">Назва товару:</label>
          <input
            type="text"
            id="name"
            name="name"
            value={formData.name}
            onChange={handleChange}
            required
          />
        </div>
        <div className="form-group">
          <label htmlFor="description">Опис:</label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleChange}
            rows={4}
          />
        </div>
        <div className="form-group">
          <label htmlFor="price">Ціна (грн):</label>
          <input
            type="number"
            id="price"
            name="price"
            value={formData.price}
            onChange={handleChange}
            required
            step="0.01"
          />
        </div>
        <div className="form-group">
          <label htmlFor="stockQuantity">Кількість на складі:</label>
          <input
            type="number"
            id="stockQuantity"
            name="stockQuantity"
            value={formData.stockQuantity}
            onChange={handleChange}
            required
            step="1"
          />
        </div>

        {isEditMode && existingImageUrls.length > 0 && (
          <div className="form-group">
            <label>Поточні зображення:</label>
            <div className="current-images">
              {existingImageUrls.map((url, index) => (
                <img key={index} src={url} alt={`Current product image ${index + 1}`} style={{ width: '100px', height: '100px', marginRight: '10px' }} />
              ))}
            </div>
            <p style={{ fontSize: '0.9em', color: '#aaa' }}>Завантаження нових файлів замінить поточні зображення.</p>
          </div>
        )}

        <div className="form-group">
          <label htmlFor="images">{isEditMode ? 'Завантажити нові зображення:' : 'Зображення:'}</label>
          <input
            type="file"
            id="images"
            name="images"
            onChange={handleChange}
            multiple
            accept="image/*"
          />
        </div>
        <div className="form-group">
          <label htmlFor="categoryId">Категорія:</label>
          <select
            id="categoryId"
            name="categoryId"
            value={formData.categoryId || ''}
            onChange={handleChange}
          >
            <option value="">Не вибрано</option>
            {categories.map(category => (
              <option key={category.id} value={category.id.toString()}>
                {category.title}
              </option>
            ))}
          </select>
        </div>
        <div className="form-actions">
          <button type="submit" disabled={loading} className="submit-button">
            {loading ? (isEditMode ? 'Оновлення...' : 'Створення...') : (isEditMode ? 'Оновити товар' : 'Створити товар')}
          </button>
          <button type="button" onClick={() => navigate('/admin/products')} className="cancel-button" disabled={loading}>
            Скасувати
          </button>
        </div>
      </form>
    </div>
  );
};

export default ProductForm; 