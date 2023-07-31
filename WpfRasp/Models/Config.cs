using System.Collections.Generic;

namespace WpfRasp.Models
{
    public class Config
    {
        public string PathFaculty { get; set; }
        public Dictionary<string, string> FacultiesNames { get; set; }
        public List<string>? TimeOfLessons { get; set; }
    }
}
