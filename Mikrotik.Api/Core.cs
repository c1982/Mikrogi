using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Mikrotik.Api
{
    public class MK : IDisposable
    {
        Stream connection;
        TcpClient con;

        public MK(string ip)
        {
            con = new TcpClient();
            con.Connect(ip, 8728);
            connection = (Stream)con.GetStream();
        }

        public MK(string ip, int port)
        {
            con = new TcpClient();
            con.Connect(ip, port);
            connection = (Stream)con.GetStream();
        }

        public MK(string ip, string userName, string password)
        {
            con = new TcpClient();
            con.Connect(ip, 8728);
            connection = (Stream)con.GetStream();
            ctorLogin(userName, password);
        }

        public void Close()
        {
            connection.Close();
            con.Close();
        }

        public void ctorLogin(string username, string password)
        {
            Send("/login", true);
            string hash = Read()[0].Split(new string[] { "ret=" }, StringSplitOptions.None)[1];
            Send("/login");
            Send("=name=" + username);
            Send("=response=00" + EncodePassword(password, hash), true);            
        }

        public bool Login(string username, string password)
        {
            Send("/login", true);
            string hash = Read()[0].Split(new string[] { "ret=" }, StringSplitOptions.None)[1];
            Send("/login");
            Send("=name=" + username);
            Send("=response=00" + EncodePassword(password, hash), true);
            if (Read()[0] == "!done")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Send(string co)
        {
            byte[] bajty = Encoding.ASCII.GetBytes(co.ToCharArray());
            byte[] velikost = EncodeLength(bajty.Length);

            connection.Write(velikost, 0, velikost.Length);
            connection.Write(bajty, 0, bajty.Length);
        }

        public void Send(string co, bool endsentence)
        {
            byte[] bajty = Encoding.ASCII.GetBytes(co.ToCharArray());
            byte[] velikost = EncodeLength(bajty.Length);
            connection.Write(velikost, 0, velikost.Length);
            connection.Write(bajty, 0, bajty.Length);
            connection.WriteByte(0);
        }

        public List<string> Read()
        {
            List<string> output = new List<string>();
            string o = "";
            byte[] tmp = new byte[4];
            long count;
            while (true)
            {
                tmp[3] = (byte)connection.ReadByte();
                //if(tmp[3] == 220) tmp[3] = (byte)connection.ReadByte(); it sometimes happend to me that 
                //mikrotik send 220 as some kind of "bonus" between words, this fixed things, not sure about it though
                if (tmp[3] == 0)
                {
                    output.Add(o);
                    if (o.Substring(0, 5) == "!done")
                    {
                        break;
                    }
                    else
                    {
                        o = "";
                        continue;
                    }
                }
                else
                {
                    if (tmp[3] < 0x80)
                    {
                        count = tmp[3];
                    }
                    else
                    {
                        if (tmp[3] < 0xC0)
                        {
                            int tmpi = BitConverter.ToInt32(new byte[] { (byte)connection.ReadByte(), tmp[3], 0, 0 }, 0);
                            count = tmpi ^ 0x8000;
                        }
                        else
                        {
                            if (tmp[3] < 0xE0)
                            {
                                tmp[2] = (byte)connection.ReadByte();
                                int tmpi = BitConverter.ToInt32(new byte[] { (byte)connection.ReadByte(), tmp[2], tmp[3], 0 }, 0);
                                count = tmpi ^ 0xC00000;
                            }
                            else
                            {
                                if (tmp[3] < 0xF0)
                                {
                                    tmp[2] = (byte)connection.ReadByte();
                                    tmp[1] = (byte)connection.ReadByte();
                                    int tmpi = BitConverter.ToInt32(new byte[] { (byte)connection.ReadByte(), tmp[1], tmp[2], tmp[3] }, 0);
                                    count = tmpi ^ 0xE0000000;
                                }
                                else
                                {
                                    if (tmp[3] == 0xF0)
                                    {
                                        tmp[3] = (byte)connection.ReadByte();
                                        tmp[2] = (byte)connection.ReadByte();
                                        tmp[1] = (byte)connection.ReadByte();
                                        tmp[0] = (byte)connection.ReadByte();
                                        count = BitConverter.ToInt32(tmp, 0);
                                    }
                                    else
                                    {
                                        //Error in packet reception, unknown length
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    o += (Char)connection.ReadByte();
                }
            }
            return output;
        }

        private byte[] EncodeLength(int delka)
        {
            if (delka < 0x80)
            {
                byte[] tmp = BitConverter.GetBytes(delka);
                return new byte[1] { tmp[0] };
            }
            if (delka < 0x4000)
            {
                byte[] tmp = BitConverter.GetBytes(delka | 0x8000);
                return new byte[2] { tmp[1], tmp[0] };
            }
            if (delka < 0x200000)
            {
                byte[] tmp = BitConverter.GetBytes(delka | 0xC00000);
                return new byte[3] { tmp[2], tmp[1], tmp[0] };
            }
            if (delka < 0x10000000)
            {
                byte[] tmp = BitConverter.GetBytes(delka | 0xE0000000);
                return new byte[4] { tmp[3], tmp[2], tmp[1], tmp[0] };
            }
            else
            {
                byte[] tmp = BitConverter.GetBytes(delka);
                return new byte[5] { 0xF0, tmp[3], tmp[2], tmp[1], tmp[0] };
            }
        }

        private string EncodePassword(string Password, string hash)
        {
            byte[] hash_byte = new byte[hash.Length / 2];
            for (int i = 0; i <= hash.Length - 2; i += 2)
            {
                hash_byte[i / 2] = Byte.Parse(hash.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            byte[] heslo = new byte[1 + Password.Length + hash_byte.Length];
            heslo[0] = 0;
            Encoding.ASCII.GetBytes(Password.ToCharArray()).CopyTo(heslo, 1);
            hash_byte.CopyTo(heslo, 1 + Password.Length);

            Byte[] hotovo;
            System.Security.Cryptography.MD5 md5;

            md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

            hotovo = md5.ComputeHash(heslo);

            //Convert encoded bytes back to a 'readable' string
            string navrat = "";
            foreach (byte h in hotovo)
            {
                navrat += h.ToString("x2");
            }
            return navrat;
        }

        public void Dispose()
        {
            connection.Close();
            con.Close();
        }
    }
}
