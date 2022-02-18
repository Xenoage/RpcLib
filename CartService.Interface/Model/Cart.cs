namespace CartService.Model; 

using System.Collections.Immutable;

/// <summary>
/// A list of <see cref="Item"/> in a shopping cart.
/// </summary>
public record Cart(ImmutableList<Item> Items);