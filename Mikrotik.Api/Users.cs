using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mikrotik.Api
{
    public interface IUsers
    {
        IUsers Add(string userName, string password, string group, string allowedIpAddress = "0.0.0.0/0", string comment = null);
        IUsers Remove(string userName);
        IUsers Disable(string userName);
        IUsers Enable(string userName);

        string UserList();
        string ActiveUsers();
    }

    public class Users : Connectivity, IUsers
    {
        public Users(string mikrotikIp, string userName, string password, int port = 8728) :
            base(mikrotikIp, userName, password, port)
        {

        }

        public IUsers Add(string userName, string password, string group, string allowedIpAddress = "0.0.0.0/0", string comment = null)
        {
            Command("/user/add");
            Command(String.Format("=name={0}", userName));
            Command(String.Format("=password={0}", password));
            Command(String.Format("=disabled=no"));
            Command(String.Format("=address={0}", allowedIpAddress));
            Command(String.Format("=group={0}", group));
            EndCommand(String.Format("=comment={0}", comment));

            return this;
        }

        public IUsers Remove(string userName)
        {
            Command("/user/remove");
            EndCommand(String.Format("=numbers={0}", userName));

            return this;
        }

        public string ActiveUsers()
        {
            var result = EndCommand("/user/active/print");
            return result.Message;
        }

        public IUsers Disable(string userName)
        {
            Command("/user/disable");
            EndCommand(String.Format("=numbers={0}", userName));

            return this;
        }

        public IUsers Enable(string userName)
        {
            Command("/user/enable");
            EndCommand(String.Format("=numbers={0}", userName));

            return this;
        }

        public string UserList()
        {
            var result = EndCommand("/user/print");
            return result.Message;
        }
    }
}
