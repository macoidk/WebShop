namespace WebShop.Models
{
    public class User
    {
        private int _id;
        private string _username;
        private string _email;
        private string _passwordHash;
        private UserRole _role;
        private string _firstName;
        private string _lastName;
        private string _address;
        private string _phone;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string PasswordHash
        {
            get { return _passwordHash; }
            set { _passwordHash = value; }
        }

        public UserRole Role
        {
            get { return _role; }
            set { _role = value; }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }
    }
    
}