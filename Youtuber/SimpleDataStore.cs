using System.Threading.Tasks;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace Youtuber
{
    public interface ISimpleDataStore : IDataStore
    {
        Task<string> GetJsonAsync(string key);
    }
    public class SimpleDataStore : ISimpleDataStore
    {
        private string json = "";
        public Task ClearAsync()
        {
            json = string.Empty;
            return Task.CompletedTask;
        }

        public Task DeleteAsync<T>(string key)
        {
            json = string.Empty;
            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string key)
        {
            T value = string.IsNullOrWhiteSpace(json) ? default : JsonConvert.DeserializeObject<T>(json);

            return Task.FromResult(value);
        }

        public Task<string> GetJsonAsync(string key)
        {
            return Task.FromResult(json);
        }

        public Task StoreAsync<T>(string key, T value)
        {
            json = JsonConvert.SerializeObject(value);
            return Task.CompletedTask;
        }
    }
}