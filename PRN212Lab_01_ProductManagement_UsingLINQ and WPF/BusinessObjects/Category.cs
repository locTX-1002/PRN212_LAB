using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects
{
    public class Category
    {
        public Category()
        {
            Products = new List<Product>();
        }
        public Category(int CategoryId,string CategoryName)
        {
            this.CategoryId = CategoryId;
            this.CategoryName = CategoryName;
            Products = new List<Product>();
        }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<Product> Products { get; set; }
    }
}
