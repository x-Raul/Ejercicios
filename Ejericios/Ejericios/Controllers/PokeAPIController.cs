using Ejericios.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Net;

namespace Ejericios.Controllers
{
    public class PokeAPIController : Controller
    {
        // GET: PokeAPI
        private const string BaseUrl = "https://pokeapi.co/api/v2/";

        public async Task<ActionResult> Index(string nameFilter, string typeFilter)
        {
            var pokemons = await GetPokemonsAsync();
            var types = await GetTypesAsync();

            if (!string.IsNullOrEmpty(nameFilter))
            {
                pokemons = pokemons.Where(p => p.Name.Contains(nameFilter)).ToList();
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                pokemons = pokemons.Where(p => p.Types.Contains(typeFilter)).ToList();
            }
            //Guardo los fintros para mostrarlos
            ViewBag.Types = types;
            ViewBag.NameFilter = nameFilter;
            ViewBag.TypeFilter = typeFilter;

            return View(pokemons);
        }

        private async Task<List<Pokemon>> GetPokemonsAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{BaseUrl}pokemon?limit=150"); //Limite
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var pokemonList = JsonConvert.DeserializeObject<PokemonList>(content);

                foreach (var pokemon in pokemonList.Results)
                {
                    var pokemonResponse = await client.GetAsync(pokemon.Url);
                    pokemonResponse.EnsureSuccessStatusCode();
                    var pokemonContent = await pokemonResponse.Content.ReadAsStringAsync();
                    dynamic pokemonData = JsonConvert.DeserializeObject(pokemonContent);
                    pokemon.Types = ((IEnumerable<dynamic>)pokemonData.types)
                        .Select(t => (string)t.type.name).ToList();
                }

                return pokemonList.Results;
            }
        }

        private async Task<List<Models.Type>> GetTypesAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{BaseUrl}type");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                dynamic typeData = JsonConvert.DeserializeObject(content);
                var types = ((IEnumerable<dynamic>)typeData.results)
                    .Select(t => new Models.Type { Name = t.name, Url = t.url }).ToList();
                return types;
            }
        }
        /*
        public ActionResult FilterPokemons(string nameFilter, string typeFilter)
        {
            var pokemons = GetPokemonsAsync().Result;

            if (!string.IsNullOrEmpty(nameFilter))
            {
                pokemons = pokemons.Where(p => p.Name.Contains(nameFilter)).ToList();
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                pokemons = pokemons.Where(p => p.Types.Contains(typeFilter)).ToList();
            }

            return View("_PokemonTable", pokemons);
        }*/


        //Exportar CSV

        public async Task<string> ExportToCsv(string nameFilter, string typeFilter)
        {
            var pokemons = await GetPokemonsAsync();
            //Filtros
            if (!string.IsNullOrEmpty(nameFilter))
            {
                pokemons = pokemons.Where(p => p.Name.Contains(nameFilter)).ToList();
            }
            if (!string.IsNullOrEmpty(typeFilter))
            {
                pokemons = pokemons.Where(p => p.Types.Contains(typeFilter)).ToList();
            }
            var csv = new StringBuilder();
            csv.AppendLine("Nombre ,Tiipo , Tipo 2");
            foreach (var pokemon in pokemons)
            {
                csv.AppendLine($"{pokemon.Name},{pokemon.Types[0]},{(pokemon.Types.Count > 1 ? pokemon.Types[1] : "")}");
            }
            return csv.ToString();
        }

        public async Task<ActionResult> SendEmail(string email, string nameFilter, string typeFilter)
        {
            try
            {
                // Generar el CSV
                string csvData = await ExportToCsv(nameFilter, typeFilter);

                // Configuracion SMTP
                var smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("raul.gonzalez@scisa.com.mx", "vrpddhbdliqqedkc")
                };

                // Mensaje de mail
                var message = new MailMessage("raul.gonzalez@scisa.com.mx", email)
                {
                    Subject = "Pokemons Excel",
                    Body = "Se adjunta CSV con los pokemons filtrados..."
                };

                // Se agrega el CSV
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvData)))
                {
                    var attachment = new Attachment(stream, "Pokemons.csv", "text/csv");
                    message.Attachments.Add(attachment);

                    // Se envia
                    smtpClient.Send(message);
                }

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }





    }
}