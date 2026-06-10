using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObjects;
using DataAccessLayer;

namespace Repositories
{
    public class ProductRepository : IProductRepository
    {
        public void DeleteProduct(Product p) => ProductDAO.DeleteProduct(p);
        public void SaveProduct(Product p) => ProductDAO.SaveProduct(p);
        public void UpdateProduct(Product p) => ProductDAO.UpdateProduct(p);
        public List<Product> GetProducts() => ProductDAO.GetProducts();
        public Task<List<Product>> GetProductsAsync() => ProductDAO.GetProductsAsync();
        public Product? GetProductById(int id) => ProductDAO.GetProductById(id);
    }
}
