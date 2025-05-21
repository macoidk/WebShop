import { OrderStatus, PaymentType, DeliveryType } from './enums';

export interface User {
  id: number;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  address?: string;
  phone?: string;
  role?: number;
}

export interface UserRegistrationRequest {
  username: string;
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  address: string;
  phone: string;
}

export interface UserUpdateRequest {
  id: number;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  address?: string;
  phone?: string;
  password?: string;
}

export interface ProfileOrderItemDto {
  id: number;
  orderId: number;
  productId: number;
  quantity: number;
  unitPrice: number;
  productName?: string;
}

export interface ProfileOrderDto {
  id: number;
  userId: number;
  orderDate: string;
  totalAmount: number;
  status: OrderStatus;
  orderItems: ProfileOrderItemDto[];
  deliveryAddress?: string;
  paymentDeeplink?: string;
  paymentType?: PaymentType;
}

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrls?: string[];
  categoryId?: number;
  stock?: number;
}

export interface FetchedComment {
  id: number;
  productId: number;
  userId: number;
  text: string;
  date: string;
  userName: string;
}

export interface CommentSubmitDto {
  productId: number;
  userId: number;
  text: string;
}

export interface RatingSubmitDto {
  productId: number;
  userId: number;
  value: number;
}

export interface FetchedRating {
  id: number;
  productId: number;
  userId: number;
  value: number;
}

export interface ProductDetail {
  id: number;
  name: string;
  description: string;
  price: number;
  imageUrls: string[];
  category: string;
  stock: number;
}

export interface ProductDetailOrderItemDto {
  ProductId: number;
  Quantity: number;
  UnitPrice: number;
}

export interface ProductDetailOrderDto {
  OrderItems: ProductDetailOrderItemDto[];
  PaymentType: PaymentType;
  DeliveryType: DeliveryType;
  DeliveryAddress?: string;
  PaymentDeeplink?: string;
  UserId?: number;
  OrderDate?: string;
  TotalAmount?: number;
  status?: OrderStatus;
  Id?: number;
  FirstName?: string;
  LastName?: string;
  Phone?: string;
}

export interface LoginModel {
  username: string;
  password: string;
}

export interface LoginResponse {
  user: User;
  token: string;
}

export interface Category {
  id: number;
  title: string;
  bannerImage: string;
}

export interface ProductFormData {
  name: string;
  description: string;
  price: string;
  stockQuantity: string;
  imageUrls: string;
  categoryId?: string;
}

export interface OrderItemDto {
  id: number;
  orderId: number;
  productId: number;
  quantity: number;
  unitPrice: number;
  productName?: string;
}

export interface OrderDto {
  id: number;
  userId: number;
  orderDate: string;
  totalAmount: number;
  status: OrderStatus;
  paymentType?: PaymentType;
  deliveryAddress?: string;
  orderItems: OrderItemDto[];
  user?: User;
  firstName?: string;
  lastName?: string;
  phone?: string;
  email?: string;
}

export interface ProductStatisticsDto {
  productId: number;
  productName?: string;
  unitsSold: number;
  revenue: number;
} 