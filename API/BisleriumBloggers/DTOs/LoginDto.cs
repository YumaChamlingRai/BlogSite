using System.ComponentModel.DataAnnotations;

namespace BisleriumBloggers.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }

    [Required]
    public string Password { get; set; }
}
