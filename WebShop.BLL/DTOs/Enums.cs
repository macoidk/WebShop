namespace WebShop.BLL.DTOs
{
    public enum UserRole
    {
        Administrator = 0,
        Manager = 1,
        RegisteredUser = 2,
        UnregisteredUser = 3
    }

    public enum OrderStatus
    {
        Pending = 0,
        Processed = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum DeliveryType
    {
        Pickup = 0,
        PostOffice = 1
    }

    public enum PaymentType
    {
        CashOnPickup = 0,
        CashOnDelivery = 1,
        BankCard = 2
    }
}