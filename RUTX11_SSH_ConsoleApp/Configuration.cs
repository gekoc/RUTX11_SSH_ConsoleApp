using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUTX11_SSH_ConsoleApp
{
    public interface IConfiguration
    {
        string Address { get; }
        int Port { get; }
        string UserName { get; }
        string Password { get; }
    }

    public class Configuration : IConfiguration
    {
        public string Address { get; }
        public int Port { get; }
        public string UserName { get; }
        public string Password { get; }

        public Configuration(string address, int port, string userName, string password)
        {
            Address = address;
            Port = port;
            UserName = userName;
            Password = password;
        }
    }
}
