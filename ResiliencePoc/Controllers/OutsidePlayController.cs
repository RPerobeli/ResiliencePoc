using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using System.Text.Json;

namespace ResiliencePoc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OutsidePlayController : ControllerBase
    {
        private readonly ResiliencePipelineProvider<string> _pipelineProvider;
        private readonly ILogger<OutsidePlayController> _logger;

        public OutsidePlayController(ILogger<OutsidePlayController> logger, ResiliencePipelineProvider<string> pipelineProvider)
        {
            _logger = logger;
            _pipelineProvider = pipelineProvider;
        }

        [HttpGet(Name = "PlayOutside")]
        public async Task<IEnumerable<PlayOutside>> Get()
        {

            string url = "https://localhost:7008/WeatherForecast";
            HttpClient client = new HttpClient();
            List<string> shouldPlayWeather = ["Cool", "Mild", "Warm", "Balmy", "Hot"];
            List<PlayOutside> result = new List<PlayOutside>();


            try
            {
                //Create resilience policy:
                ResiliencePipeline<IList<WeatherForecast>> pipeline = _pipelineProvider.GetPipeline<IList<WeatherForecast>>("WeatherForecast-circuit-breaker");


                // Faz a requisição GET para a API com a policy pipe
                var weatherForecasts = await pipeline.ExecuteAsync<IList<WeatherForecast>>(
                    async token => await client.GetFromJsonAsync<IList<WeatherForecast>>(url),
                    CancellationToken.None
                    );


                if (weatherForecasts?.Count > 0)
                {
                    foreach (var day in weatherForecasts)
                    {
                        CreateShouldPlayOutside(shouldPlayWeather, result, day);
                    }
                }
                return result;
            }
            catch (BrokenCircuitException ex)
            {
                Console.WriteLine("Operation failed too many times please try again later.");
                return result;
            }
            catch (Exception ex)
            {
                // Trata exceções (como problemas de rede ou desserialização)
                Console.WriteLine("Erro ao chamar a API: " + ex.Message);
                return result;
            }
        }


        private static void CreateShouldPlayOutside(List<string> shouldPlayWeather, List<PlayOutside> result, WeatherForecast day)
        {
            if (day.Summary is not null && shouldPlayWeather.Contains(day.Summary))
            {
                result.Add(new PlayOutside
                {
                    IsPossibleToPlayOutside = true,
                    Date = day.Date
                });
            }
            else
            {
                result.Add(new PlayOutside
                {
                    IsPossibleToPlayOutside = false,
                    Date = day.Date
                });
            }
        }
    }
}
