using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using GlamourWishlist.Attributes;
using GlamourWishlist.CommandHandler;
using GlamourWishlist.Services;
using GlamourWishlist.Windows;
using Lumina.Excel.GeneratedSheets;
using System.Linq;

#nullable disable

namespace GlamourWishlist;
public sealed class Plugin : IDalamudPlugin
{
    public static string Name => "GlamourWishlist";

    internal Configuration Config { get; init; }
    
    public WindowSystem WindowSystem = new("GlamourWishlist");

    private PluginCommandManager<Plugin> PluginCommandManager { get; init; }

    public WishlistWindow MainWindow { get; init; }
    public QuickAddToWishlist QuickAdd { get; init; }

    public Plugin(DalamudPluginInterface _pluginInterface)
    {
        Service.Initialize(_pluginInterface);
        Service.DrawService = new();
        Service.WishlistService = new(this);
        Service.ContextMenuService = new(this);

        

        Service.ClientState.Login += ClientState_Login;

        PluginCommandManager = new(this);

        Config = Service.Interface.GetPluginConfig() as Configuration ?? new Configuration();
        if (Service.ClientState.LocalPlayer != null)
        {
            Config.Initialize(Service.Interface,
                Service.ClientState.LocalPlayer.Name,
                Service.ClientState.LocalPlayer.HomeWorld.GameData.Name);
        }

        MainWindow = new WishlistWindow(this);
        WindowSystem.AddWindow(MainWindow);

        QuickAdd = new(this);
        WindowSystem.AddWindow(QuickAdd);

        Service.Interface.UiBuilder.Draw += DrawUI;

        Service.Items = Service.DataManager.GetExcelSheet<Item>()
            .Where(x => x.IsGlamourous && x.Name != string.Empty && x.RowId > 1600)
            .ToList();
    }

    private void ClientState_Login()
    {
        Config.Login(Service.ClientState.LocalPlayer.Name, Service.ClientState.LocalPlayer.HomeWorld.GameData.Name);
    }

    public void Dispose()
    {
        PluginCommandManager.Dispose();
        WindowSystem.RemoveAllWindows();
        Service.Interface.UiBuilder.Draw -= DrawUI;
        Service.DrawService.textureDictionary.Clear();

        Service.ContextMenuService.Dispose();
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    [Command("/pwl")]
    [HelpMessage("Open main wishlist window")]
    public void WishlistCommand(string _, string __)
    {
        MainWindow.Toggle();
    }
}
