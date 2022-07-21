using Microsoft.AspNetCore.Mvc;

namespace proxy_rest.Controllers;

[ApiController]
[Route("[controller]")]
public class CommandController : ControllerBase
{
    private readonly ILogger<CommandController> _logger;

    public CommandController(ILogger<CommandController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "RelexTest")]
    public string Command(string command)
    {
        switch (command)
        {
            case "REFLEX":
                return "Reflex Command Received";
            case "SW_UPDATE_AVAILABLE":
                return "SW_UPDATE_AVAILABLE Command Received";
            case "TDEF_AVAILABLE":
                return "TDEF_AVAILABLE Command Received";
            case "HELP_FILE_AVAILABLE":
                return "HELP_FILE_AVAILABLE Command Received";
        }
        return "Unknown command received";
        
    }
}

