using Microsoft.Extensions.Logging;
using OniBot.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OniBot
{
    public class AzureRestVoiceService : IVoiceService
    {
        private const string AuthEndpoint = "https://westus2.api.cognitive.microsoft.com/sts/v1.0/issuetoken";
        private const string ServiceEndpoint = "https://westus2.tts.speech.microsoft.com/cognitiveservices/v1";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly Authentication _auth;
        private readonly ILogger<AzureRestVoiceService> _logger;
        private const string SpeechTemplate = "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='https://www.w3.org/2001/mstts' xml:lang='en-US'><voice xml:lang='en-US' xml:gender='Female' name='en-US-JessaNeural'>{0}</voice></speak>";


        public AzureRestVoiceService(IBotConfig config, ILogger<AzureRestVoiceService> logger)
        {
            _auth = new Authentication(AuthEndpoint, config.AzureVoiceKey);
            httpClient.BaseAddress = new Uri(ServiceEndpoint);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "Keep-Alive");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "meowbot-speech");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Microsoft-OutputFormat", "audio-24khz-48kbitrate-mono-mp3");
            _logger = logger;
        }

        public async Task<Stream> ToVoice(string message)
        {
            var speech = string.Format(SpeechTemplate, message);
            var token = _auth.GetAccessToken();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(speech, Encoding.UTF8, "application/ssml+xml");

            var response = await httpClient.PostAsync("", content);
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

        private Stream CreateStream(string path)
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
            if(proc.ExitCode != 0)
            {
                throw new Exception("ffmpeg error");
            }

            ms.Position = 0;
            return ms;
        }

        public class Authentication
        {
            private readonly string _subscriptionKey;
            private readonly string _tokenFetchUri;
            private readonly Timer _accessTokenRenewer;
            private string _accessToken;
            private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1);
            private static readonly HttpClient _httpClient = new HttpClient();

            //Access token expires every 10 minutes. Renew it every 9 minutes only.
            private const int RefreshTokenDuration = 9;

            public Authentication(string tokenFetchUri, string subscriptionKey)
            {
                if (string.IsNullOrWhiteSpace(tokenFetchUri))
                {
                    throw new ArgumentNullException(nameof(tokenFetchUri));
                }

                if (string.IsNullOrWhiteSpace(subscriptionKey))
                {
                    throw new ArgumentNullException(nameof(subscriptionKey));
                }

                _tokenFetchUri = tokenFetchUri;
                _subscriptionKey = subscriptionKey;

                _accessToken = this.FetchTokenAsync().Result;

                // renew the token every specfied minutes
                _accessTokenRenewer = new Timer(new TimerCallback(async (s) => await OnTokenExpiredCallback(s)),
                                               this,
                                               TimeSpan.FromMinutes(RefreshTokenDuration),
                                               TimeSpan.FromMilliseconds(-1));
            }

            public string GetAccessToken()
            {
                _tokenLock.Wait();
                try
                {
                    return _accessToken;
                }
                finally
                {
                    _tokenLock.Release();
                }
            }

            private async Task OnTokenExpiredCallback(object stateInfo)
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
                _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this._subscriptionKey);
                UriBuilder uriBuilder = new UriBuilder(this._tokenFetchUri);

                HttpResponseMessage result = await _httpClient.PostAsync(uriBuilder.Uri.AbsoluteUri, null).ConfigureAwait(false);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
    }
}

