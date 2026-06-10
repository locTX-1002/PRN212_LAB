using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class Product
    {
        public Product() { }
        public Product(int productId, string productName, decimal unitPrice, short unitsInStock, int categoryId)
        {
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            UnitsInStock = unitsInStock;
            CategoryId = categoryId;
        }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public short UnitsInStock { get; set; }
        public int CategoryId { get; set; }

        // Navigation
        public Category? Category { get; set; }

        [NotMapped]
        public bool IsValid => !string.IsNullOrWhiteSpace(ProductName) && UnitPrice > 0 && UnitsInStock >= 0;
    }
}
