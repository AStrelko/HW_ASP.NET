using Microsoft.AspNetCore.Mvc;
using OnlineStoreLes02Lab.Core.Models;
using OnlineStoreLes02Lab.Storage;
using System.Text.RegularExpressions;
using OnlineStoreLes02Lab.Web.Services;

namespace OnlineStoreLes02Lab.Web.Controllers;
[ApiController]
[Route("api/v1/orders")]
public class OrderController: ControllerBase
{
    private readonly DataContext _context;
    private readonly LogService _logService;

    public OrderController(DataContext context, LogService logService)
    {
        _context = context;
        _logService = logService;
    }

    [HttpGet]
    public IActionResult GetAllOrders()
    {
        var orders = _context.Orders.ToList();

        var result = orders.Select(o =>
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == o.UserId);
            var product = _context.Products.FirstOrDefault(p => p.Id == o.ProductId);

            return new OrderRespons
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                CustomerName = user?.FullName ?? "Unknown",
                ProductName = product?.Name ?? "Unknown",
                Quantity = o.Quantity,
                TotalSum = (product?.Price ?? 0) * o.Quantity
            };
        });

        _logService.AddLog("Get orders", "Show all orders");

        return Ok(result);
    }

    [HttpPost]
    public IActionResult CreateOrder(int userId, int productId, int quantity)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return NotFound("Користувача не знайдено");

        var product = _context.Products.FirstOrDefault(p => p.Id == productId);
        if (product == null)
            return NotFound("Продукт не знайдено");

        if (quantity <= 0)
            return BadRequest("Кількість має бути більше 0");

        var order = new Order
        {
            UserId = user.Id,
            ProductId = product.Id,
            Quantity = quantity
        };

        _context.Orders.Add(order);
        _context.SaveChanges();

        _logService.AddLog("Create order", $"Order {order.Id} created");

        return Ok(order.Id);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteOrder(int id)
    {
        var order = _context.Orders.FirstOrDefault(o => o.Id == id);
        if(order == null)
            return NotFound("Замовлення не знайдено");
        _context.Orders.Remove(order);
        _context.SaveChanges();
        _logService.AddLog("Delete order", $"Order № {order.Id} deleted ");
        return Ok("Замовлення видалено");
    }
}