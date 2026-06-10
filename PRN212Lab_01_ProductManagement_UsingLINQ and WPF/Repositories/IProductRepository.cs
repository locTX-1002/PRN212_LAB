using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObjects;

namespace Repositories
{
    public interface IProductRepository
    {
        void SaveProduct(Product p);
        void DeleteProduct(Product p);
        void UpdateProduct(Product p);
        List<Product> GetProducts();
        Task<List<Product>> GetProductsAsync();
        Product? GetProductById(int id);
    }
}
