using less_06.App.Models;
using less_06.App.Services;
using Microsoft.AspNetCore.Mvc;
namespace less_06.App.Controllers;

[ApiController]
[Route("api/products")]
public class ProductControllers : ControllerBase
{
   private readonly ProductServices _productServices;

   public ProductControllers(ProductServices productServices)
   {
      _productServices = productServices;
   }

   //Get api/products?Limit=10
   [HttpGet]
   public async Task<IActionResult> GetProducts([FromQuery] ProductsFilter filter, CancellationToken cancellationToken)
   {
      
      var products = await _productServices.GetAll(filter, cancellationToken);
      return Ok(products);
   }
   
   //Get aps/products/12
   [HttpGet("{id}")]
   public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken cancellationToken)
   {
      var product = await _productServices.GetById(id, cancellationToken);
      
      if(product is null)
         return NotFound();
      
      return Ok(product);
   }
   
   //
   [HttpPost]
   public async Task<IActionResult> AddProduct([FromBody] Product product, CancellationToken cancellationToken)
   {
      var addedProduct = await _productServices.Add(product, cancellationToken);
      return CreatedAtRoute(nameof(GetById), new[] { addedProduct.Id }, addedProduct); // 201 Created
   }

   [HttpPut("{id}")] // повне оновлення
   public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] Product updataproduct,
      CancellationToken cancellationToken)
   {
      var product = await _productServices.Update(id, updataproduct, cancellationToken);
      
      return Ok(product);
   }

   [HttpDelete("{id}")]
   public async Task<IActionResult> DeleteProduct([FromRoute] int id, CancellationToken cancellationToken)
   {
      await _productServices.Delete(id, cancellationToken);
      return NoContent();
   }
   
   [HttpPost("restore")]
   public async Task<IActionResult> Restore(
      CancellationToken cancellationToken)
   {
      await _productServices.Restore(cancellationToken);

      return NoContent();
   }
}