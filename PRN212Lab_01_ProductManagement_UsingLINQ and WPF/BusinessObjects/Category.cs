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
        public Category(int CategoryID,string CategoryName)
        {
            this.CategoryID = CategoryID;
            this.CategoryName = CategoryName;
            Products = new List<Product>();
        }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<Product> Products { get; set; }
    }
}
