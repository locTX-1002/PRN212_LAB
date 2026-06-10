using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer;

public class ProductDAO
{
    public static List<Product> GetProducts()
    {
        using var db = new MyStoreContext();
        // Include Category để DataGrid hiển thị tên category
        return db.Products.Include(p => p.Category).ToList();
    }

    public static void SaveProduct(Product p)
    {
        using var context = new MyStoreContext();
        context.Products.Add(p);
        context.SaveChanges();
    }

    public static void UpdateProduct(Product p)
    {
        using var context = new MyStoreContext();
        context.Entry(p).State = EntityState.Modified;
        context.SaveChanges();
    }

    public static void DeleteProduct(int productId)
    {
        using var context = new MyStoreContext();
        var p1 = context.Products.SingleOrDefault(c => c.ProductId == productId);
        if (p1 == null) return;
        context.Products.Remove(p1);
        context.SaveChanges();
    }

    public static Product? GetProductById(int id)
    {
        using var db = new MyStoreContext();
        return db.Products.FirstOrDefault(c => c.ProductId == id);
    }
}
