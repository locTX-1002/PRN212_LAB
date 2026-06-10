using BusinessObjects;
using System.Collections.Generic;

namespace Services;

public interface IProductService
{
    void SaveProduct(Product p);
    void DeleteProduct(int productId);
    void UpdateProduct(Product p);
    List<Product> GetProducts();
    Product? GetProductById(int id);
}
