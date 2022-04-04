using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace RUTX11_SSH_ConsoleApp
{
    public interface IApp
    {
        void Run();
    }

    public class App
    {
        private IUserLog _userLog;
        private IConsoleIO _console;
        private IRouter _router;

        public App(IUserLog userLog, IConsoleIO console, IRouter router)
        {
            _userLog = userLog;
            _console = console;
            _router = router;
        }

        private User? _currentUser;
        private Attempt? _currentAttempt;

        public void Run()
        {
            _currentUser = GetAuthenticatedUser();
            var config = _userLog.GetConfiguration(_currentUser);
            do
            {
                _currentAttempt = _userLog.SaveAttempt(_currentUser);
                var task = GetTaskSelection();
                ExecuteTask(task, config, _currentUser, _currentAttempt);
            } while (true);
        }

        public User GetAuthenticatedUser()
        {
            Console.WriteLine("1. Create New User");
            Console.WriteLine("2. Choose Existing User");
            var input = Console.ReadLine();
            var inputValue = 0;
            while (!int.TryParse(input, out inputValue))
            {
                Console.WriteLine("Choose one of the two options:");
                Console.WriteLine("1. Create New User");
                Console.WriteLine("2. Choose Existing User");
                input = Console.ReadLine();
            }
            if (inputValue == 1)
            {
                CreateUser();
            }
            var userList = _userLog.GetAllUsers();
            Console.WriteLine("List of Users:");
            foreach (var item in userList)
            {
                Console.WriteLine($"{item.Id}. {item.Name}");
            }
            var userSelection = Console.ReadLine();
            var userId = 0;
            while (!int.TryParse(userSelection, out userId))
            {
                Console.WriteLine("User Selection not recognized. Please select a user:");
                foreach (var item in userList)
                {
                    Console.WriteLine($"{item.Id}. {item.Name}");
                }
                userSelection = Console.ReadLine();
            }
            var user = GetUser(userList, userId);
            user = AuthenticateUser(user, userId);
            return user;
        }

        public User AuthenticateUser(User user, int userId)
        {
            if (!_router.CanConnectToRouter(user))
            {
                var attempt = _userLog.SaveAttempt(user);
                _userLog.LogAuthenticationError(attempt);
                do
                {
                    Console.WriteLine("Could not connect to the router. Are you sure you entered correct user details?");
                    var userList = _userLog.GetAllUsers();
                    Console.WriteLine("List of Users:");
                    foreach (var item in userList)
                    {
                        Console.WriteLine($"{item.Id}. {item.Name}");
                    }
                    var newUserId = Console.ReadLine();
                    while (!int.TryParse(newUserId, out userId))
                    {
                        Console.WriteLine("Choose one of the two options:");
                        Console.WriteLine("1. Create New User");
                        Console.WriteLine("2. Choose Existing User");
                        newUserId = Console.ReadLine();
                    }
                    user = GetUser(userList, userId);
                } while (!_router.CanConnectToRouter(user));
                return user;
            }
            Console.WriteLine("Router Connection Established");
            return user;
        }

        public void ExecuteTask(int task, IConfiguration config, User user, Attempt attempt)
        {
            switch (task)
            {
                case 1:
                    Console.WriteLine("Signal Strength Check Will Start Now");
                    System.Threading.Thread.Sleep(2000);
                    _router.GetSignalStrength(config, user, attempt);
                    break;

                case 2:
                    Console.WriteLine("List of Connected Clients Will Be Listed:");
                    System.Threading.Thread.Sleep(2000);
                    _router.GetConnectedDevices(config);
                    break;

                case 3:
                    Console.WriteLine("Please Wait While We Get Ready To Send SMS");
                    System.Threading.Thread.Sleep(2000);
                    _router.SendSMS(config, user, attempt);
                    break;
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Task Ended.");
            GetSessionReport(user, attempt);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Press Enter to choose another task.");
            Console.ReadLine();

        }

        public int GetTaskSelection()
        {
            Console.WriteLine("What do you want to do?");
            Console.WriteLine("1. Check Mobile Signal Strenth");
            Console.WriteLine("2. Get Connected Devices");
            Console.WriteLine("3. Send SMS");
            var taskInput = Console.ReadLine();
            var taskValue = 0;
            while (!int.TryParse(taskInput, out taskValue))
            {
                Console.WriteLine("Incorrect input.");
                Console.WriteLine("What do you want to do?");
                Console.WriteLine("1. Check Mobile Signal Strenth");
                Console.WriteLine("2. Get Connected Devices");
                Console.WriteLine("3. Send SMS");
                taskInput = Console.ReadLine();
            }
            return taskValue;
        }

        public User GetUser(IEnumerable<User> userList, int userId)
        {
            return userList.SingleOrDefault(u => u.Id == userId);
        }

        public void CreateUser()
        {
            Console.WriteLine("User Name:");
            var userName = Console.ReadLine();
            Console.WriteLine("Password:");
            var password = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && password.Length > 0)
                {
                    Console.Write("\b \b");
                    password = password[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    password += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            Console.WriteLine();
            Console.WriteLine("Target IP:");
            var ip = Console.ReadLine();
            Console.WriteLine("Target Port:");
            var portString = Console.ReadLine();
            var port = 0;
            while (!int.TryParse(portString, out port))
            {
                Console.WriteLine("Incorrect input.");
                Console.WriteLine("Target Port:");
                portString = Console.ReadLine();
            }

            var user = new User
            {
                Name = userName,
                Password = password,
                Ip = ip,
                Port = port
            };
            _userLog.SaveUser(user);
        }

        public void GetSessionReport(User user, Attempt attempt)
        {
            var attempts = _userLog.GetUserAttempts(user);
            var errors = _userLog.GetAttemptErrors(attempt);
            Console.WriteLine("Task report:");
            Console.WriteLine($"Total task attempts made by the user {user.Name} this session: {attempts.Count()}");
            Console.WriteLine($"Current attempt got {errors.Count()} errors.");
            if (errors.Count() > 0)
            {
                Console.WriteLine("The errors ecountered are:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"{error.Message}");
                }
            }
        }
    }
}
