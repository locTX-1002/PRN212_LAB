using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObjects;
using Repositories;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository repo;

        public ProductService() : this(new ProductRepository()) { }
        public ProductService(IProductRepository productRepository) { repo = productRepository; }

        public void DeleteProduct(Product p) => repo.DeleteProduct(p);
        public Product? GetProductById(int id) => repo.GetProductById(id);
        public List<Product> GetProducts() => repo.GetProducts();
        public Task<List<Product>> GetProductsAsync() => repo.GetProductsAsync();
        public void SaveProduct(Product p) => repo.SaveProduct(p);
        public void UpdateProduct(Product p) => repo.UpdateProduct(p);
    }
}
