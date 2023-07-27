using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
