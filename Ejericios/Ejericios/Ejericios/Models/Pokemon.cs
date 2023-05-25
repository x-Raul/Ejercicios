using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ejericios.Models
{
    public class Pokemon
    {
        //? Null
        public string Name { get; set; }
        public string Url { get; set; }
        public List<string> Types { get; set; }
    }
}