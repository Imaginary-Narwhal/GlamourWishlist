using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;

#nullable disable

namespace GlamourWishlist.Services;
public class Service
{
    public static void Initialize(IDalamudPluginInterface _pluginInterface)
    {
        _pluginInterface.Create<Service>();
    }

    [PluginService] public static IChatGui ChatGui { get; private set; } = null;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null;
    [PluginService] public static IDataManager DataManager { get; private set; } = null;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null;
    [PluginService] public static IFramework Framework { get; private set; } = null;
    [PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null;
    [PluginService] public static IClientState ClientState { get; private set; } = null;
    [PluginService] public static IGameGui GameGui { get; private set; } = null;
    public static DrawService DrawService { get; set; }
    public static WishlistService WishlistService { get; set; }
    //public static ContextMenuService ContextMenuService { get; set; }
    public static List<Item> Items { get; set; }

    public static void Message(string msg)
        => Service.ChatGui.Print(new SeStringBuilder()
            .AddUiForeground("[GlamourWishlist] ", 48)
            .AddText(msg)
            .Build());
}
