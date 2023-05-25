using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ejericios.Models
{
    public class Juego
    {
        public List<Tuple<string, string>> Jugadas { get; set; }
        public string Resultado { get; set; }
    }
}