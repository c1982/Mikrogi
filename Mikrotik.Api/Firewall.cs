using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mikrotik.Api
{
    public interface IFirewall
    {
        IFirewall Connect(string mikrotikIp, string userName, string Password, int mikrotikPort = 8728);
        IFirewall Drop();
        IFirewall Accept();
        IFirewall Jump(Targets jumpTarget);        

        IFirewall SourceIp(string Ip, int port = 0);
        IFirewall DestinationIp(string Ip, int port = 0);
        IFirewall Protocol(Protocols protocol);
        IFirewall Comment(string comment);
        IFirewall Chain(Targets chain);
        
        Response Apply();
    }

    public class Firewall : IDisposable, IFirewall
    {
        private MK mikrotik;

        private Actions _action;
        private string _sourceIp;
        private string _destinationIp;
        private int _sourcePort;
        private int _destinationPort;
        private string _comment;
        private Targets _jumpTarget;
        private Protocols _protocol;
        private Targets _chain;

        public IFirewall Drop()
        {
            this._action = Actions.drop;            
            return this;
        }

        public IFirewall Accept()
        {
            this._action = Actions.accept;
            return this;
        }

        public IFirewall Jump(Targets jumpTarget)
        {
            this._action = Actions.jump;
            this._jumpTarget = jumpTarget;

            return this;
        }

        public IFirewall SourceIp(string Ip, int port = 0)
        {
            this._sourceIp = Ip;

            if (port != 0)
                this._sourcePort = port;

            return this;
        }

        public IFirewall DestinationIp(string Ip, int port = 0)
        {
            this._destinationIp = Ip;

            if(port != 0)
                this._destinationPort = port;

            return this;
        }

        public IFirewall Protocol(Protocols protocol)
        {
            this._protocol = protocol;
            return this;
        }

        public IFirewall Comment(string comment)
        {
            this._comment = comment;
            return this;
        }

        public Response Apply()
        {
            mikrotik.Send("/ip/firewall/filter/add");
            mikrotik.Send(String.Format("=action={0}", _action));
            mikrotik.Send(String.Format("=chain={0}", _chain));

            if(_destinationPort != 0)
                mikrotik.Send(String.Format("=dst-port={0}", _destinationPort));            
            
            mikrotik.Send(String.Format("=protocol={0}",_protocol));

            if(!String.IsNullOrEmpty(_sourceIp))
                mikrotik.Send(String.Format("=src-address={0}", _sourceIp));

            mikrotik.Send(".tag=firewall", true);

            var message = readResponse();

            return new Response() { Status = true, Message = message };
        }

        public IFirewall Connect(string mikrotikIp, string userName, string Password, int mikrotikPort = 8728)
        {
            mikrotik = new MK(mikrotikIp, mikrotikPort);
            if (!mikrotik.Login(userName, Password))
                throw new System.Exception("Login incorrect!");
            
            return this;
        }

        public IFirewall Chain(Targets chain)
        {
            this._chain = chain;
            return this;
        }

        public void Dispose()
        {
            mikrotik.Close();
        }

        #region Privates
        private string readResponse()
        {
            StringBuilder sbuilt = new StringBuilder();

            foreach (string h in mikrotik.Read())
            {
                sbuilt.AppendLine(h);
            }

            return sbuilt.ToString();
        }
        #endregion
    }
}
