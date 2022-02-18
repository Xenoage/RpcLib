using CartService.Model;
using System.Collections.Immutable;

namespace CartService;

/// <summary>
/// Real implementation of the <see cref="ICartService"/>.
/// </summary>
public class CartService : ICartService {

    private Cart cart = new Cart(ImmutableList<Item>.Empty);

    public Task<Cart> AddArticle(Article article, int count) {
        cart = cart.AddArticles(article, count);
        CartChanged(cart);
        return Task.FromResult<Cart>(cart);
    }

    public Task<int> GetTotalPrice() =>
        Task.FromResult(cart.GetTotalPrice());

    public event Action<Cart> CartChanged = delegate { };

}