using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Ejericios.Models;

namespace Ejericios.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var random = new Random();
            var opciones = new[] { "R", "P", "S" };
            var jugadas = new List<Tuple<string, string>>();

            for (int i = 0; i < 3; i++)
            {
                var jugador1 = opciones[random.Next(opciones.Length)];
                var jugador2 = opciones[random.Next(opciones.Length)];
                jugadas.Add(Tuple.Create(jugador1, jugador2));
            }

            Juego juego = new Juego
            {
                Jugadas = jugadas
            };
            juego.Resultado = CalcularResultado(juego.Jugadas);
            return View(juego);
        }

        public string CalcularResultado(List<Tuple<string, string>> jugadas)
        {
            int jugador1 = 0;
            int jugador2 = 0;

            foreach (var jugada in jugadas)
            {
                if (jugada.Item1 == jugada.Item2)
                    continue;

                if ((jugada.Item1 == "R" && jugada.Item2 == "S") || (jugada.Item1 == "P" && jugada.Item2 == "R") || (jugada.Item1 == "S" && jugada.Item2 == "P"))
                    jugador1++;
                else
                    jugador2++;
            }

            if (jugador1 > jugador2)
                return "Jugador 1";
            else if (jugador1 < jugador2)
                return "Jugador 2";
            else
                return "Empate";
        }


        public ActionResult Contact()
        {
            ViewBag.Message = "Test page.";

            return View();
        }
    }
}