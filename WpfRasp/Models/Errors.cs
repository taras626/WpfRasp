using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRasp.Models
{
    internal static class Errors
    {
        public static void ShowErrorConsole(Exception e) 
        {
            Console.WriteLine(e.Message);
        }
    }
}
