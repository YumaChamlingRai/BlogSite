namespace Bislerium.Models.Response;

public class LoginResponseDto
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Username { get; set; }
    
    public string ImageUrl { get; set; }

    public int RoleId { get; set; }
    
    public string Role { get; set; }
    
    public string EmailAddress { get; set; }
    
    public string Token { get; set; }
}