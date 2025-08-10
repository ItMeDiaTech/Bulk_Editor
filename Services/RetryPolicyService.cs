using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bulk_Editor.Configuration;
using Bulk_Editor.Models;

namespace Bulk_Editor.Services
{
    /// <summary>
    /// Retry policy service for handling transient failures
    /// </summary>
    public class RetryPolicyService
    {
        private readonly RetrySettings _settings;
        private readonly IProgress<ProgressReport> _progress;

        public RetryPolicyService(RetrySettings settings, IProgress<ProgressReport> progress = null)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _progress = progress;
        }

        /// <summary>
        /// Execute an operation with retry policy
        /// </summary>
        public async Task<T> ExecuteWithRetryAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            Exception lastException = null;

            for (int attempt = 1; attempt <= _settings.MaxRetryAttempts; attempt++)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return await operation(cancellationToken);
                }
                catch (Exception ex) when (IsRetryableException(ex) && attempt < _settings.MaxRetryAttempts)
                {
                    lastException = ex;

                    // Report retry status
                    var retryReport = ProgressReport.CreateRetryNotification(
                        attempt,
                        _settings.MaxRetryAttempts,
                        GetConciseErrorMessage(ex)
                    );
                    _progress?.Report(retryReport);

                    var delay = CalculateDelay(attempt);
                    await Task.Delay(delay, cancellationToken);
                }
                catch (Exception)
                {
                    // Non-retryable exception or max attempts reached
                    throw;
                }
            }

            // All retries exhausted
            throw lastException ?? new InvalidOperationException("Retry operation failed");
        }

        /// <summary>
        /// Execute an operation with retry policy (void return)
        /// </summary>
        public async Task ExecuteWithRetryAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            await ExecuteWithRetryAsync(async (ct) =>
            {
                await operation(ct);
                return true; // Dummy return value
            }, cancellationToken);
        }

        /// <summary>
        /// Calculate delay for retry attempt
        /// </summary>
        private int CalculateDelay(int attempt)
        {
            if (!_settings.UseExponentialBackoff)
                return _settings.BaseDelayMs;

            // Exponential backoff: base * 2^(attempt-1)
            var delay = _settings.BaseDelayMs * Math.Pow(2, attempt - 1);
            return Math.Min((int)delay, _settings.MaxDelayMs);
        }

        /// <summary>
        /// Get a concise error message for user display
        /// </summary>
        private string GetConciseErrorMessage(Exception ex)
        {
            return ex switch
            {
                HttpRequestException httpEx => "Network error",
                TaskCanceledException => "Request timeout",
                WebException webEx => GetWebExceptionMessage(webEx),
                ArgumentException => "Invalid data",
                UnauthorizedAccessException => "Access denied",
                _ => "API error"
            };
        }

        /// <summary>
        /// Get specific message for web exceptions
        /// </summary>
        private string GetWebExceptionMessage(WebException webEx)
        {
            return webEx.Status switch
            {
                WebExceptionStatus.Timeout => "Request timeout",
                WebExceptionStatus.ConnectFailure => "Connection failed",
                WebExceptionStatus.NameResolutionFailure => "DNS error",
                WebExceptionStatus.ProxyNameResolutionFailure => "Proxy error",
                _ => "Network error"
            };
        }

        /// <summary>
        /// Determine if an exception is retryable
        /// </summary>
        private bool IsRetryableException(Exception ex)
        {
            return ex switch
            {
                // Network-related exceptions that are typically transient
                HttpRequestException => true,
                TaskCanceledException when !ex.Message.Contains("canceled") => true, // Timeout, not user cancellation
                WebException webEx => IsRetryableWebException(webEx),

                // Specific HTTP status codes that are retryable
                HttpStatusCodeException httpStatusEx => IsRetryableStatusCode(httpStatusEx.StatusCode),

                // General transient failures
                TimeoutException => true,
                SocketException => true,

                // Don't retry these
                ArgumentException => false,
                UnauthorizedAccessException => false,
                SecurityException => false,
                NotSupportedException => false,

                _ => false
            };
        }

        /// <summary>
        /// Check if a web exception is retryable
        /// </summary>
        private bool IsRetryableWebException(WebException webEx)
        {
            return webEx.Status switch
            {
                WebExceptionStatus.Timeout => true,
                WebExceptionStatus.ConnectFailure => true,
                WebExceptionStatus.NameResolutionFailure => false, // DNS issues usually persistent
                WebExceptionStatus.ProxyNameResolutionFailure => false,
                WebExceptionStatus.SendFailure => true,
                WebExceptionStatus.ReceiveFailure => true,
                WebExceptionStatus.RequestCanceled => false, // User cancellation
                WebExceptionStatus.ProtocolError => IsRetryableHttpStatusCode(webEx),
                _ => false
            };
        }

        /// <summary>
        /// Check if HTTP status code in WebException is retryable
        /// </summary>
        private bool IsRetryableHttpStatusCode(WebException webEx)
        {
            if (webEx.Response is HttpWebResponse response)
            {
                return IsRetryableStatusCode(response.StatusCode);
            }
            return false;
        }

        /// <summary>
        /// Check if HTTP status code is retryable
        /// </summary>
        private bool IsRetryableStatusCode(HttpStatusCode statusCode)
        {
            return statusCode switch
            {
                // Server errors that might be transient
                HttpStatusCode.InternalServerError => true,        // 500
                HttpStatusCode.BadGateway => true,                 // 502
                HttpStatusCode.ServiceUnavailable => true,        // 503
                HttpStatusCode.GatewayTimeout => true,            // 504

                // Client errors that are NOT retryable
                HttpStatusCode.BadRequest => false,               // 400
                HttpStatusCode.Unauthorized => false,             // 401
                HttpStatusCode.Forbidden => false,                // 403
                HttpStatusCode.NotFound => false,                 // 404
                HttpStatusCode.MethodNotAllowed => false,         // 405

                // Rate limiting - could be retryable
                HttpStatusCode.TooManyRequests => true,           // 429

                _ => false
            };
        }
    }

    /// <summary>
    /// Custom exception for HTTP status codes
    /// </summary>
    public class HttpStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public HttpStatusCodeException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Socket exception placeholder for .NET Framework compatibility
    /// </summary>
    public class SocketException : Exception
    {
        public SocketException(string message) : base(message) { }
    }

    /// <summary>
    /// Security exception placeholder
    /// </summary>
    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
    }
}