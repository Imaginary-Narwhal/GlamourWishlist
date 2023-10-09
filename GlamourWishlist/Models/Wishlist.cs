using System.Collections.Generic;

namespace GlamourWishlist.Models;
public class Wishlist
{
    public int Id { get; set; } //doubles as order
    public string Name { get; set; }
    public List<uint> ItemIds { get; set; }

    public Wishlist()
    {
        ItemIds = new();
    }
}
