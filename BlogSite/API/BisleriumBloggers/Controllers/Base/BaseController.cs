using Microsoft.AspNetCore.Mvc;

namespace BisleriumBloggers.Controllers.Base
{
    public class BaseController<T> : Controller where T : BaseController<T>
    {
    }
}

