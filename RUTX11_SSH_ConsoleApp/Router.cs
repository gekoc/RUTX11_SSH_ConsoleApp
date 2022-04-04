using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace RUTX11_SSH_ConsoleApp
{
    public interface IRouter
    {
        bool CanConnectToRouter(User user);
        void GetSignalStrength(IConfiguration config, User user, Attempt attempt);
        void GetConnectedDevices(IConfiguration config);
        void SendSMS(IConfiguration config, User user, Attempt attempt);

    }


    public class Router : IRouter
    {

        private IUserLog _userLog;
        private IConsoleIO _console;

        public Router(IUserLog userLog, IConsoleIO console)

        {
            _userLog = userLog;
            _console = console;
        }

        public ConnectionInfo GetConnectionInfo(IConfiguration configuration)
        {
            return new ConnectionInfo(configuration.Address, configuration.Port, configuration.UserName, new AuthenticationMethod[] { new PasswordAuthenticationMethod(configuration.UserName, configuration.Password) });
        }

        public bool CanConnectToRouter(User user)
        {

            var connNfo = GetConnectionInfo(_userLog.GetConfiguration(user));
            using (var sshclient = new SshClient(connNfo))
            {
                try
                {
                    sshclient.Connect();
                }
                catch (Renci.SshNet.Common.SshAuthenticationException)
                {
                    return false;
                }
                finally
                {
                    sshclient.Disconnect();
                }

                return true;
            }
        }

        public string ExecuteCommand(IConfiguration configuration, string command)
        {
            var connectionInfo = GetConnectionInfo(configuration);
            var result = string.Empty;
            using (var sshclient = new SshClient(connectionInfo))
            {
                sshclient.Connect();
                using (var cmd = sshclient.CreateCommand(command))
                {
                    result = cmd.Execute();
                }
                sshclient.Disconnect();
            }
            return result;
        }

        public void GetSignalStrength(IConfiguration config, User user, Attempt attempt)
        {
            var result = "";
            var retry = 0;
            while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                Console.Clear();
                result = ExecuteCommand(config, "gsmctl -q");
                Console.WriteLine(result);
                System.Threading.Thread.Sleep(500);
                if (result == "N/A\n")
                {
                    retry++;

                    if (retry >= 5)
                    {
                        _userLog.LogSignalLostError(retry, attempt);
                        break;
                    }
                }
                else
                {
                    retry = 0;
                }
            }
        }


        public void GetConnectedDevices(IConfiguration config)
        {
            Console.Clear();
            Console.WriteLine("Connected Devices Are:");
            var result = ExecuteCommand(config, "cat /tmp/dhcp.leases");
            string[] lines = result.Split(new string[] { "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var index = lines[i].IndexOf("192.168.10.");
                var devices = lines[i].Substring(index, 27);
                Console.WriteLine(devices);
            }
        }


        public void SendSMS(IConfiguration config, User user, Attempt attempt)
        {
            Console.Clear();
            Console.WriteLine("Enter Phone Number To Send SMS (00370 ...) ");
            var phoneNumber = Console.ReadLine();
            Console.WriteLine("Enter the message to send");
            var message = Console.ReadLine();
            var command = $"gsmctl -S -s \"{phoneNumber} {message}\"";
            var result = ExecuteCommand(config, command);
            if (result == "OK\n")
            {
                Console.WriteLine("If you entered the correct number, Message should be sent");
            }
            else
            {
                Console.WriteLine("Something happened while trying to send the message");
                _userLog.LogSmsError(attempt);
            }
        }
    }
}
