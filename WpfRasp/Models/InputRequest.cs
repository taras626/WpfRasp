using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRasp.Models
{
    public class InputRequest
    {
        public string FacultyName { get; private set; }

        //"yyyy.MM.dd"
        public DateTime MonthOfLessons { get; private set; }

        public InputRequest(string FacultyName, DateTime Date)
        {
            this.FacultyName = FacultyName;
            MonthOfLessons = Date;
        }
    }
}
