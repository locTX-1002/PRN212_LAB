# 📚 PRN212 — Tài liệu ôn thi Lab 1
**Product Management — LINQ + WPF**

> Tài liệu này tổng hợp toàn bộ kiến thức của Lab 1 để bạn ôn thi: kiến trúc, từng layer, LINQ, WPF/XAML, EF Core, MVVM, và các tình huống thi hay gặp.

---

## 📑 Mục lục

1. [Tổng quan kiến trúc N-tier](#1-tổng-quan-kiến-trúc-n-tier)
2. [Layer 1 — BusinessObjects (Entities)](#2-layer-1--businessobjects)
3. [Layer 2 — DataAccessLayer (EF Core + DAO)](#3-layer-2--dataaccesslayer)
4. [Layer 3 — Repositories](#4-layer-3--repositories)
5. [Layer 4 — Services](#5-layer-4--services)
6. [Layer 5 — WPFApp (UI)](#6-layer-5--wpfapp-ui)
7. [LINQ — Toàn bộ query trong lab](#7-linq--toàn-bộ-query)
8. [WPF/XAML — DataBinding, MVVM, Triggers](#8-wpfxaml--databinding-mvvm-triggers)
9. [EF Core cheat sheet](#9-ef-core-cheat-sheet)
10. [Connection String & Cấu hình](#10-connection-string--cấu-hình)
11. [Tình huống thi hay gặp](#11-tình-huống-thi-hay-gặp)
12. [Bug đã sửa & bài học](#12-bug-đã-sửa--bài-học)

---

## 1. Tổng quan kiến trúc N-tier

Lab 1 dùng kiến trúc **5 tầng** (n-tier / layered architecture). Mỗi tầng là 1 **project riêng** trong solution. Tầng trên chỉ được gọi tầng ngay dưới, KHÔNG được nhảy cóc.

```
┌─────────────────────────────────────────┐
│  WPFApp        (Presentation / UI)      │ ← User tương tác ở đây
└─────────────────────────────────────────┘
                  ↓ gọi
┌─────────────────────────────────────────┐
│  Services      (Business Logic Layer)   │ ← Xử lý nghiệp vụ
└─────────────────────────────────────────┘
                  ↓ gọi
┌─────────────────────────────────────────┐
│  Repositories  (Repository Pattern)     │ ← Trừu tượng hóa data access
└─────────────────────────────────────────┘
                  ↓ gọi
┌─────────────────────────────────────────┐
│  DataAccessLayer (DAO + EF Core)        │ ← Nói chuyện với DB
└─────────────────────────────────────────┘
                  ↓ dùng
┌─────────────────────────────────────────┐
│  BusinessObjects (Entities / POCO)      │ ← Lớp dữ liệu chung
└─────────────────────────────────────────┘
```

### Vì sao chia tầng?

| Lợi ích | Ví dụ thực tế |
|---|---|
| **Separation of Concerns** | UI không biết EF Core là gì. Đổi từ SQL Server → MySQL chỉ sửa DAO. |
| **Testable** | Mock `IProductRepository` để test Service mà không cần DB thật. |
| **Reusable** | ConsoleApp và WPFApp dùng chung Services. |
| **Maintainable** | Sửa logic ở 1 chỗ, không lan ra cả app. |

### Cây project (sau khi đã flatten)

```
PRN212Lab_01_ProductManagement_UsingLINQ and WPF/
├── BusinessObjects/             ← .csproj không phụ thuộc gì
├── DataAccessLayer/             ← phụ thuộc: BusinessObjects, EF Core
├── Repositories/                ← phụ thuộc: BusinessObjects, DataAccessLayer
├── Services/                    ← phụ thuộc: BusinessObjects, Repositories
├── ConsoleApp/                  ← phụ thuộc: BusinessObjects, Services
├── WPFApp/                      ← phụ thuộc: BusinessObjects, Services
├── ProductManagementDemo.slnx   ← Solution file
└── SQLQueryfinal.sql            ← Script tạo DB
```

---

## 2. Layer 1 — BusinessObjects

**Vai trò:** chứa các class entity (POCO — Plain Old CLR Object). Không có logic, chỉ định nghĩa **shape** của dữ liệu. Mọi layer khác đều phụ thuộc layer này.

### Các class chính

#### `Product.cs`
```csharp
public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public short UnitsInStock { get; set; }
    public int CategoryId { get; set; }       // ← FK

    public Category? Category { get; set; }   // ← Navigation property

    [NotMapped]
    public bool IsValid => !string.IsNullOrWhiteSpace(ProductName)
                         && UnitPrice > 0 && UnitsInStock >= 0;
}
```

**Điểm cần nhớ:**
- `CategoryId` (FK column) + `Category` (navigation property) → cặp đôi quan hệ 1-N
- `[NotMapped]` = không tạo column trong DB (đây là tính toán client-side)
- `string.Empty` = `""`, dùng làm default để tránh `null reference warning`

#### `Category.cs`
```csharp
public class Category
{
    public int CategoryId { get; set; }                  // ← PK
    public string CategoryName { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new(); // ← Navigation collection
}
```

**⚠️ Lưu ý naming:** Microsoft khuyến nghị `Id` chứ không phải `ID`. Đặt sai casing sẽ vỡ binding XAML (case-sensitive).

#### `AccountMember.cs`, `Role.cs`
- `AccountMember`: tài khoản đăng nhập (MemberId, Password đã hash, Role).
- `Role`: enum (`Admin = 1`, `Staff = 2`).

#### `PasswordHasher.cs`
- Static class chứa `Hash(string)` và `Verify(string, string)`. Không lưu mật khẩu plain text.

---

## 3. Layer 2 — DataAccessLayer

**Vai trò:** nói chuyện trực tiếp với SQL Server qua EF Core. Tầng này biết về `DbContext`, `SQL`, EF.

### `MyDbContext.cs` — Bộ não của EF

```csharp
public class MyDbContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<AccountMember> AccountMembers { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);  // Không cho xóa Category khi còn Product
    }
}
```

| Khái niệm | Giải thích |
|---|---|
| `DbContext` | Đại diện 1 session làm việc với DB. Mỗi câu query đều đi qua nó. |
| `DbSet<T>` | Đại diện 1 bảng. `db.Products` = `SELECT * FROM Products`. |
| `OnConfiguring` | Khai báo connection string. |
| `OnModelCreating` | Fluent API để cấu hình quan hệ, constraint. |
| `DeleteBehavior.Restrict` | Cấm xóa Category nếu còn Product. (Hành vi khác: `Cascade`, `SetNull`, `NoAction`) |

### Các DAO (Data Access Object)

Mẫu DAO **static**, mở DbContext mới mỗi query (tránh leak):

```csharp
public class ProductDAO
{
    public static List<Product> GetProducts()
    {
        using var db = new MyDbContext();
        return db.Products.Include(p => p.Category).AsNoTracking().ToList();
    }

    public static Product? GetProductById(int id)
    {
        using var db = new MyDbContext();
        return db.Products.Include(p => p.Category)
                         .AsNoTracking()
                         .FirstOrDefault(p => p.ProductId == id);
    }

    public static void SaveProduct(Product p)
    {
        using var db = new MyDbContext();
        db.Products.Add(p);
        db.SaveChanges();
    }

    public static void UpdateProduct(Product product)
    {
        using var db = new MyDbContext();
        var existing = db.Products.Find(product.ProductId);
        if (existing == null) return;
        existing.ProductName = product.ProductName;
        existing.UnitPrice = product.UnitPrice;
        existing.UnitsInStock = product.UnitsInStock;
        existing.CategoryId = product.CategoryId;
        db.SaveChanges();
    }

    public static void DeleteProduct(Product product)
    {
        using var db = new MyDbContext();
        var existing = db.Products.Find(product.ProductId);
        if (existing == null) return;
        db.Products.Remove(existing);
        db.SaveChanges();
    }
}
```

### Từ khóa EF Core quan trọng

| Từ khóa | Tác dụng |
|---|---|
| `Include(p => p.Category)` | **Eager loading** — JOIN luôn bảng Category, tránh lazy load N+1 |
| `AsNoTracking()` | Không theo dõi entity → query đọc-only nhanh hơn, không cache |
| `FirstOrDefault(...)` | Lấy 1 record, không có → null |
| `Find(id)` | Tìm theo PK, dùng cache nếu có |
| `ToList()` | Materialize → thực sự gọi SQL |
| `SaveChanges()` | Commit tất cả thay đổi (INSERT/UPDATE/DELETE) |

---

## 4. Layer 3 — Repositories

**Vai trò:** trừu tượng hóa data access bằng **interface**. Service không biết bên dưới là EF, DAO, hay file system.

### Pattern

**Interface** (Repositories chỉ định nghĩa):
```csharp
public interface IProductRepository
{
    void SaveProduct(Product p);
    void DeleteProduct(Product p);
    void UpdateProduct(Product p);
    List<Product> GetProducts();
    Task<List<Product>> GetProductsAsync();
    Product? GetProductById(int id);
}
```

**Implementation** (gọi DAO):
```csharp
public class ProductRepository : IProductRepository
{
    public void DeleteProduct(Product p) => ProductDAO.DeleteProduct(p);
    public void SaveProduct(Product p) => ProductDAO.SaveProduct(p);
    // ... các method khác
}
```

### Vì sao có lớp này?

Trong bài này nó chỉ là proxy mỏng (gọi DAO 1-1). Trong dự án thực:
- Có thể swap repo (ví dụ Repository EF ↔ Repository in-memory cho test)
- Gộp logic cache, retry, log...
- Service phụ thuộc `interface`, không phụ thuộc `class cụ thể` → **Dependency Inversion** (chữ D trong SOLID)

---

## 5. Layer 4 — Services

**Vai trò:** business logic. UI gọi Service, không bao giờ gọi thẳng Repository hay DAO.

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository repo;

    // Constructor cho dependency injection
    public ProductService(IProductRepository productRepository) { repo = productRepository; }

    // Constructor mặc định (cho lab đơn giản)
    public ProductService() : this(new ProductRepository()) { }

    public void DeleteProduct(Product p) => repo.DeleteProduct(p);
    // ...
}
```

### `CategoryService` — chứa logic chính trong lab

```csharp
public class CategoryService : ICategoryService
{
    public List<Category> GetCategories() => categoryRepository.GetCategories();
    public Category? GetCategoryWithMostProducts() => categoryRepository.GetCategoryWithMostProducts();
    public List<Category> GetTopCategoriesByProductCount(int top)
        => categoryRepository.GetTopCategoriesByProductCount(top);
}
```

### 2 constructor (mẫu thi hay hỏi)

```csharp
public CategoryService() : this(new CategoryRepository()) { }                       // default
public CategoryService(ICategoryRepository repo) { categoryRepository = repo; }     // DI
```

Cả 2 cùng tồn tại để:
- **Default constructor** → dùng nhanh trong code-behind WPF.
- **DI constructor** → unit test có thể truyền mock.

---

## 6. Layer 5 — WPFApp (UI)

### Cấu trúc thư mục

```
WPFApp/
├── App.xaml + App.xaml.cs           ← Entry point, khởi tạo DB
├── LoginWindow.xaml/.cs             ← Màn login
├── MainWindow.xaml/.cs              ← Màn chính (CRUD products)
├── ChangePasswordWindow.xaml/.cs
├── ForgotPasswordWindow.xaml/.cs
├── ManageMembersWindow.xaml/.cs
├── Dialogs/
│   └── AppDialog.xaml/.cs           ← Custom MessageBox
└── Mvvm/
    ├── BaseViewModel.cs             ← INotifyPropertyChanged base
    ├── MainViewModel.cs             ← ViewModel của MainWindow
    └── RelayCommand.cs              ← ICommand implementation
```

### Flow đăng nhập → main

```
App.xaml.cs (Startup)
   ↓
LoginWindow.xaml
   ↓ (Login thành công)
MainWindow.xaml ← truyền AccountMember
   ↓
Load: cboFilterCategory, dgData (DataGrid)
   ↓ user CRUD
```

### `MainWindow.xaml.cs` — khung sườn

```csharp
public partial class MainWindow : Window
{
    private readonly IProductService iProductService;
    private readonly ICategoryService iCategoryService;
    private readonly MainViewModel vm;

    public MainWindow(AccountMember? user)
    {
        InitializeComponent();
        iProductService = new ProductService();
        iCategoryService = new CategoryService();
        vm = new MainViewModel(iProductService, iCategoryService);
        DataContext = vm;                  // ← Kết nối ViewModel với View
    }
}
```

### MVVM trong lab

| Class | Vai trò |
|---|---|
| `MainWindow.xaml` | **View** — XAML định nghĩa giao diện |
| `MainWindow.xaml.cs` | **Code-behind** — vẫn xử lý 1 số sự kiện UI trực tiếp |
| `MainViewModel.cs` | **ViewModel** — giữ state (Products, Categories, Filter), expose binding |
| `BaseViewModel.cs` | Implement `INotifyPropertyChanged` để tự refresh UI khi property đổi |

### `MainViewModel` — phần quan trọng

```csharp
public class MainViewModel : BaseViewModel
{
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ICollectionView ProductsView { get; }  // ← View có Filter

    public MainViewModel(IProductService p, ICategoryService c)
    {
        ProductsView = CollectionViewSource.GetDefaultView(Products);
        ProductsView.Filter = FilterProduct;       // ← Hàm lọc client-side
    }

    public string SearchText { get => searchText; set { ...; ProductsView.Refresh(); } }
    public bool InStockOnly { ... }
    public int? CategoryFilter { ... }

    private bool FilterProduct(object obj)
    {
        if (obj is not Product p) return false;
        if (InStockOnly && p.UnitsInStock <= 0) return false;
        if (CategoryFilter.HasValue && p.CategoryId != CategoryFilter.Value) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        return (p.ProductName ?? "").Contains(SearchText, StringComparison.OrdinalIgnoreCase);
    }
}
```

**Điểm vàng:**
- `ObservableCollection<T>` tự thông báo UI khi Add/Remove → DataGrid auto refresh
- `ICollectionView` cho phép **Filter / Sort / Group** mà không động chạm Products gốc
- `Refresh()` = chạy lại Filter

---

## 7. LINQ — Toàn bộ query

### Cú pháp 2 dạng

**Method syntax** (dùng trong lab):
```csharp
var x = db.Products.Where(p => p.UnitPrice > 10).OrderBy(p => p.ProductName).ToList();
```

**Query syntax** (cũng được, nhưng ít dùng hơn trong lab):
```csharp
var x = (from p in db.Products
         where p.UnitPrice > 10
         orderby p.ProductName
         select p).ToList();
```

### Các method LINQ xuất hiện trong lab

| Method | Ý nghĩa | Ví dụ trong lab |
|---|---|---|
| `Where(pred)` | Lọc | `db.Products.Where(p => p.CategoryId == 1)` |
| `OrderBy(key)` | Sắp xếp tăng | `Categories.OrderBy(c => c.CategoryName)` |
| `OrderByDescending(key)` | Sắp xếp giảm | `Categories.OrderByDescending(c => c.Products.Count)` |
| `Take(n)` | Lấy n phần tử đầu | `.Take(3)` lấy top 3 |
| `Skip(n)` | Bỏ n phần tử đầu (paging) | — |
| `FirstOrDefault(pred)` | Lấy 1 hoặc null | `db.Products.FirstOrDefault(p => p.ProductId == id)` |
| `Any(pred)` | Có ít nhất 1 match? → bool | `vm.Products.Any(p => p.CategoryId == cid)` |
| `Count(pred)` | Đếm | `cat.Products.Count` |
| `Select(proj)` | Map / projection | `top3.Select((c, i) => $"{i+1}. {c.CategoryName}")` |
| `Include(nav)` | EF: JOIN navigation | `db.Categories.Include(c => c.Products)` |
| `GroupBy(key)` | Gom nhóm | (xem bài tập mở rộng) |
| `Sum / Min / Max / Average` | Tổng hợp | — |
| `Distinct()` | Loại trùng | — |
| `Contains(item)` / `StartsWith` | So sánh chuỗi | `p.ProductName.Contains(SearchText, OrdinalIgnoreCase)` |

### 3 query quan trọng trong lab

#### Q1. Lấy category có **nhiều product nhất**

```csharp
public static Category? GetCategoryWithMostProducts()
{
    using var db = new MyDbContext();
    return db.Categories
        .Include(c => c.Products)
        .AsNoTracking()
        .OrderByDescending(c => c.Products.Count)
        .FirstOrDefault();
}
```

**Giải thích từng bước:**
1. `db.Categories` → bảng Categories
2. `.Include(c => c.Products)` → JOIN Products
3. `.AsNoTracking()` → query đọc-only nhanh
4. `.OrderByDescending(c => c.Products.Count)` → sắp xếp theo số lượng product giảm dần
5. `.FirstOrDefault()` → lấy phần tử đầu (nhiều nhất)

#### Q2. Lấy **top N category** theo số product

```csharp
public static List<Category> GetTopCategoriesByProductCount(int top)
{
    using var db = new MyDbContext();
    return db.Categories
        .Include(c => c.Products)
        .AsNoTracking()
        .OrderByDescending(c => c.Products.Count)
        .Take(top)
        .ToList();
}
```

**Khác Q1:** thay `FirstOrDefault()` bằng `Take(top).ToList()` → trả về list N phần tử.

#### Q3. Filter products theo nhiều điều kiện (client-side)

```csharp
private bool FilterProduct(object obj)
{
    if (obj is not Product p) return false;
    if (InStockOnly && p.UnitsInStock <= 0) return false;
    if (CategoryFilter.HasValue && p.CategoryId != CategoryFilter.Value) return false;
    if (string.IsNullOrWhiteSpace(SearchText)) return true;
    return (p.ProductName ?? "").Contains(SearchText, StringComparison.OrdinalIgnoreCase);
}
```

Đây không phải LINQ trên DB mà là filter predicate cho `ICollectionView`.

---

## 8. WPF/XAML — DataBinding, MVVM, Triggers

### 8.1. DataBinding cơ bản

```xml
<TextBox Text="{Binding ProductName}" />
```

- `{Binding X}` → trỏ vào property `X` của `DataContext`
- `DataContext` set ở code-behind: `this.DataContext = vm;`

### 8.2. Binding `ItemsSource` cho DataGrid

```xml
<DataGrid ItemsSource="{Binding ProductsView}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="NAME" Binding="{Binding ProductName}" />
    </DataGrid.Columns>
</DataGrid>
```

`ProductsView` là `ICollectionView` của Products → support filter/sort/group.

### 8.3. ComboBox + DisplayMemberPath + SelectedValuePath

```xml
<ComboBox x:Name="cboCategory"
          ItemsSource="{Binding Categories}"
          DisplayMemberPath="CategoryName"     ← Hiển thị tên
          SelectedValuePath="CategoryId"/>     ← Giá trị thực sự lấy
```

Khi user chọn 1 item: `cboCategory.SelectedValue` = giá trị `CategoryId` của item đó.

**⚠️ Case-sensitive!** Property `CategoryId` (chữ d nhỏ) thì path phải đúng `"CategoryId"`. Sai casing → binding silent fail → SelectedValue = null.

### 8.4. DataTrigger — đổi style theo điều kiện

```xml
<Border>
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Background" Value="Transparent"/>
            <Style.Triggers>
                <DataTrigger Binding="{Binding Products.Count}" Value="0">
                    <Setter Property="Background" Value="#FACC15"/>   <!-- yellow -->
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
</Border>
```

Dùng trong lab để **tô vàng category rỗng** trong dropdown filter.

### 8.5. INotifyPropertyChanged

Mỗi khi property đổi, raise event để UI re-bind:

```csharp
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

Cách dùng trong ViewModel:
```csharp
private string searchText = "";
public string SearchText
{
    get => searchText;
    set { if (SetField(ref searchText, value)) ProductsView.Refresh(); }
}
```

### 8.6. ObservableCollection

```csharp
public ObservableCollection<Product> Products { get; } = new();
```

Khác `List<T>`: tự fire event khi Add/Remove → DataGrid binding tự refresh.

---

## 9. EF Core cheat sheet

| Tác vụ | Code |
|---|---|
| **C** — Insert | `db.Products.Add(p); db.SaveChanges();` |
| **R** — Get all | `db.Products.ToList()` |
| **R** — Get by id | `db.Products.Find(id)` hoặc `FirstOrDefault(p => p.Id == id)` |
| **R** — Filter | `db.Products.Where(p => p.UnitPrice > 10).ToList()` |
| **R** — Include nav | `db.Products.Include(p => p.Category).ToList()` |
| **U** — Update | Load → sửa property → `SaveChanges()` |
| **D** — Delete | `db.Products.Remove(p); db.SaveChanges();` |
| **Async** | `await db.Products.ToListAsync();` |
| **Read-only** | `.AsNoTracking()` trước `.ToList()` |
| **Create DB** | `db.Database.EnsureCreated();` |

### `using var db` — Quan trọng

```csharp
public static List<Product> GetProducts()
{
    using var db = new MyDbContext();   // ← `using` → auto Dispose() cuối hàm
    return db.Products.ToList();
}
```

DbContext **không thread-safe**, **không tái sử dụng**. Mỗi query → 1 instance mới, đóng ngay.

---

## 10. Connection String & Cấu hình

### `appsettings.json` (trong WPFApp/ConsoleApp)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

| Phần | Ý nghĩa |
|---|---|
| `Server=localhost` | SQL Server chạy trên máy local (có thể là `.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`) |
| `Database=...` | Tên DB |
| `Trusted_Connection=True` | Dùng Windows Authentication (không cần user/pass) |
| `TrustServerCertificate=True` | Bỏ qua check SSL self-signed |
| Thay bằng `User Id=sa;Password=xxx` | Nếu dùng SQL Authentication |

### Đọc connection string

```csharp
private static string GetConnectionString()
{
    var config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

    return config.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Missing connection string");
}
```

---

## 11. Tình huống thi hay gặp

### Q1. "Viết LINQ trả về category có nhiều product nhất"
```csharp
db.Categories.Include(c => c.Products)
             .OrderByDescending(c => c.Products.Count)
             .FirstOrDefault();
```

### Q2. "Viết LINQ trả về top N category"
```csharp
db.Categories.Include(c => c.Products)
             .OrderByDescending(c => c.Products.Count)
             .Take(n)
             .ToList();
```

### Q3. "Lọc Product theo giá > 10 và còn hàng"
```csharp
db.Products.Where(p => p.UnitPrice > 10 && p.UnitsInStock > 0).ToList();
```

### Q4. "Tổng tồn kho của 1 category"
```csharp
db.Products.Where(p => p.CategoryId == cid).Sum(p => p.UnitsInStock);
```

### Q5. "Tìm product theo tên (chứa keyword)"
```csharp
db.Products.Where(p => p.ProductName.Contains(keyword)).ToList();
```

### Q6. "Sắp xếp Product theo giá giảm dần, lấy 5 cái đắt nhất"
```csharp
db.Products.OrderByDescending(p => p.UnitPrice).Take(5).ToList();
```

### Q7. "Group product theo category, đếm số lượng từng category"
```csharp
db.Products.GroupBy(p => p.CategoryId)
           .Select(g => new { CategoryId = g.Key, Count = g.Count() })
           .ToList();
```

### Q8. "Vì sao dùng `AsNoTracking()`?"
> Tăng tốc query read-only. Mặc định EF cache mọi entity trả về để có thể update — không cần thì tắt đi, đỡ tốn RAM, nhanh hơn ~10-30%.

### Q9. "`Include` khác gì với JOIN thường?"
> `Include` là **eager loading** — EF tự tạo SQL `LEFT JOIN` để tải navigation property cùng lúc, tránh `lazy loading` phát sinh N+1 query.

### Q10. "Vì sao Repository pattern?"
> Service phụ thuộc `interface`, không phụ thuộc implementation cụ thể → dễ test (mock), dễ thay đổi (đổi storage engine không sửa Service).

### Q11. "MVVM là gì?"
> Model-View-ViewModel. View (XAML) bind vào ViewModel (data + logic UI). ViewModel không biết View. Model là entity. MVVM giúp UI có thể test được.

### Q12. "ObservableCollection khác List như nào?"
> `ObservableCollection<T>` implement `INotifyCollectionChanged` → khi Add/Remove tự fire event → UI binding tự refresh. `List<T>` thì không, phải gán lại ItemsSource.

---

## 12. Bug đã sửa & bài học

### Bug 1: `CategoryID` → `CategoryId` rename không toàn diện
**Triệu chứng:** Save product không nhận category, ComboBox SelectedValue null.

**Nguyên nhân:** Đổi tên property nhưng quên 5 chỗ tham chiếu (Program.cs + MainWindow.xaml).

**Bài học:**
- WPF binding **case-sensitive**.
- Khi rename, dùng Visual Studio Refactor (F2) hoặc grep toàn project.
- Tuân thủ **C# naming convention**: `Id` (không phải `ID`, `iD`).

### Bug 2: Lab 1 thư mục bị lồng dư
**Trước:** `Lab/PRN212Lab_01_.../ProductManagementDemo/<projects>`
**Sau:** `Lab/PRN212Lab_01_.../<projects>` (giống Lab 2)

**Bài học:** consistency trong cấu trúc folder giúp dễ navigate và CI/CD.

### Bug 3: Dung lượng phình to (90 MB cho 2 lab)
**Nguyên nhân:** commit/lưu các folder `bin/`, `obj/`, `.vs/`.

**Sửa:** thêm vào `.gitignore`, dọn bằng `dotnet clean` hoặc xóa thủ công.

**Bài học:** trước khi nén/gửi/commit, luôn `dotnet clean` để bỏ build artifacts.

### Bug 4: Filter dropdown không cho biết category nào rỗng
**Trước:** user phải chọn xong mới biết category rỗng (cả combobox tô vàng).
**Sau:** từng item trong dropdown tô vàng nếu Products.Count == 0.

**Bài học:** good UX hint trước khi user mắc lỗi, không phải sau.

---

## 🎯 Checklist trước khi vào phòng thi

- [ ] Hiểu sơ đồ 5 tầng và dependency direction (trên → dưới, không nhảy cóc)
- [ ] Viết được CRUD bằng EF Core + DbContext
- [ ] Viết được 3 query LINQ kinh điển: `OrderByDescending + FirstOrDefault`, `OrderByDescending + Take`, `Where + Sum`
- [ ] Hiểu `Include`, `AsNoTracking`, `FirstOrDefault` vs `Find`
- [ ] Biết MVVM: View ↔ ViewModel ↔ Model + INotifyPropertyChanged
- [ ] Hiểu DataBinding, DataTrigger, ObservableCollection, ICollectionView
- [ ] Connection string format + nơi đặt (`appsettings.json`)
- [ ] Naming convention C# (`Id`, không `ID`)
- [ ] `using var db = new MyDbContext()` — vì sao
- [ ] Repository pattern phục vụ điều gì (DI, testability)

---

**Chúc bạn thi tốt! 🎓**
