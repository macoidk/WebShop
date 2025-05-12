namespace WebShop.Models
{
    public class OrderItem
    {
        private int _id;
        private int _orderId;
        private Order _order;
        private int _productId;
        private Product _product;
        private int _quantity;
        private decimal _unitPrice;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public int OrderId
        {
            get { return _orderId; }
            set { _orderId = value; }
        }

        public Order Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public int ProductId
        {
            get { return _productId; }
            set { _productId = value; }
        }

        public Product Product
        {
            get { return _product; }
            set { _product = value; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        public decimal UnitPrice
        {
            get { return _unitPrice; }
            set { _unitPrice = value; }
        }
    }
}