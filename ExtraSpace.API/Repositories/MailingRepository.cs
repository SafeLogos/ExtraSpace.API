using ExtraSpace.API.Models;
using ExtraSpace.API.Models.MailingModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace ExtraSpace.API.Repositories
{
    public class MailingRepository : IMailingRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _login;
        private readonly string _password;

        public MailingRepository(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._login = _configuration["MailSettings:Login"];
            this._password = _configuration["MailSettings:Password"];
        }
        public ApiResponse<bool> SendMail(MailModel mail) =>
            ApiResponse<bool>.DoMethod(resp =>
            {
                MailMessage message = new MailMessage(mail.From, mail.To);

                message.Subject = mail.Title;
                message.Body = mail.Body;
                message.BodyEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;
                SmtpClient client = new SmtpClient("smtp.gmail.com", 25); //Gmail smtp    
                System.Net.NetworkCredential basicCredential1 = new
                System.Net.NetworkCredential(_login, _password);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = basicCredential1;
                client.Send(message);
            });
    }

    public interface IMailingRepository
    {
        ApiResponse<bool> SendMail(MailModel mail);
    }
}
