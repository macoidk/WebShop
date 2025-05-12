namespace WebShop.Models
{
    public enum OrderStatus
    {
        Pending,
        Processed,
        Completed,
        Cancelled
    }
    public enum DeliveryType
    {
        Pickup,
        PostOffice
    }
    
    public enum PaymentType
    {
        CashOnPickup,
        CashOnDelivery,
        BankCard
    }
    
    public enum UserRole
    {
        Administrator,
        Manager,
        RegisteredUser,
        UnregisteredUser
    }
    
}