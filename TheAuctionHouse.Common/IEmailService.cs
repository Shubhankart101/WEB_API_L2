public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}

public class EmailService : IEmailService
{
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        //Todo
        await Task.Run(() => { });
    }
}