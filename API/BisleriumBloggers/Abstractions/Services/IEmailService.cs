using BisleriumBloggers.DTOs;

namespace BisleriumBloggers.Abstractions.Services
{
    public interface IEmailService
    {
        void SendEmail(EmailDto email);
    }
}

