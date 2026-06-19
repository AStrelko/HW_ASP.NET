using less_06.App.Models;
using less_06.App.Storage;
using Microsoft.EntityFrameworkCore;

namespace less_06.App.Services;

public class ProductServices
{
    // Get all products
    // Add, update, delete products
    // Get product by id
    /*
    private readonly DataContext _context;

    public ProductServices(DataContext context)
    {
        _context = context;
    }
    
    //отримати за id
    public async Task<Product?> GetById(int id)
    {
        var product = await _context.Set<Product>().FindAsync(id);
        
        return product;
    }
    
    // отримати всі товари
    public async Task<List<Product>> GetAll()
    {
        return await _context.Set<Product>().ToListAsync();
    }
    
    // + товар
    public async Task<Product> Add(Product product)
    {
        await _context.Set<Product>().AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }
    //- товар за id
    public async Task<bool> Delete(int id)
    {
        var product = await _context.Set<Product>().FindAsync(id);
        if(product == null) return false;
        _context.Set<Product>().Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
    // оновити товар за id
    public async Task<Product?> Update(int id,
        string newName, string newDescription, decimal newPrice, int newCategoryId )
    {
        var product = await _context.Set<Product>().FindAsync(id);
        if (product == null) return null;
        product.Name = newName;
        product.Description = newDescription;
        product.Price = newPrice;
        
        await _context.SaveChangesAsync();
        return product;
    }
    
    */
    // Get all products
    // Add, update, delete products
    // Get product by id    
       private readonly DataContext _context;
    
    public ProductServices(DataContext context)
    {
        _context = context;
    }
    //отримую продукт по id
    public async Task<Product?> GetById(int id, CancellationToken cancellationToken)
    {
        return await _context.Set<Product>().FindAsync([id], cancellationToken);
    }
    
    // отримую всі продукти
    public async Task<List<Product>> GetAll(ProductsFilter filter, CancellationToken cancellationToken)
    {
        var productsQuery = _context.Set<Product>()
            .AsNoTracking();
        //якщо у нас є  ключіве слово
        if(!string.IsNullOrEmpty(filter.SearchQuery))
            //шукає по запитуємоьу слову в назві або в описі
            productsQuery = productsQuery.Where(p => 
                p.Name.Contains(filter.SearchQuery) || p.Description.Contains(filter.SearchQuery));
        // якщо маю ключ
        if(filter.SortingKey == "price")
            //сортую по ключу
            productsQuery = productsQuery.OrderBy(p => p.Price);
        else if(filter.SortingKey == "name")
            productsQuery = productsQuery.OrderBy(p => p.Name);
        else productsQuery = productsQuery.OrderBy(p => p.Id);

        return await productsQuery
            .Skip(filter.Offset)//скільки пропустити
            .Take(filter.Limit)//скільки взяти
            .ToListAsync(cancellationToken);// вертаю ToList - Async!!!
    }
    //оновленя за id
    public async Task<Product> Update(int id, Product updated, CancellationToken cancellationToken)
    {
        ValidateProduct(updated);//додаю метод валідації( перевірка правильності даних)
        //шукаю продукт по [id]
        var product = await _context.Set<Product>().FindAsync([id], cancellationToken);
        if (product is null) return null;

        product.Name = updated.Name;
        product.Description = updated.Description;
        product.Price = updated.Price;

        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }
    //додавання 
    public async Task<Product> Add(Product product, CancellationToken cancellationToken)
    {
        ValidateProduct(product);//додаю метод валідації( перевірка правильності даних)

        _context.Set<Product>().Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }
    //вадалення за id
    public async Task Delete(int id, CancellationToken cancellationToken)
    {
        await _context.Set<Product>().Where(p => p.Id == id)
            //видалення та закінчення транзакції
            .ExecuteDeleteAsync(cancellationToken);
    }
    // перевірка волідації данних
    private void ValidateProduct(Product product)
    {
        if(string.IsNullOrEmpty(product.Name))
            throw new ArgumentException("Product name is required");

        if(product.Price <= 0)
            throw new ArgumentException("Product price must be greater than 0");

        if(product.Description?.Length > 200)
            throw new ArgumentException("Product description must be less than 200 characters");
    }
    
    public async Task Restore(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var borderDate = now.AddDays(-30);

        // видоляю товари старійше 30 днів
        await _context.Set<Product>()
            .Where(p => p.CreatedAt < borderDate)
            .ExecuteDeleteAsync(cancellationToken);

        // випровляю товари із майбутного
        await _context.Set<Product>()
            .Where(p => p.CreatedAt > now)
            .ExecuteUpdateAsync(
                p => p.SetProperty(
                    product => product.CreatedAt,
                    now),
                cancellationToken);
    }
}


