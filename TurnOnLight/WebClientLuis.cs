using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Popups;

namespace TurnOnLight
{
    /// <summary>
    /// Classe per gestire la connessione all'endpoint cloud per l'analisi del flusso in input
    /// tramite l'intelligenza artificiale fornita da LUIS (MS Azure Cognitive Services) per
    /// mezzo del protocollo REST
    /// </summary>
    public class WebClientLuis
    {
        /// <summary>
        /// URL per la connessione a LUIS
        /// </summary>
        private const string LuisUrl = " YOUR_LUIS_ENDPOINT_HERE ";


        /// <summary>
        /// Instaura la connessione con il servizio LUIS (MS Azure)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<string> Get(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(new Uri(url));
            string responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }


        /// <summary>
        /// Invia al flusso al servizio LUIS (MS Azure)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static async Task<string> Post(string url, string key, string value)
        {
            HttpClient client = new HttpClient();
            FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>(key, value)
            });
            HttpResponseMessage response = await client.PostAsync(new Uri(url), content);
            string responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        /// <summary>
        /// Analizza la risposta ricevuta da LUIS
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public static async Task<Analyzer> Order(string sentence)
        {
            Analyzer result = new Analyzer
            {
                Entity = Entity.AllLights,
                Intent = Intent.None
            };
            try
            {

                string queryUrl = LuisUrl + sentence;
                string resultJson = await Get(queryUrl);

                //convert JSON in comands
                JsonObject obj = JsonObject.Parse(resultJson);
                string intent = obj["topScoringIntent"].GetObject()["intent"].GetString();
                switch (intent)
                {
                    case "TurnOn":
                        result.Intent = Intent.TurnOn;
                        break;
                    case "TurnOff":
                        result.Intent = Intent.TurnOff;
                        break;
                }

                IJsonValue entity = obj["entities"];
                if (entity.GetArray().Any())
                {
                    string entityLight = obj["entities"].GetArray()[0].GetObject()["type"].ToString().Replace("\"", "");
                    switch (entityLight)
                    {
                        case "AllLight":
                            result.Entity = Entity.AllLights;
                            break;
                        case "Green":
                            result.Entity = Entity.Green;
                            break;
                        case "Yellow":
                            result.Entity = Entity.Yellow;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (NetworkInformationException internetException)
            {
                var messageDialog = new MessageDialog($"Errore di rete, verificare la connessione a internet!\n\nError Code:\t{internetException.ErrorCode}\nError Message:\t{internetException.Message}", "NETWORK ERROR");

                // Show the message dialog of a connection error
                await messageDialog.ShowAsync();
            }
            catch (Exception exception)
            {
                var messageDialog = new MessageDialog($"Errore non previsto!\n\nError Source:\t{exception.Source}\nError Message:\t{exception.Message}", "GENERIC ERROR");

                // Show the message dialog of a connection error
                await messageDialog.ShowAsync();
            }

            return result;
        }
    }
}
