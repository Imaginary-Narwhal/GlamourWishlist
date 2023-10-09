using Dalamud.Configuration;
using Dalamud.Plugin;
using GlamourWishlist.Models;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace GlamourWishlist;
[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public List<Character> Characters { get; set; } = new();
    public float ItemSeparatorSize = 150.0f;

    // the below exist just to make saving less cumbersome
    [NonSerialized] private DalamudPluginInterface? PluginInterface;
    [NonSerialized] public List<Wishlist> LocalWishlists = new();
    [NonSerialized] public Character CurrentCharacter = new();

    public void Initialize(DalamudPluginInterface pluginInterface, Dalamud.Game.Text.SeStringHandling.SeString name, Lumina.Text.SeString world)
    {
        this.PluginInterface = pluginInterface;
        Login(name, world);
    }

    public void Save()
    {
        var cc = Characters.First(x => x.Name == CurrentCharacter.Name && x.World == CurrentCharacter.World);
        cc.Wishlists = LocalWishlists;

        this.PluginInterface!.SavePluginConfig(this);
    }

    public void Login(Dalamud.Game.Text.SeStringHandling.SeString name, Lumina.Text.SeString world)
    {
        var chara = Characters.FirstOrDefault(x => x.Name == name.ToString() && x.World == world.ToString());
        if (chara != null)
        {
            CurrentCharacter = chara;
            LocalWishlists = chara.Wishlists;
        }
        else
        {
            LocalWishlists = new();
            CurrentCharacter = new()
            {
                Name = name.ToString(),
                World = world.ToString(),
                Wishlists = LocalWishlists
            };

            Characters.Add(CurrentCharacter);
            Save();
        }
    }
}
