using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfRasp.Models;

namespace WpfRasp
{
    public class Group
    {
        public string description { get; set; }

        public int id { get; set; }

        public string label { get; set; }

        public List<Lecture> lectures { get; set; }
    }
}
