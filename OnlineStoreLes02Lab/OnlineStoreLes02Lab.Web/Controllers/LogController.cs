using Microsoft.AspNetCore.Mvc;
using OnlineStoreLes02Lab.Core.Models;
using OnlineStoreLes02Lab.Storage;
using System.Text.RegularExpressions;

namespace OnlineStoreLes02Lab.Web.Controllers;

[ApiController]
[Route("api/v1/logs")]

public class LogController: ControllerBase
{
    private readonly DataContext _context;

    public LogController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAllLogs()
    {
        var logs = _context.Logs.ToList();
        return Ok(logs);
    }
}