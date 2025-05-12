using System.Runtime.CompilerServices;


namespace WebShop.Models
{
    public class Order
    {
        private int _id;
        private int _userId;
        private User _user;
        private DateTime _orderDate;
        private decimal _totalAmount;
        private OrderStatus _status;
        private List<OrderItem> _orderItems;
        private DeliveryType _deliveryType;
        private string _deliveryAddress;
        private PaymentType _paymentType;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public User User
        {
            get { return _user; }
            set { _user = value; }
        }

        public DateTime OrderDate
        {
            get { return _orderDate; }
            set { _orderDate = value; }
        }

        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set { _totalAmount = value; }
        }

        public OrderStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public List<OrderItem> OrderItems
        {
            get { return _orderItems; }
            set { _orderItems = value; }
        }

        public DeliveryType DeliveryType
        {
            get { return _deliveryType; }
            set { _deliveryType = value; }
        }

        public string DeliveryAddress
        {
            get { return _deliveryAddress; }
            set { _deliveryAddress = value; }
        }

        public PaymentType PaymentType
        {
            get { return _paymentType; }
            set { _paymentType = value; }
        }
    }
}