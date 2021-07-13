using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OniBot
{
    public class AzureRestVoiceService : IVoiceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AzureRestVoiceService> _logger;
        private const string SpeechTemplate = @"
        <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='https://www.w3.org/2001/mstts' xml:lang='en-US'>
            <voice xml:lang='en-US' xml:gender='Female' name='en-US-JessaNeural'>{0}</voice>
        </speak>";

        public AzureRestVoiceService(ILogger<AzureRestVoiceService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Stream> ToVoice(string message)
        {
            var httpClient = _httpClientFactory.CreateClient(nameof(AzureRestVoiceService));
            var speech = string.Format(SpeechTemplate, message);

            var content = new StringContent(speech, Encoding.UTF8, "application/ssml+xml");

            var response = await httpClient.PostAsync(string.Empty, content);
            var headers = string.Join("\n", response.Content.Headers.Select(a => $"{a.Key}: {string.Join(", ", a.Value)}"));
            _logger.LogInformation($"Azure tts call returned {response.StatusCode} with {headers} headers");
            var tts = await response.Content.ReadAsByteArrayAsync();
            var file = Path.GetTempFileName();
            try
            {
                await File.WriteAllBytesAsync(file, tts);
                var stream = CreateStream(file);
                return stream;
            }
            finally
            {
                File.Delete(file);
            }
        }

        private static Stream CreateStream(string path)
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });

            var ms = new MemoryStream();
            proc.StandardOutput.BaseStream.CopyTo(ms);
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                throw new Exception("ffmpeg error");
            }

            ms.Position = 0;
            return ms;
        }
    }

    public class Authentication
    {
        private readonly Timer _accessTokenRenewer;
        private string _accessToken;
        private readonly SemaphoreSlim _tokenLock = new(1);
        private readonly IHttpClientFactory _httpClientFactory;

        private const string AuthEndpoint = "https://westus2.api.cognitive.microsoft.com/sts/v1.0/issuetoken";
        //Access token expires every 10 minutes. Renew it every 9 minutes only.
        private const int RefreshTokenDuration = 9;

        public Authentication(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            _accessToken = this.FetchTokenAsync().Result;

            // renew the token every specfied minutes
            _accessTokenRenewer = new Timer(new TimerCallback(async _ => await OnTokenExpiredCallback()),
                                           this,
                                           TimeSpan.FromMinutes(RefreshTokenDuration),
                                           TimeSpan.FromMilliseconds(-1));
        }

        public string AccessToken { get { return _accessToken; } }

        private async Task OnTokenExpiredCallback()
        {
            await _tokenLock.WaitAsync();
            try
            {
                _accessToken = await FetchTokenAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Failed renewing access token. Details: {0}", ex.Message));
            }
            finally
            {
                _tokenLock.Release();
                try
                {
                    _accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
                }
            }
        }

        public async Task<string> FetchTokenAsync()
        {
            var httpClient = _httpClientFactory.CreateClient(nameof(Authentication));

            var uriBuilder = new UriBuilder(AuthEndpoint);

            var result = await httpClient.PostAsync(uriBuilder.Uri.AbsoluteUri, null).ConfigureAwait(false);
            result.EnsureSuccessStatusCode();
            return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
    }
}

