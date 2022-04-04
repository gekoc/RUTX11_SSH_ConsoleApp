using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUTX11_SSH_ConsoleApp
{
    public interface IConsoleIO
    {
        void WriteLine(string output);
    }

    public class ConsoleIO : IConsoleIO
    {
        public void WriteLine(string output)
        {
            Console.WriteLine(output);
        }

    }
}
