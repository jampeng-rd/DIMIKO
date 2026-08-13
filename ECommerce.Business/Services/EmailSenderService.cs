using ECommerce.Business.Services.IServices;
using ECommerce.Utility.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ECommerce.Business.Services
{
	public class EmailSenderService : IEmailSenderService
	{
		private readonly GmailSmtpSettings _settings;

		public EmailSenderService(IOptions<GmailSmtpSettings> options)
		{
			_settings = options.Value;
		}

		public async Task SendEmailAsync(
			string toEmail,
			string subject,
			string htmlMessage)
		{
			var email = new MimeMessage();

			email.From.Add(new MailboxAddress("DIMIKO", _settings.Username));

			email.To.Add(MailboxAddress.Parse(toEmail));

			email.Subject = subject;

			email.Body = new TextPart("html")
			{
				Text = htmlMessage
			};


			using var smtpClient = new SmtpClient();

			await smtpClient.ConnectAsync(
				_settings.Host,
				_settings.Port,
				SecureSocketOptions.StartTls);

			await smtpClient.AuthenticateAsync(_settings.Username, _settings.AppPassword);

			await smtpClient.SendAsync(email);

			await smtpClient.DisconnectAsync(true);
		}


	}
}
