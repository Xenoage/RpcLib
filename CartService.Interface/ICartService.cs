namespace CartService;

using CartService.Model;

/// <summary>
/// Public interface of the Shopping Cart Service.
/// </summary>
public interface ICartService {

    /// <summary>
    /// Adds the given article (one or more) to the cart.
    /// If articles with the same <see cref="IArticle.ID"/> are already within the cart,
    /// their name and price must match.
    /// </summary>
    /// <returns>The updated cart.</returns>
    Task<Cart> AddArticle(Article article, int count);

    /// <summary>
    /// Gets the total price of all items in the shopping carts.
    /// </summary>
    Task<int> GetTotalPrice();

    /// <summary>
    /// This event is raised whenever the cart was changed. The updated cart is given.
    /// </summary>
    event Action<Cart> CartChanged;

}
