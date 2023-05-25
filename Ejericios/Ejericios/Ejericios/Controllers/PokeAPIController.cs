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
using OfficeOpenXml;
using System.Net.Mail;
using System.Text;

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

            ViewBag.Types = types;
            return View(pokemons);
        }

        private async Task<List<Pokemon>> GetPokemonsAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"{BaseUrl}pokemon?limit=100");
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


        //Exportar EXCEL

        private async Task<List<Pokemon>> GetFilteredDataAsync(string nameFilter, string typeFilter)
        {
            var pokemons = await GetPokemonsAsync();
            if (!string.IsNullOrEmpty(nameFilter))
            {
                pokemons = pokemons.Where(p => p.Name.Contains(nameFilter)).ToList();
            }
            if (!string.IsNullOrEmpty(typeFilter))
            {
                pokemons = pokemons.Where(p => p.Types.Contains(typeFilter)).ToList();
            }
            return pokemons;
        }

        public async Task<ActionResult> ExportToCsv(string nameFilter, string typeFilter)
        {
            var pokemons = await GetPokemonsAsync();
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
            var fileName = "Pokemons.csv";
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        [HttpPost]
        public async Task<ActionResult> SendEmail(string email)
        {
            try
            {
                var filteredData = await GetFilteredDataAsync(Request["nameFilter"], Request["typeFilter"]);
                var message = new MailMessage();
                message.To.Add(new MailAddress(email));
                message.Subject = "Pokemons filtrados";
                message.Body = "Se adjunta Excel con los pokemons filtrados..";
                var stream = new MemoryStream();
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Pokemons");
                    worksheet.Cells.LoadFromCollection(filteredData, true);
                    package.Save();
                }
                stream.Position = 0;
                var fileName = "Pokemons.xlsx";
                message.Attachments.Add(new Attachment(stream, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                var smtpClient = new SmtpClient();
                smtpClient.Send(message);
                return Content("El correo electrónico ha sido enviado exitosamente.");
            }
            catch (Exception ex)
            {
                return Content("Error al enviar el correo electrónico: " + ex.Message);
            }
        }

    }
}