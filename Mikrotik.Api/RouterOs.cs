using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mikrotik.Api
{
    public interface IRouterOs
    {
        IRouterOs Backup();
        IRouterOs Identity(string identity);
        IRouterOs Reboot();
        IRouterOs LoginNotice(string notice);
        IRouterOs Shutdown();        
    }

    public class RouterOs : Connectivity, IRouterOs
    {
        public RouterOs(string mikrotikIp, string userName, string password, int port = 8728) :
            base(mikrotikIp, userName, password, port)
        {

        }

        public IRouterOs Backup()
        {
            EndCommand("/system/backup/save");
            return this;
        }

        public IRouterOs Identity(string identity)
        {
            Command("/system/identity/set");
            EndCommand(String.Format("=name={0}", identity));

            return this;
        }

        public IRouterOs Reboot()
        {
            EndCommand("/system/reboot");
            return this;
        }

        public IRouterOs LoginNotice(string notice)
        {
            Command("/system/note/set");
            Command("=show-at-login=yes");
            EndCommand(String.Format("=note={0}", notice));

            return this;
        }

        public IRouterOs Shutdown()
        {
            EndCommand("/system/shutdown");
            return this;
        }
    }
}
