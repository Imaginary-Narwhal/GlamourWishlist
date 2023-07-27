using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlamourWishlist.Services;
using GlamourWishlist;
using GlamourWishlist.Models;
using static System.Net.Mime.MediaTypeNames;

namespace GlamourWishlist.Services;
public class WishlistService
{
    //addon config location: %appdata%\XIVLauncher\pluginConfigs\GlamourWishlist.json



    private Plugin Plugin;

    public WishlistService(Plugin plugin)
    {
        Plugin = plugin;
    }

    public void AddWishlist(string name)
    {
        var nextId = 0;

        if (Plugin.Config.LocalWishlists.Count > 0)
        {
            var lastWishlist = Plugin.Config.LocalWishlists.OrderBy(x=>x.Id).Last();
            nextId = lastWishlist.Id + 1;
        }

        Plugin.Config.LocalWishlists.Add(new Wishlist
        {
            Id = nextId,
            Name = name
        });

        Plugin.Config.Save();
    }

    public void EditWishlist(string name, Wishlist selectedWishlist)
    {
        var wl = Plugin.Config.LocalWishlists.Where(x => x.Id == selectedWishlist.Id).First();
        wl.Name = name;
        Plugin.Config.Save();
    }

    public void DeleteWishlist(Wishlist selectedWishlist) 
    {
        Plugin.Config.LocalWishlists.Remove(selectedWishlist);
        int i = 0;
        foreach(var wishlist in Plugin.Config.LocalWishlists.OrderBy(x=>x.Id))
        {
            wishlist.Id = i;
            i++;
        }
        Plugin.Config.Save();
    }

    public int GetLastWishlistId()
    {
        return Plugin.Config.LocalWishlists.Last().Id;
    }

    internal void RemoveItem(uint itemId, Wishlist selectedWishlist)
    {
        var selected = Plugin.Config.LocalWishlists.Where(x => x.Id == selectedWishlist.Id).First();
        selected.ItemIds.Remove(itemId);
        Plugin.Config.Save();
    }

    internal void AddItem(uint itemId, Wishlist selectedWishlist)
    {
        var selected = Plugin.Config.LocalWishlists.Where(x => x.Id == selectedWishlist.Id).First();
        selected.ItemIds.Add(itemId);
        Plugin.Config.Save();
    }
}
