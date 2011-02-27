using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mikrotik.Api;

namespace Usage
{
    class Program
    {
        static void Main(string[] args)
        {

            var blockIp = new Firewall()
                            .Connect("127.0.0.1","admin","")
                            .Chain(Targets.forward)
                            .Drop()
                            .SourceIp("178.18.193.21")
                            .Protocol(Protocols.tcp)
                            .Comment("Test Comment")
                            .Apply();

            if (blockIp.Status)
                Console.WriteLine("Command Succesful");
            else
                Console.WriteLine(blockIp.Message);
       
        }
    }
}
