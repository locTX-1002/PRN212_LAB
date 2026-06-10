using BusinessObjects;
using System.Collections.Generic;
using DataAccessLayer;

namespace Repositories;

public class ProductRepository : IProductRepository
{
    public void DeleteProduct(int productId) => ProductDAO.DeleteProduct(productId);
    public void SaveProduct(Product p) => ProductDAO.SaveProduct(p);
    public void UpdateProduct(Product p) => ProductDAO.UpdateProduct(p);
    public List<Product> GetProducts() => ProductDAO.GetProducts();
    public Product? GetProductById(int id) => ProductDAO.GetProductById(id);
}
