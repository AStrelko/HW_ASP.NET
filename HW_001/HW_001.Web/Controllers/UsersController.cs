using Microsoft.AspNetCore.Mvc;// підключення інструментів для роботи з API-контролерами

namespace HW001.Web.Controllers;

[ApiController]// атрибут позначає клас як API-контролер
[Route("api/users")]// задає маршрут (URL) для контролера
public class UsersController: ControllerBase
{
    private static List<User> _users = new List<User>
    {
        new User(1, "Ivan", "Ivanov", 25),
        new User(2, "Boby", "Smeath", 18),
        new User(3, "Anny", "Goold", 29)
    };
    
    //GET http://localhost:{port}/api/users
    [HttpGet]// HTTP GET запит для отримання даних всіх обєктів(користувачів)
    public User[] GetUsers() => _users.ToArray();//повертає список перетворений в масив
    
    //POST http://localhost:{port}/api/users

    [HttpPost]// HTTP POST запит для додавання нового користувача
    public IActionResult AddUser(User user)
    {
        _users.Add(user);//додає новий обєкт
        return Ok(user);// повертає відповідь клієнту про результат операції
    }
    
    //PUT http://localhost:{port}/api/users/1

    [HttpPut("{id}")]// Put - зміна всіх данних обєкту
    public IActionResult UpdateUser(int id, User updatedUser)
    {
        //cтвор. обєкт шукає в списку перший знайдений по Id
        var user = _users.FirstOrDefault(p => p.Id == id);
        if (user == null)//перевірка якщо обєкт не існує
        {
            return NotFound();// повертає статус (404 Not Found)- "Обєкт не знайден"
        }
        _users.Remove(user);// видаляємо старий об'єкт
        _users.Add(updatedUser);// додаємо оновлений об'єкт

        return Ok(updatedUser);// повертає статус 200 та оновлений обєкт
    }
    
    //Delete http://localhost:{port}/api/users/1
    
    [HttpDelete("{id}")]//видалення об. за Id
    public IActionResult DeleteUser(int id)
    {
        var user = _users.FirstOrDefault(p => p.Id == id);

        if (user == null)//перевірка якщо обєкт не існує
        {
            return NotFound();// повертає статус 
        }

        _users.Remove(user);// видаляємо об'єкт
        return Ok();//повертає статус
    }
    // данні класу
    public record User(int Id, string Name, string LastName, int Age);
}