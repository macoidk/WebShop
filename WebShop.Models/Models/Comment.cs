namespace WebShop.Models
{
    public class Comment
    {
        private int _id;
        private int _productId;
        private Product _product;
        private int _userId;
        private User _user;
        private string _text;
        private DateTime _date;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
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

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }
    }
}