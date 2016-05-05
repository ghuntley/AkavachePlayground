using System;
using System.Net.Http;
using Fusillade;
using HttpClientDiagnostics;
using ModernHttpClient;
using Refit;

namespace WpfApplication1
{
    public class DuckDuckGoApiService 
    {
        public const string ApiBaseAddress = "https://api.duckduckgo.com";

        private readonly Lazy<IDuckDuckGoApi> _background;
        private readonly Lazy<IDuckDuckGoApi> _speculative;
        private readonly Lazy<IDuckDuckGoApi> _userInitiated;

        public DuckDuckGoApiService(string apiBaseAddress = null, bool enableDiagnostics = false)
        {
            Func<HttpMessageHandler, IDuckDuckGoApi> createClient = innerHandler =>
            {
            HttpMessageHandler handler;

            if (enableDiagnostics)
            {
                handler = new HttpClientDiagnosticsHandler(innerHandler);
            }
            else
            {
                handler = innerHandler;
            }

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(apiBaseAddress ?? ApiBaseAddress)
            };

            return RestService.For<IDuckDuckGoApi>(client);
        };

        _background = new Lazy<IDuckDuckGoApi>(() => createClient(
                new RateLimitedHttpMessageHandler(new NativeMessageHandler(), Priority.Background)));

        _userInitiated = new Lazy<IDuckDuckGoApi>(() => createClient(
                new RateLimitedHttpMessageHandler(new NativeMessageHandler(), Priority.UserInitiated)));

        _speculative = new Lazy<IDuckDuckGoApi>(() => createClient(
                new RateLimitedHttpMessageHandler(new NativeMessageHandler(), Priority.Speculative)));
    }

    public IDuckDuckGoApi Background => _background.Value;
    public IDuckDuckGoApi Speculative => _speculative.Value;
    public IDuckDuckGoApi UserInitiated => _userInitiated.Value;
}
}
