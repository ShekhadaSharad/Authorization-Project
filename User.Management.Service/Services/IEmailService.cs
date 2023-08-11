using User.Management.Service.Model;

namespace User.Management.Service.Services
{
    internal interface IEmailServices
    {
        void SendEmail(Message message);
    }
}
