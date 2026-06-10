using System.Collections.Generic;
using BusinessObjects;

namespace Repositories;

public interface IProductRepository
{
    void SaveProduct(Product p);
    void DeleteProduct(int productId);
    void UpdateProduct(Product p);
    List<Product> GetProducts();
    Product? GetProductById(int id);
}
