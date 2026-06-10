using BusinessObjects;
using Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPFApp.Mvvm
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IProductService productService;
        private readonly ICategoryService categoryService;

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ICollectionView ProductsView { get; }

        public MainViewModel(IProductService productService, ICategoryService categoryService)
        {
            this.productService = productService;
            this.categoryService = categoryService;
            ProductsView = CollectionViewSource.GetDefaultView(Products);
            ProductsView.Filter = FilterProduct;
        }

        private string searchText = "";
        public string SearchText
        {
            get => searchText;
            set { if (SetField(ref searchText, value)) ProductsView.Refresh(); }
        }

        private bool inStockOnly;
        public bool InStockOnly
        {
            get => inStockOnly;
            set { if (SetField(ref inStockOnly, value)) ProductsView.Refresh(); }
        }

        private int? categoryFilter;
        public int? CategoryFilter
        {
            get => categoryFilter;
            set { if (SetField(ref categoryFilter, value)) ProductsView.Refresh(); }
        }

        private bool isLoading;
        public bool IsLoading { get => isLoading; set => SetField(ref isLoading, value); }

        public int TotalCount => Products.Count;
        public int FilteredCount => ProductsView.Cast<object>().Count();

        private bool FilterProduct(object obj)
        {
            if (obj is not Product p) return false;
            if (InStockOnly && p.UnitsInStock <= 0) return false;
            if (CategoryFilter.HasValue && p.CategoryId != CategoryFilter.Value) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            return (p.ProductName ?? "").Contains(SearchText, System.StringComparison.OrdinalIgnoreCase);
        }

        public async Task ReloadAsync()
        {
            IsLoading = true;
            try
            {
                Products.Clear();
                foreach (var p in await productService.GetProductsAsync())
                    Products.Add(p);

                Categories.Clear();
                foreach (var c in categoryService.GetCategories())
                    Categories.Add(c);

                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(FilteredCount));
            }
            finally { IsLoading = false; }
        }
    }
}
