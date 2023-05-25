using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ejericios.Models
{
    public class PokemonList
    {
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }



        public List<Pokemon> Results { get; set; }
    }
}