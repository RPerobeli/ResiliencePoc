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
                pipelineBuilder => pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<IList<WeatherForecast>>
                    {
                        FailureRatio = 0.5,
                        BreakDuration = TimeSpan.FromSeconds(5),
                        SamplingDuration = TimeSpan.FromSeconds(30),
                        MinimumThroughput = 2,
                        ShouldHandle = new PredicateBuilder<IList<WeatherForecast>>()
                            .Handle<Exception>(),
                        OnOpened = args =>
                        {
                            Console.WriteLine("Operation failed too many times please try again later.");
                            return ValueTask.CompletedTask;
                        },
                        OnClosed = args =>
                        {
                            Console.WriteLine("Circuit breaker closed. Requests are allowed again.");
                            return ValueTask.CompletedTask;
                        },
                        OnHalfOpened = args =>
                        {
                            Console.WriteLine("Circuit breaker half-opened. Testing the system...");
                            return ValueTask.CompletedTask;
                        }

                    })

                );
            return services;
        }
    }
}
