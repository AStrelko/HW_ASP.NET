using Microsoft.AspNetCore.Mvc;
using OnlineStoreLes02Lab.Core.Models;
using OnlineStoreLes02Lab.Storage;
using System.Text.RegularExpressions;
using OnlineStoreLes02Lab.Web.Services;


namespace OnlineStoreLes02Lab.Web.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductController: ControllerBase
{
    private readonly DataContext _context;
    private readonly LogService _logService;

    public ProductController(DataContext context, LogService logService)
    {
        _context = context;
        _logService = logService;
    }
    // отримати всі продукти
    [HttpGet]
    public IActionResult GetAllProducts()
    {
        var products = _context.Products.ToList();
        _logService.AddLog("Get products", "Show all products");
        return Ok(products);
    }
    //додати продукт
    [HttpPost]
    public IActionResult CreateProduct(Product product)
    {
        // NAME
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Назва не може бути порожньою");
        // PRICE 
        if (product.Price <= 0)
            return BadRequest("Ціна має бути більшою за 0");
        _context.Products.Add(product);
        _context.SaveChanges();
        _logService.AddLog("Create product",$"Product: {product.Name} created");
        return Ok(product);
    }
    //редагувати продукт
    [HttpPut]
    public IActionResult UpdateProduct(Product product)
    {
        var prod = _context.Products.FirstOrDefault(x => x.Id == product.Id);

        if (prod == null)
            return NotFound("Продукт не знайдено");

        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Назва не може бути порожньою");

        if (product.Price < 0)
            return BadRequest("Ціна має бути меньшою за 0");

        // оновлюю продукт
        prod.Name = product.Name;
        prod.Description = product.Description;
        prod.Price = product.Price;
        _context.SaveChanges();
        _logService.AddLog("Updated product",$"Product: {product.Name} updated");
        return Ok(prod);
    }
    //видалити продукт
    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return NotFound("Продукт не знайдено");
        _context.Products.Remove(product);
        _context.SaveChanges();
        _logService.AddLog("Delete product",$"Product: {product.Name} delete");
        return Ok("Продукт видалено");
    }
    
}