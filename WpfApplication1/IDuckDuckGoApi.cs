using System;
using System.Threading.Tasks;
using ReactiveSearch.Services.Api;
using Refit;

namespace WpfApplication1
{
    public interface IDuckDuckGoApi
    {
        [Get("/?q={query}&format=json")]
        Task<DuckDuckGoSearchResult> Search(string query);
    }
}
