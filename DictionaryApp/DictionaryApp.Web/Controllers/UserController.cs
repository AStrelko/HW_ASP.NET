using DictionaryApp.Core.Helpers;
using DictionaryApp.Core.Models;
using DictionaryApp.Storage;
using Microsoft.AspNetCore.Mvc;

namespace DictionaryApp.Web.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    private readonly DataContext _context;

    public UserController()
    {
        _context = new DataContext();
    }

    // регістрація
    [HttpPost("register")]
    public IActionResult Register(User user)
    {
        var exists = _context.Users.Any(u => u.Email == user.Email);
        if (exists)
            return BadRequest("User already exists");
        user.Password = PasswordHelper.HashPassword(user.Password);
        _context.Users.Add(user);
        _context.SaveChanges();
        return Ok(user);
    }

    // вхід пиймає адресу та пароль
    [HttpPost("login")]
    public IActionResult Login(string email, string password)
    {
        var hashedPassword = PasswordHelper.HashPassword(password);

        var user = _context.Users.FirstOrDefault(u =>
            u.Email == email &&
            u.Password == hashedPassword);

        if (user == null)
            return Unauthorized("Invalid email or password");

        return Ok(user);
    }

    // видаляє по id
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var user = _context.Users.Find(id);//знаходе по id
        if (user == null)// якщо не знайшов
            return NotFound();//404
        var deletedUser = new DeletedUser//створюю новий об'єкт лля запису в таблицю DeletedUser
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = user.Password,
            BirthDate = user.BirthDate,
            CreatedAt = user.CreatedAt
        };
        _context.DeletedUsers.Add(deletedUser);// додаю
        _context.Users.Remove(user);//видоляю
        _context.SaveChanges();//закінчую транзакцію

        return Ok("User deleted");
    }

    //відновити по id в теж саме що і  при видолені
    [HttpPost("restore/{id}")]
    public IActionResult Restore(int id)
    {
        var deletedUser = _context.DeletedUsers.Find(id);
        if (deletedUser == null)
            return NotFound();
        var exists = _context.Users.Any(u => u.Email == deletedUser.Email);
        if (exists)
            return BadRequest("User already exists in active users");
        var user = new User
        {
            FirstName = deletedUser.FirstName,
            LastName = deletedUser.LastName,
            Email = deletedUser.Email,
            Password = deletedUser.Password,
            BirthDate = deletedUser.BirthDate,
            CreatedAt = deletedUser.CreatedAt
        };
        _context.Users.Add(user);
        _context.DeletedUsers.Remove(deletedUser);
        _context.SaveChanges();
        return Ok(user);
    }
    
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _context.Users.ToList();
        return Ok(users);
    }
    
    [HttpGet("deleted")]
    public IActionResult GetDeletedUsers()
    {
        var deletedUsers = _context.DeletedUsers.ToList();
        return Ok(deletedUsers);
    }
    
    //http://localhost:5208/swagger/index.html
}