namespace TestClient;

using CartService;
using CartService.Model;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utils;

/// <summary>
/// TODO: Auto-generate class.
/// </summary>
public class CartServiceClient : ICartService {

    public string BaseUrl { get; set; } = "http://localhost/";

    public HttpClient HttpClient { get; set; } = new();

    public CartServiceClient() {
        HttpClient.Timeout = TimeSpan.FromSeconds(RpcSettings.HttpTimeoutSeconds);
    }

    /// <inheritdoc/>
    public async Task<Cart> AddArticle(Article article, int count) =>
        await AddArticle(article, count, CancellationToken.None);

    /// <summary>
    /// See <see cref="AddArticle"/>, just with additional cancellation token.
    /// </summary>
    public async Task<Cart> AddArticle(Article article, int count, CancellationToken cancellationToken) {
        // Code inspired by NSwag. Thanks!
        // URL
        System.Text.StringBuilder url = new();
        url.Append(BaseUrl.TrimEnd('/')).Append("/api/cart/addarticle?");
        // Query string
        url.Append(Uri.EscapeDataString("count") + "=")
            .Append(Uri.EscapeDataString(ConvertToString(count, System.Globalization.CultureInfo.InvariantCulture)))
            .Append("&");
        url.Length--;
        // Body
        var content = new StringContent(JsonUtils.Serialize(article));
        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        // Prepare HTML client
        using HttpRequestMessage request = new();
        request.Content = content;
        request.Method = new HttpMethod("POST");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.RequestUri = new Uri(url.ToString(), UriKind.RelativeOrAbsolute);
        // Send and wait for response
        try {
            var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var status = (int)response.StatusCode;
            if (status == 200) {
                var ret = await ReadResponse<Cart>(response, cancellationToken).ConfigureAwait(false);
                return ret ?? throw new Exception("Unexpected null response");
            } else {
                throw new RpcException(RpcFailureType.RemoteException, "Unexpected status code " + status);
            }
        } catch (HttpRequestException ex) {
            throw new RpcException(RpcFailureType.NetworkFailure, ex.Message);
        }
    }

    public Task<int> GetTotalPrice() {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public event Action<Cart> CartChanged {
        add {
            lock (listenToCartLock) {
                // Already registered? Then ignore.
                if (cartChangedListeners.Contains(value))
                    return;
                // Remember subscription
                cartChangedListeners.Add(value);
            }
            // Start listening handler, if not already started
            _ = ListenToCartChanged();
        }
        remove {
            // Cancel subscription
            lock (listenToCartLock) {
                cartChangedListeners.Remove(value);
                if (listenToCartLoop != null && cartChangedListeners.Count == 0) {
                    listenToCartLoop.Cancel();
                }
            }
        }
    }

    private HashSet<Action<Cart>> cartChangedListeners = new();
    private object listenToCartLock = new();
    CancellationTokenSource? listenToCartLoop = null;

    private async Task ListenToCartChanged() {

        lock (listenToCartLock) {
            if (listenToCartLoop != null || cartChangedListeners.Count == 0)
                return;
            listenToCartLoop = new CancellationTokenSource();
        }

        // Loop, as long as there are subscribers.
        while (!listenToCartLoop.Token.IsCancellationRequested) {
            // Listen to server-sent events
            string url = BaseUrl.TrimEnd('/') + "/api/cart/cartchanged";
            // Prepare HTML client
            try {
                // Read streamed data
                using var stream = await HttpClient.GetStreamAsync(url, listenToCartLoop.Token);
                using var streamReader = new StreamReader(stream);
                StringBuilder currentData = new();
                while (!listenToCartLoop.Token.IsCancellationRequested) {
                    var readTask = streamReader.ReadLineAsync();
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(RpcSettings.KeepAliveSeconds + RpcSettings.HttpTimeoutSeconds), listenToCartLoop.Token);
                    await Task.WhenAny(readTask, timeoutTask);
                    if (listenToCartLoop.Token.IsCancellationRequested)
                        break;
                    if (timeoutTask.IsCompleted) {
                        Console.WriteLine("!!! Connection seems to be dead. Kill.");
                        throw new Exception("Connection dead");
                    } else {
                        // Handle message
                        var message = readTask.Result;
                        if (message == null) {
                            // Ignore
                        } else if (message.Length == 0) {
                            // End of event. Fire event, if we have data.
                            if (currentData.Length > 0) {
                                var eventData = JsonUtils.Deserialize<Cart>(currentData.ToString());
                                if (eventData != null) {
                                    try {
                                        foreach (var listener in cartChangedListeners)
                                            listener(eventData);
                                    } catch (Exception ex) {
                                        Console.WriteLine("!!! Error in event handler: " + ex); // TODO: OnError / OnLog event to allow arbitrary logging binding ?
                                    }
                                }
                                currentData.Clear();
                            }
                        } else if (message.StartsWith("data:")) {
                            // Additional data
                            currentData.Append(message!.Substring("data:".Length).Trim());
                        } else {
                            // Ignore
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine("!!! No connection to service. Try again in 5 seconds. (" + ex.Message + ")"); // TODO: max retries? report service status ?
                await Task.Delay(5000, listenToCartLoop.Token);
            }
        }

        lock (listenToCartLock) {
            listenToCartLoop = null;
        }
    }



    // Code from NSwag (thanks!)
    private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo) {
        if (value == null) {
            return "";
        }

        if (value is Enum) {
            var name = Enum.GetName(value.GetType(), value);
            if (name != null) {
                var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                if (field != null) {
                    var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute))
                        as System.Runtime.Serialization.EnumMemberAttribute;
                    if (attribute != null) {
                        return attribute.Value != null ? attribute.Value : name;
                    }
                }

                var converted = Convert.ToString(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), cultureInfo));
                return converted == null ? string.Empty : converted;
            }
        } else if (value is bool) {
            return Convert.ToString((bool)value, cultureInfo).ToLowerInvariant();
        } else if (value is byte[]) {
            return Convert.ToBase64String((byte[])value);
        } else if (value.GetType().IsArray) {
            var array = Enumerable.OfType<object>((System.Array)value);
            return string.Join(",", Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
        }

        var result = Convert.ToString(value, cultureInfo);
        return result == null ? "" : result;
    }

    private async Task<T?> ReadResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken) {
        if (response.Content == null)
            return default(T);
        try {
            /*
            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return JsonUtils.Deserialize<T>(responseStream); /*/
            string json = await response.Content.ReadAsStringAsync();
            return JsonUtils.Deserialize<T>(json);
            //*/
        } catch (Exception ex) {
            var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
            throw new Exception(message, ex);
        }
    }



}