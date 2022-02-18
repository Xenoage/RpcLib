namespace CartService.Model;

public static class CartEx {

    public static Cart AddArticles(this Cart cart, Article article, int count) {
        // Check parameters
        if (count < 1 || article.Price < 0)
            throw new ArgumentException("Illegal parameters");
        // Find existing article in items.
        int itemIndex = cart.Items.FindIndex(it => it.Article.ID == article.ID);
        var ret = cart;
        if (itemIndex > -1) {
            // Existing items found. Add it there.
            // Check data
            var item = cart.Items[itemIndex];
            if (item.Article != article)
                throw new Exception("Article data not consistent");
            ret = cart with {
                Items = cart.Items.SetItem(itemIndex, new Item(item.Count + count, article))
            };
        } else {
            // Create new items
            ret = cart with {
                Items = cart.Items.Add(new Item(count, article))
            };
        }
        return ret;
    }

    public static int GetTotalPrice(this Cart cart) =>
        cart.Items.Sum(it => it.Count * it.Article.Price);

}
