using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Akavache;
using Newtonsoft.Json;
using ReactiveSearch.Services.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

namespace WpfApplication1
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DuckDuckGoApiService _apiService;

        public MainWindow()
        {
            BlobCache.ApplicationName = "test";
            BlobCache.EnsureInitialized();

            var log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile("debug.txt")
                .CreateLogger();

            Log.Logger = log;

            _apiService = new DuckDuckGoApiService(enableDiagnostics: false);

            Search = ReactiveCommand.CreateAsyncObservable(x => GetAndFetchLatest());
            Search
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(results =>
                {
                    Log.Error("===========================================================");
                    Log.Debug("ShouldThrowExceptions={0}", ShouldThrowExceptions);
                    var json = JsonConvert.SerializeObject(results);
                    Log.Debug("Results={0}", json.TruncateWithEllipsis(20));
                    try
                    {
                        Log.Information("CacheContentsAfterwards={0}", BlobCache.LocalMachine.Get("myCoolKey").Wait().ToString());
                    }
                    catch (Exception ex)
                    {
                        Log.Information("Key was not found in Akavache");
                    }
                    Log.Error("===========================================================");

                });
            Search.ThrownExceptions.Subscribe(exception =>
            {
                Log.Error("++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Log.Error(exception.ToString());
                Log.Error("++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            });

            InitializeComponent();
        }
        
        [Reactive]
        public bool ShouldThrowExceptions { get; private set; }

        public ReactiveCommand<DuckDuckGoSearchResult> Search { get; }

        private IObservable<DuckDuckGoSearchResult> GetAndFetchLatest()
        {
            return BlobCache.LocalMachine.GetAndFetchLatest("myCoolKey",
                async () => await FetchOrThrow(),
                datetimeOffset => true, // now + 7 days was unspecified by the business.
                DateTime.Now + TimeSpan.FromDays(7)).Catch(Observable.Return(new DuckDuckGoSearchResult()));
        }

        private async void OnExecuteClick(object sender, RoutedEventArgs e)
        {
            Log.Debug("Execute button pressed");
            Search.Execute(null);
        }

        private async Task<DuckDuckGoSearchResult> FetchOrThrow()
        {
            if (ShouldThrowExceptions)
            {
                Log.Debug("Exceptions are enabled, will throw");
                throw new Exception("This is the exception");
            }

            Log.Debug("Exceptions are not enabled");

            try
            {
                Log.Information("CacheContents={0}", BlobCache.LocalMachine.Get("myCoolKey").Wait().ToString());
            }
            catch (KeyNotFoundException ex)
            {
                Log.Information("Akavache key not found.");
            }
            Log.Information("Retrieving from API");
            var response = await _apiService.Background.Search("apple");
            return response;
        }

        private void OnResetAkavacheClicked(object sender, RoutedEventArgs e)
        {
            BlobCache.LocalMachine.InvalidateAll().Wait();
            Log.Warning("Akavache has been reset");
        }

        private void ShouldThrowCheckbox_OnChecked(object sender, RoutedEventArgs e)
        {
            ShouldThrowExceptions = true;
            Log.Error("===========================================================");
            Log.Error("ShouldThrowExceptions={0}", ShouldThrowExceptions);
            Log.Error("===========================================================");
        }

        private void ShouldThrowCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ShouldThrowExceptions = false;
            Log.Error("===========================================================");
            Log.Error("ShouldThrowExceptions={0}", ShouldThrowExceptions);
            Log.Error("===========================================================");
        }
    }
}