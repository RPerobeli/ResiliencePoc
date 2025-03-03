using Polly.CircuitBreaker;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResiliencePoc;

namespace Energisa.UtResilient.CircuitBreakers
{
    public static class CircuitBreakPipelineBuilderExtensions
    {
        public static ResiliencePipelineBuilder<T> AddCircuitBreakerReconhecimentoFacial<T>(this ResiliencePipelineBuilder<T> builder)
        {
            var options = new CircuitBreakerStrategyOptions<T>()
            {
                FailureRatio = 0.5,
                BreakDuration = TimeSpan.FromSeconds(5),
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 2,
                ShouldHandle = new PredicateBuilder<T>()
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

            };
            builder.AddCircuitBreaker(options);

            return builder;
        }
    }
}
