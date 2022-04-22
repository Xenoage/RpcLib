namespace CartService.Generated;

using global::CartService.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Utils;

/// <summary>
/// TODO: Auto-generate this file.
/// </summary>
[ApiController]
[Route("api/cart")]
public class CartServiceController : ControllerBase {

    private readonly ICartService cartService;

    public CartServiceController(ICartService cartService) {
        this.cartService = cartService;
    }

    /// <summary>
    /// Adds the given article (one or more) to the cart.
    /// If articles with the same <see cref="Article.ID"/> are already within the cart,
    /// their name and price must match.
    /// </summary>
    /// <returns>The updated cart.</returns>
    [HttpPost("addarticle")]
    public async Task<Cart> AddArticle([FromBody] Article article, [FromQuery] int count, CancellationToken cancellationToken) =>
        await cartService.AddArticle(article, count);

    /// <summary>
    /// Gets the total price of all items in the shopping carts.
    /// </summary>
    [HttpGet("totalprice")]
    public async Task<int> GetTotalPrice(CancellationToken cancellationToken) =>
        await cartService.GetTotalPrice();

    /// <summary>
    /// Call this method to open a SSE stream for listening to the CartChanged event.
    /// This event is raised whenever the cart was changed. The updated cart is given.
    /// </summary>
    [HttpGet("cartchanged")]
    public async Task CartChanged(CancellationToken cancellationToken) {

        // add listener early

        // Header for server-sent events
        Response.StatusCode = 200;
        Response.Headers.Add("Content-Type", "text/event-stream");
        SemaphoreSlim writeLock = new(1, 1);
        await WriteAndFlush(Response, "\n", writeLock);


     
        // start handler loop

        // Register event listener and send event, when fired
        CancellationTokenSource errorCancellation = new();
        Action<Cart> listener = async (Cart cart) => {
            try {
                string json = JsonUtils.Serialize(cart);
                // TODO: Make event handler synchronous and queue into ConcurrentQueue
                // and add additional loop to handle that queue,
                // in this way we guarantee the right order of the events.
                // Allows us to collapse the queue to remove equal event data or
                // send just the latest event, and so on... (future strategies)
                await WriteAndFlush(Response, $"data:{json}\n\n", writeLock);
            } catch (Exception) {
                errorCancellation.Cancel();
            }
        };
        cartService.CartChanged += listener;
        // Wait until connection closes
        for (long i = 0; !cancellationToken.IsCancellationRequested && !errorCancellation.Token.IsCancellationRequested; i++) {
            await Task.Delay(1000);
            // Send an empty line for keepalive from time to time
            if (i % RpcSettings.KeepAliveSeconds == 0) {
                try {
                    await WriteAndFlush(Response, "\n", writeLock);
                } catch {
                    errorCancellation.Cancel();
                }
            }
        }
        // When the connection closes, unregister the event listener
        cartService.CartChanged -= listener; 
    }

    private async Task WriteAndFlush(HttpResponse response, string text, SemaphoreSlim writeLock) {

        try {
            await writeLock.WaitAsync();
            await Response.WriteAsync(text);
            await Response.Body.FlushAsync();
        } finally {
            writeLock.Release();
        }
    }

}