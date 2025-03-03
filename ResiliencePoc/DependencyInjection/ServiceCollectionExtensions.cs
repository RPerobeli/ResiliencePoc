using Energisa.UtResilient.CircuitBreakers;
using Polly;
using Polly.CircuitBreaker;
using System.Runtime.CompilerServices;

namespace ResiliencePoc.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPoliciesWithPolly(this IServiceCollection services)
        {
            services.AddResiliencePipeline<String, IList<WeatherForecast>>(
                "WeatherForecast-circuit-breaker",
                pipelineBuilder => pipelineBuilder.AddCircuitBreakerReconhecimentoFacial()
                );

            return services;
        }
    }
}
