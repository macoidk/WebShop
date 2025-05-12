namespace WebShop.Models
{
    public class Product
    {
        private int _id;
        private string _name;
        private string _description;
        private string _category;
        private decimal _price;
        private int _stock;
        private List<string> _imageUrls;
        private List<Comment> _comments;
        private List<Rating> _ratings;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public int Stock
        {
            get { return _stock; }
            set { _stock = value; }
        }

        public List<string> ImageUrls
        {
            get { return _imageUrls; }
            set { _imageUrls = value; }
        }

        public List<Comment> Comments
        {
            get { return _comments; }
            set { _comments = value; }
        }

        public List<Rating> Ratings
        {
            get { return _ratings; }
            set { _ratings = value; }
        }
    }
}