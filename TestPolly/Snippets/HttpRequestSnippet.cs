using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace TestPolly.Snippets
{
    internal class HttpRequestSnippet
    {
        private readonly static IHttpClientFactory httpClientFactory;
        
        //С помощью этого пайплайна можно задать политику выполнения кода
        //В данном случае заданы 2 политики: retry и timeout
        private readonly ResiliencePipeline resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions() //Делаем запрос 5 раз каждые 100мс, если словили Exception
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMilliseconds(100)
            })
            .AddTimeout(TimeSpan.FromMilliseconds(400)) //Если через 400мс не получили результат, отменяем выполнение
            .Build();

        static HttpRequestSnippet()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();
            httpClientFactory = serviceProvider.GetService<IHttpClientFactory>()!;
        }

        public async Task<string> GetRandomEnglishWord()
        {
            var httpClient = httpClientFactory.CreateClient();

            try
            {
                var response = await resiliencePipeline.ExecuteAsync(async cancellationToken =>
                {
                    return await httpClient.GetAsync("https://random-word-api.herokuapp.com/word?lang=en", cancellationToken);
                });

                return JsonConvert.DeserializeObject<string[]>(await response.Content.ReadAsStringAsync())[0];
            }
            catch (TimeoutRejectedException ex)
            {
                return "timeout rejected";
            }
        }
    }
}
