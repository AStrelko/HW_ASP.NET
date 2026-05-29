using Microsoft.AspNetCore.Mvc;
using OnlineStoreLes02Lab.Core.Models;
using OnlineStoreLes02Lab.Storage;
using System.Text.RegularExpressions;
using OnlineStoreLes02Lab.Web.Services;

namespace OnlineStoreLes02Lab.Web.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    private readonly DataContext _context;
    private readonly LogService _logService;

    public UserController(DataContext context, LogService logService)
    {
        _context = context;
        _logService = logService;
    }
    
    //show users
    [HttpGet]
    public IActionResult GetAllUsers()
    {
       var users = _context.Users.ToList();
       _logService.AddLog("Get users", "Show all users");
        return Ok(users);
    }

    // регистрация
    [HttpPost("register")]
    public IActionResult Register(User user)
    {
        // перевірка імені
        if (string.IsNullOrWhiteSpace(user.FirstName))
            return BadRequest("Ім'я не може бути порожнім");
        // перевірка прізвища
        if (string.IsNullOrWhiteSpace(user.LastName))
            return BadRequest("Прізвище не може бути порожнім");
        // перевірка email
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(user.Email, emailPattern))
            return BadRequest("Невірний формат email");
        var exists = _context.Users.Any(u => u.Email == user.Email);
        if (exists)
            return BadRequest("Користувач з таким email вже існує");
        // перевірка пароля
        if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 6)
            return BadRequest("Пароль повинен містити не менше 6 символів");
        // перевірка існування користувача
        var ex = _context.Users.Any(u => u.Email == user.Email);
        if (ex)
            return BadRequest("Користувач з таким email вже існує");
        // хешування пароля
        user.Password = PasswordHelper.HashPassword(user.Password);
        _context.Users.Add(user);
        _context.SaveChanges();
        _logService.AddLog("Register user", $"Registered user {user.FullName}");
        return Ok(user);
    }

    // логин
    [HttpPost("login")]
    public IActionResult Login(string email, string password)
    {
        var hashedPassword = PasswordHelper.HashPassword(password);

        var user = _context.Users.FirstOrDefault(u =>
            u.Email == email &&
            u.Password == hashedPassword);

        if (user == null)
            return Unauthorized("Не вірний пароль або адрес");
        _logService.AddLog("Login", $"Logged in {user.FullName}");
        return Ok(user);
    }
    
    //редагування
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, User dto)
    {
        var user = _context.Users.FirstOrDefault(x => x.Id == id);

        if (user == null)
            return NotFound("Користувача не знайдено");

        // FIRST NAME
        if (string.IsNullOrWhiteSpace(dto.FirstName) || dto.FirstName == "string")
            return BadRequest("Ім'я не може бути порожнім");

        // LAST NAME
        if (string.IsNullOrWhiteSpace(dto.LastName) || dto.LastName == "string")
            return BadRequest("Прізвище не може бути порожнім");

        // EMAIL
        if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string")
            return BadRequest("Email не може бути порожнім");

        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(dto.Email, emailPattern))
            return BadRequest("Некоректний email");
        var exists = _context.Users.Any(u => u.Email == dto.Email && u.Id != id);
        if (exists)
            return BadRequest("Користувач з таким email вже існує");

        // PASSWORD
        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password == "string")
            return BadRequest("Пароль не може бути порожнім");

        if (dto.Password.Length < 6)
            return BadRequest("Пароль має бути мінімум 6 символів");

        // якщо все доре оновлюю
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.Password = PasswordHelper.HashPassword(dto.Password);
        user.BirthDate = dto.BirthDate;

        _context.SaveChanges();
        _logService.AddLog("Update user", $"Updated {user.FullName}");
        return Ok(user);
    }
    
    //видоляю
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return NotFound("Клієнт не знайден");
        _context.Users.Remove(user);
        _context.SaveChanges();
        _logService.AddLog("Delete user", $"Deleted {user.FullName}");
        return Ok("Клієнт видалений");
    }
    
    //http://localhost:5151/swagger/index.html
}

//в NuGet додаю Swagger для зручного тестування
//Swashbuckle.AspNetCore.SwaggerUI в .Web
//Swashbuckle.AspNetCore.Swagger в .Web
//Swashbuckle.AspNetCore.SwaggerGen в .Web