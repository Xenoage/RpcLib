namespace CartService.Model;

/// <summary>
/// One ore more instances of an <see cref="Article"/> accumulated.
/// </summary>
public record Item(int Count, Article Article);