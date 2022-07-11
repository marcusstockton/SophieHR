using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace SophieHR.Api.Services
{
    public interface IJobTitleService
    {
        Task<IList<string>> JobTitlesAsync();
    }
    public class JobTitleServiceCache : IJobTitleService
    {
        IMemoryCache _inMemoryCache;
        static List<string> _cacheKeys = new List<string>();

        public JobTitleServiceCache(IMemoryCache inMemoryCache)
        {
            _inMemoryCache = inMemoryCache;
        }

        public async Task<IList<string>> JobTitlesAsync()
        {
            if (_inMemoryCache.TryGetValue("JobTitles", out IList<string> jobTitles))
            {
                return jobTitles;
            }
            jobTitles = await GetJobTitles();

            _inMemoryCache.Set("JobTitles", jobTitles);

            _cacheKeys.Add("JobTitles");

            return jobTitles;
        }

        private async Task<IList<string>> GetJobTitles()
        {
            var url = "https://raw.githubusercontent.com/jneidel/job-titles/master/job-titles.json";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var data = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<JobTitleAutocompleteResponse>(data);
                return result.JobTitles.ToList();
            }
        }
        class JobTitleAutocompleteResponse
        {
            [JsonProperty("job-titles")]
            public string[] JobTitles { get; set; }
        }
    }
}
