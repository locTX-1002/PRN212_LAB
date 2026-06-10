using BusinessObjects;
using Services;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

ICategoryService categoryService = new CategoryService();
IProductService productService = new ProductService();

bool isRunning = true;
while (isRunning)
{
    Console.WriteLine();
    Console.WriteLine("Mời bạn chọn chức năng:");
    Console.WriteLine("1. List of Categories");
    Console.WriteLine("2. List of Products");
    Console.WriteLine("-1. Exit");
    string? op = Console.ReadLine();
    switch (op)
    {
        case "1":
            ShowAllCategories();
            break;
        case "2":
            ShowAllProducts();
            break;
        case "-1":
            Console.WriteLine("Tạm biệt bạn!");
            isRunning = false;
            break;
        default:
            Console.WriteLine("Lựa chọn không hợp lệ!");
            break;
    }
}

void ShowAllCategories()
{
    List<Category> categories = categoryService.GetCategories();
    Console.WriteLine("List Of Categories:");
    foreach (var c in categories)
    {
        Console.WriteLine($"{c.CategoryID}\t{c.CategoryName}");
    }
}

void ShowAllProducts()
{
    List<Product> products = productService.GetProducts();
    Console.WriteLine("List Of Products:");
    foreach (var p in products)
    {
        Console.WriteLine($"{p.ProductId}\t{p.ProductName}\t{p.UnitPrice}\t{p.UnitsInStock}\t{p.CategoryId}");
    }
}
