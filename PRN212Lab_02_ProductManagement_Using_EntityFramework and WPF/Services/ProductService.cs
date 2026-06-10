using BusinessObjects;
using Repositories;
using System.Collections.Generic;

namespace Services;

public class ProductService : IProductService
{
    private readonly IProductRepository iProductRepository;

    public ProductService()
    {
        iProductRepository = new ProductRepository();
    }

    public void DeleteProduct(int productId) => iProductRepository.DeleteProduct(productId);
    public Product? GetProductById(int id) => iProductRepository.GetProductById(id);
    public List<Product> GetProducts() => iProductRepository.GetProducts();
    public void SaveProduct(Product p) => iProductRepository.SaveProduct(p);
    public void UpdateProduct(Product p) => iProductRepository.UpdateProduct(p);
}
