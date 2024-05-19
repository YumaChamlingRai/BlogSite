using BisleriumBloggers.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace BisleriumBloggers.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ImageUploadController(IWebHostEnvironment webHostEnvironment) : BaseController<ImageUploadController>
    {
        [HttpPost]
        public IActionResult UploadImage([FromForm] List<IFormFile> files)
        {
            var imageUrls = files.Select(UploadFile).ToList();

            return Ok(imageUrls);
        }
        
        [NonAction]
        private string UploadFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);

            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        
            var fileName = $"{file.FileName}_{timeStamp}.{extension}";

            using var stream = new FileStream(Path.Combine(webHostEnvironment.WebRootPath, fileName), FileMode.Create);
            
            file.CopyTo(stream);
            
            return fileName;
        }
    }
}

