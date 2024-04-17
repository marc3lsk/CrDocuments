using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    [HttpGet]
    public object Get()
        => new Document("1", ["a"], (int[])[1, 2, 3, 4]);
}
