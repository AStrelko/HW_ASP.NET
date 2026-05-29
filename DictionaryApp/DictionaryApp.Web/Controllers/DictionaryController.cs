using DictionaryApp.Core.Models;
using DictionaryApp.Storage;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryApp.Web.Controllers;

//https://localhost:5000/api/v1/dictionaries

[ApiController]//позначаю як контроллер
[Route("api/v1/dictionaries")]//позначаю базовий шлях до контроллера
public class DictionaryController: ControllerBase
{
    private readonly DataContext _context;

    public DictionaryController()//_context отримую з конструктора
    {
        _context = new DataContext();
    }

    [HttpGet]
    public IActionResult Get()
    {
       var dictionaries = _context.Dictionaries.ToList();
        return Ok(dictionaries);//повертає статус
    }
    
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var dictionary = _context.Dictionaries.Find(id);
        return Ok(dictionary);//повертає статус
    }

    [HttpPost]
    public IActionResult Post(DictionaryItem dictionaryItem)
    {
        _context.Dictionaries.Add(dictionaryItem);//додаю об'єкт класу
        _context.SaveChanges();//завершую транзакцію
        return Ok();// повертаю статус
    }

    [HttpPut]
    public IActionResult Put(DictionaryItem dictionaryItem)
    {
        _context.Update(dictionaryItem);
        _context.SaveChanges();
        return Ok();
    }
    
    //https://localhost:5000/api/v1/dictionaries/1
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var entity = _context.Dictionaries.Find(id);//знаходим об'єкт по id
        if (entity == null)
        {
            return NotFound();
        }
        _context.Remove(entity);// видоляю єлемент
        _context.SaveChanges();// закінчую транзакцію
        return Ok();
    }
    
}
