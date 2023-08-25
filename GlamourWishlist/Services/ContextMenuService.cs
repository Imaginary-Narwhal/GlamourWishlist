using Dalamud.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlamourWishlist.Services;
public class ContextMenuService
{
    private readonly Plugin Plugin;
    public uint SelectedItemId { get; set; }

    private readonly DalamudContextMenu contextMenu;
    public ContextMenuService(Plugin plugin) 
    {
        Plugin = plugin;
        contextMenu = new();
        contextMenu.OnOpenInventoryContextMenu += OpenInventoryContextMenu;
        contextMenu.OnOpenGameObjectContextMenu += OpenObjectContextMenu;
    }

    private uint? GetGameObjectItemId(GameObjectContextMenuOpenArgs args)
    {
        var itemid = args.ParentAddonName switch
        {
            null => null,
            "RecipeNote" => CheckGameObjectItem("RecipeNote", RecipeNoteContextItemId),
            "RecipeTree" => CheckGameObjectItem(AgentById(AgentId.RecipeItemContext), AgentItemContextItemId),
            "RecipeMaterialList" => CheckGameObjectItem(AgentById(AgentId.RecipeItemContext), AgentItemContextItemId),
            "ItemSearch" => CheckGameObjectItem(args.Agent, ItemSearchContextItemId),
            "ChatLog" => CheckGameObjectItem("ChatLog", ChatLogContextItemId),
            "ContentsInfoDetail" => CheckGameObjectItem("ContentsInfo", ContentsInfoDetailContextItemId),
            "ShopExchangeCurrency" => CheckGameObjectItem("ShopExchangeCurrency", ShopExchangeCurrencyContextItemId),
            "ShopExchangeItem" => CheckGameObjectItem("ShopExchangeItem", ShopExchangeItemContextItemId),
            "Shop" => CheckGameObjectItem("Shop", ShopContextMenuItemId),
            _ => null
        };

        if (itemid == null)
        {
            var guiHoveredItem = Service.GameGui.HoveredItem;
            if (guiHoveredItem >= 2000000 || guiHoveredItem == 0) return null;
            itemid = (uint)guiHoveredItem % 500_000;
        }

#nullable enable
        Item? item = Service.Items.FirstOrDefault(x => x.RowId == itemid);
#nullable disable

        if (item != null)
        {
            if (!item.IsGlamourous)
                return null;
        }

        return itemid;
    }

    private void OpenObjectContextMenu(GameObjectContextMenuOpenArgs args)
    {
        var gameObjectItemId = GetGameObjectItemId(args);
        if (gameObjectItemId != null)
        {
            GameObjectContextMenuItem menuItem =
                new(new SeString(new TextPayload($"{(char)0xe03b} Add to wishlist")), x => AddItemToWishlist(gameObjectItemId.Value), false);
            args.AddCustomItem(menuItem);
        }
    }

    private void OpenInventoryContextMenu(InventoryContextMenuOpenArgs args)
    {
        var guiItem = Service.GameGui.HoveredItem;
        if(guiItem < 2000000 || guiItem != 0)
        {
#nullable enable
            Item? item = Service.Items.FirstOrDefault(x => x.RowId == ((uint)guiItem % 500_000));
#nullable disable
            if(item != null)
            {
                if(item.IsGlamourous)
                {
                    InventoryContextMenuItem menuItem =
                        new(new SeString(new TextPayload($"{(char)0xe03b} Add to wishlist")), x => AddItemToWishlist(x.ItemId), false); 
                    args.AddCustomItem(menuItem);
                }
            }
        }
    }

    private void AddItemToWishlist(uint ItemId)
    {
        var item = Service.Items.Where(x => x.RowId == ItemId).FirstOrDefault();
        if(item != null)
        {
            
            Plugin.QuickAdd.Item = item;
            Plugin.QuickAdd.IsOpen = true;
        }
    }

    private uint CheckGameObjectItem(uint itemId)
    {
        if (itemId > 500000)
            itemId -= 500000;

        return itemId;
    }

    private uint? CheckGameObjectItem(string name, int offset)
    {
        return CheckGameObjectItem(Service.GameGui.FindAgentInterface(name), offset);
    }

    private unsafe uint? CheckGameObjectItem(IntPtr agent, int offset)
        =>agent != IntPtr.Zero ? CheckGameObjectItem(*(uint*)(agent + offset)) : null;
    

    private unsafe nint AgentById(AgentId id)
    {
        var uiModule = (UIModule*)Service.GameGui.GetUIModule();
        var agents = uiModule->GetAgentModule();
        var agent = agents->GetAgentByInternalId(id);
        return (IntPtr)agent;
    }

    public const int RecipeNoteContextItemId = 0x398;
    public const int AgentItemContextItemId = 0x28;
    public const int ItemSearchContextItemId = 0x1738;
    public const int ChatLogContextItemId = 0x948;
    public const int ContentsInfoDetailContextItemId = 0x17CC;
    public const int ShopExchangeItemContextItemId = 0x54;
    public const int ShopContextMenuItemId = 0x54;
    public const int ShopExchangeCurrencyContextItemId = 0x54;

    internal void Dispose()
    {
        contextMenu.OnOpenInventoryContextMenu -= OpenInventoryContextMenu;
        contextMenu.OnOpenGameObjectContextMenu -= OpenObjectContextMenu;
        contextMenu.Dispose();
    }
}
