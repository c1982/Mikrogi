using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mikrotik.Api
{
    public interface IFirewall
    {        
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

    public class Firewall : Connectivity, IFirewall
    {
        private Actions _action;
        private string _sourceIp;
        private string _destinationIp;
        private int _sourcePort;
        private int _destinationPort;
        private string _comment;
        private Targets _jumpTarget;
        private Protocols _protocol;
        private Targets _chain;


        public Firewall(string mikrotikIp, string userName, string password, int port = 8728) : 
            base(mikrotikIp, userName, password, port)
        {

        }

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

        public IFirewall Chain(Targets chain)
        {
            this._chain = chain;
            return this;
        }

        public Response Apply()
        {
            Command("/ip/firewall/filter/add");
            Command(String.Format("=action={0}", _action));
            Command(String.Format("=chain={0}", _chain));
            Command(String.Format("=protocol={0}", _protocol));
            Command(String.Format("=comment={0}", _comment));

            if (_destinationPort != 0)
                Command(String.Format("=dst-port={0}", _destinationPort));

            if (_sourcePort != 0)
                Command(String.Format("=src-port={0}", _sourcePort));

            if (!String.IsNullOrEmpty(_sourceIp))
                Command(String.Format("=src-address={0}", _sourceIp));

            if (!String.IsNullOrEmpty(_destinationIp))
                Command(String.Format("=dst-address={0}", _destinationIp));

            var _response = EndCommand(".tag=firewall");

            return _response;
        }
    }
}
