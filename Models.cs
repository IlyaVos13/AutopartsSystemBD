using System;

namespace AutopartsSystemBD
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
    }

    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    public class Part
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Article { get; set; } = "";
    }

    public class SuppliedPart
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int PartId { get; set; }
        public decimal CurrentPrice { get; set; }
    }

    public class Purchase
    {
        public int Id { get; set; }
        public int SuppliedPartId { get; set; }
        public int UserId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalSum { get; set; }
    }

    public class PriceHistory
    {
        public int Id { get; set; }
        public int SuppliedPartId { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime NotificationDate { get; set; }
        public DateTime StartDate { get; set; }
    }
}