using CartService;
using CartService.Model;
using TestClient;
using Utils;

ICartService cartService = new CartServiceClient() {
    BaseUrl = "http://localhost:5000/"
};

cartService.CartChanged += cart => {
    Console.WriteLine(">>> Event: " + JsonUtils.Serialize(cart) + "\n\n");
};

Console.WriteLine("Press enter to add another article from this client.\n");

while (true) {
    try {
        var cart = await cartService.AddArticle(new Article("CC", "Coke", 150), 1);
        Console.WriteLine("+++ Added: " + JsonUtils.Serialize(cart) + "\n\n");
    } catch (RpcException ex) {
        if (ex.Failure == RpcFailureType.NetworkFailure)
            Console.WriteLine("Call failed: Network problem: " + ex.Message);
        else if (ex.Failure == RpcFailureType.RemoteException)
            Console.WriteLine("Exception on remote side: " + ex.Message);
        else
            Console.WriteLine("Unexpected RPC error: " + ex.Message);
    } catch (Exception ex) {
        Console.WriteLine("Unexpected RPC error: " + ex.Message);
    }
    
    Console.ReadLine();
}
