using System.Collections.Generic;

namespace GlamourWishlist.Models;
public class Character
{
    public string Name { get; set; }
    public string World { get; set; }
    public List<Wishlist> Wishlists { get; set; }
}
