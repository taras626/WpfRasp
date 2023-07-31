using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRasp.Models
{
    public class Lecture
    {
        public string? date { get; set; }
        public int contentTableOfLessonsOid { get; set; }
        public int dayOfWeek { get; set; }
        public string? dayOfWeekString { get; set; }
        public string? beginLesson { get; set; }
        public string? building { get; set; }
        public string? subGroup { get; set; }
    }
}
