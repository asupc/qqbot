using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace QQBot.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("Any")]
    [Authorize]
    public class BaseController : ControllerBase
    {

    }
}
