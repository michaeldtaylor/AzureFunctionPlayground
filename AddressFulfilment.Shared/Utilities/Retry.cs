using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;

namespace AddressFulfilment.Shared.Utilities
{
    public static class Retry
    {
        /// <summary>
        /// This policy will log and throw an exception if the request returns a 4xx code
        /// </summary>
        private static readonly Policy<HttpResponseMessage> Http400LogPolicy = Policy
            .HandleResult<HttpResponseMessage>(x => (int)x.StatusCode >= 400 && (int)x.StatusCode <= 499)
            .RetryAsync(1,
                   (result, i, context) =>
                   {
                       TraceLogger.Error($"{context.PolicyKey} at {context.CorrelationId}: execution failed after {i} attempts.", nameof(Retry)); // sort this message
                       result.Result.EnsureSuccessStatusCode();
                   })
            .WithPolicyKey(nameof(Http400LogPolicy));

        /// <summary>
        /// This policy retries a request 3 times, with a one second pause in between, if a 5xx result is returned
        /// </summary>
        public static readonly Policy<HttpResponseMessage> Http500LogAndRetryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(x => (int)x.StatusCode >= 500)
            .WaitAndRetryAsync(
                Enumerable.Repeat(TimeSpan.FromSeconds(1), 3),
                (result, span, context) => TraceLogger.Warn($"{context.PolicyKey} at {context.CorrelationId}: execution failed after {span.TotalSeconds} seconds.", nameof(Http500LogAndRetryPolicy)))
            .WithPolicyKey(nameof(Http500LogAndRetryPolicy));

        /// <summary>
        /// This policy combines policies for 4xx and 5xx responses to log and throw appropriately
        /// if 4xx, the response is logged to trace and thrown
        /// if 5xx, the policy will log and retry 3 times, with a 1 second pause, before failing
        /// </summary>
        public static Policy<HttpResponseMessage> HttpApiPolicy => Policy.WrapAsync(Http400LogPolicy, Http500LogAndRetryPolicy);

        /// <summary>
        /// Executes an action that calls an http api, for example using an HttpClient. This should wrap the HttpApiPolicy
        /// e.g. Retry.HttpRequestWithFallbackAsync(async () => await Retry.HttpApiPolicy.ExecuteAsync(() => new HttpClient().GetAsync(url)));
        /// Logs on general exceptions inside the action
        /// </summary>
        /// <typeparam name="T">The return type of the action</typeparam>
        /// <param name="action">The code to execute against the policy</param>
        /// <param name="fallback">The default value to return if the action fails</param>
        /// <returns>Either the value from the action or the default value</returns>
        public static Task<T> HttpRequestWithFallbackAsync<T>(Func<Task<T>> action, T fallback)
        {
            return Policy<T>
                .Handle<Exception>()
                .FallbackAsync(fallback, result =>
                {
                    TraceLogger.Error(result.Exception.Message, nameof(Retry), result.Exception);

                    return Task.CompletedTask;
                })
                .WrapAsync(Policy<T>
                    .Handle<HttpRequestException>()
                    .FallbackAsync(fallback))
                .ExecuteAsync(action);
        }

        /// <summary>
        /// Executes an async action with repeats if the action throws an exception. The action is retried with a 1 second delay each time.
        /// </summary>
        /// <typeparam name="T">Return type of the action</typeparam>
        /// <param name="action">The action to execute</param>
        /// <returns>The result of the action</returns>
        public static Task<T> WithDelayOnExceptionAsync<T>(Func<Task<T>> action)
        {
            return Policy<T>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    Enumerable.Repeat(TimeSpan.FromSeconds(1), 3),
                    (result, span, context) => TraceLogger.Warn($"{context.PolicyKey} at {context.CorrelationId}: execution failed after {span.TotalSeconds} seconds. error={result.Exception}", nameof(WithDelayOnExceptionAsync)))
                .WithPolicyKey(nameof(WithDelayOnExceptionAsync))
                .ExecuteAsync(action);
        }

        /// <summary>
        /// Executes an async action with repeats if the action throws an exception. The action is retried with a 1 second delay each time.
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <returns>The result of the action</returns>
        public static Task WithDelayOnExceptionAsync(Func<Task> action)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    Enumerable.Repeat(TimeSpan.FromSeconds(1), 3),
                    (result, span, context) => TraceLogger.Warn($"{context.PolicyKey} at {context.CorrelationId}: execution failed after {span.TotalSeconds} seconds. error={result}", nameof(WithDelayOnExceptionAsync)))
                .WithPolicyKey(nameof(WithDelayOnExceptionAsync))
                .ExecuteAsync(action);
        }

        /// <summary>
        /// Executes an action with repeats if the action throws an exception. The action is retried with a 1 second delay each time.
        /// </summary>
        /// <typeparam name="T">Return type of the action</typeparam>
        /// <param name="action">The action to execute</param>
        /// <returns>The result of the action</returns>
        public static T WithDelayOnException<T>(Func<T> action)
        {
            return Policy<T>
                .Handle<Exception>()
                .WaitAndRetry(
                    Enumerable.Repeat(TimeSpan.FromSeconds(1), 3),
                    (result, span, context) => TraceLogger.Warn($"{context.PolicyKey} at {context.CorrelationId}: execution failed after {span.TotalSeconds} seconds. error={result.Exception}", nameof(WithDelayOnException)))
                .WithPolicyKey(nameof(WithDelayOnException))
                .Execute(action);
        }
    }
}
