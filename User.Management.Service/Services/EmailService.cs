using MimeKit;
using MailKit.Net.Smtp;
using User.Management.Service.Model;

namespace User.Management.Service.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly EmailConfigration _emailConfig;

        public EmailServices(EmailConfigration emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
        }
        public MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text= message.Content };

            return emailMessage;
        }
        private void Send(MimeMessage mailmessage)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfig.UserName,_emailConfig.Password);

                client.Send(mailmessage);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
