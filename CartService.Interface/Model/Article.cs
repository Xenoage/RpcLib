namespace CartService.Model;

/// <summary>
/// A product with a globally unique ID, a name and a price.
/// </summary>
public record Article(string ID, string Name, int Price);
