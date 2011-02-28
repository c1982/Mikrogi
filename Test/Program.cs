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
            //var variable = new Firewall("192.168.2.8","admin","")
            //                .Chain(Targets.forward)
            //                .Accept()
            //                .SourceIp("178.18.193.21")
            //                .Protocol(Protocols.tcp)
            //                .Comment("Test Comment")
            //                .Apply();



            //var variable = new RouterOs("192.168.2.8", "admin", "");

            //Console.WriteLine(variable.Status);
            //Console.WriteLine(variable.Message);


            var variable = new Users("192.168.2.8", "admin", "").UserList();
            Console.WriteLine(variable);

            Console.Read();
        }
    }
}
