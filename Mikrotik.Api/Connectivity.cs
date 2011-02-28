using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mikrotik.Api
{
    public class Connectivity
    {
        private MK mikrotik;

        public Connectivity(string ip, string userName, string password, int port = 8728)
        {
            mikrotik = new MK(ip,port);

            if (!mikrotik.Login(userName, password))
                throw new System.Exception("Login incorrect!");
        }

        public void Command(string sentence)
        {
                mikrotik.Send(sentence);
        }

        ~Connectivity()
        {
            mikrotik.Close();
        }

        public Response EndCommand(string sentence)
        {
            mikrotik.Send(sentence,true);
            var _response = readResponse();

            return new Response() { Status = _response.StartsWith("!done"), Message = _response };
        }

        private string readResponse()
        {            
            StringBuilder sbuilt = new StringBuilder();

            foreach (string h in mikrotik.Read())
            {
                sbuilt.AppendLine(h);
            }

            return sbuilt.ToString();
        }
    }
}
