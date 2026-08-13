namespace ECommerce.Business.Services.IServices
{
	public interface IEmailSenderService
	{
		Task SendEmailAsync(string toEmail, string subject, string htmlMessage);

	}
}
