using GlamourWishlist.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlamourWishlist.Models;
public class Character
{
    public string Name { get; set; }
    public string World { get; set; }
    public List<Wishlist> Wishlists { get; set; }
}
