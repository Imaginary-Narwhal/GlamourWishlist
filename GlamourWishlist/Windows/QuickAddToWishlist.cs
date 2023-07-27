using Dalamud.Interface.Windowing;
using GlamourWishlist.Services;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GlamourWishlist.Windows;
public class QuickAddToWishlist : Window, IDisposable
{
    private readonly Plugin Plugin;

    public Item Item { get; set; }
    public QuickAddToWishlist(Plugin plugin) : base (
        "Add to Wishlist",
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse |
        ImGuiWindowFlags.NoResize |
        ImGuiWindowFlags.NoTitleBar)
    {
        this.SizeConstraints = new()
        {
            MinimumSize = new Vector2(200, 300),
            MaximumSize = new Vector2(200, 300)
        };
        Plugin = plugin;
    }

    public override void OnOpen()
    {
        this.Position = new()
        {
            X = ImGui.GetMousePos().X - 100,
            Y = ImGui.GetMousePos().Y - 100
        };
    }

    public void Dispose()
    {

    }

    public override void OnClose()
    {
        Item = null;
    }

    public override void Draw()
    {
        ImGui.Text($"Add to wishlist:");
        ImGui.PushStyleColor(ImGuiCol.Text, Helper.Rarity(Item.Rarity));
        ImGui.TextWrapped($"{Item.Name}");
        ImGui.PopStyleColor();
        ImGui.PushStyleColor(ImGuiCol.Separator, 0xFF403b3b);
        ImGui.Separator();
        ImGui.PopStyleColor();

        var windowSize = ImGui.GetWindowSize();
        var wlChildSize = new Vector2(200, Math.Max(50 * ImGui.GetIO().FontGlobalScale, windowSize.Y - ImGui.GetCursorPosY() - 10 * ImGui.GetIO().FontGlobalScale) - 25);

        ImGui.BeginChild("##quickWishlists", wlChildSize, true);
        foreach(var wishlist in Plugin.Config.LocalWishlists.Where(x=>!x.ItemIds.Contains(Item.RowId)).ToList())
        {
            if(ImGui.Selectable(wishlist.Name, false))
            {
                Service.WishlistService.AddItem(Item.RowId, wishlist);
                IsOpen = false;
            }
        }
        ImGui.EndChild();

        if(ImGui.Button("Cancel", new Vector2(-1, 25)))
        {
            IsOpen = false;
        }
    }
}
