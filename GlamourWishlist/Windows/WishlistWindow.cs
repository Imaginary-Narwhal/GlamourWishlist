using Dalamud.Interface;
using Dalamud.Interface.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using GlamourWishlist.Models;
using GlamourWishlist.Services;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;

namespace GlamourWishlist.Windows;
public class WishlistWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    private ImGuiWindowFlags DefaultFlags;
    private Wishlist SelectedWishlist;

    public WishlistWindow(Plugin plugin) : base(
        "Glamour Wishlists",
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse |
        ImGuiWindowFlags.NoDocking)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(450, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.Plugin = plugin;
        DefaultFlags = this.Flags;
    }

    public void Dispose()
    {
        
    }

    private string WishlistName = "";
    private DialogError DialogError = DialogError.None;

    private bool IsAddingNewWishlist;
    private bool IsEditingWishlist;
    private bool IsDeletingWishlist;

    private bool EnterOrOkPressed;

    public override void Draw()
    {
        ImGui.BeginTable("mainWishlistTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("Wish Lists", ImGuiTableColumnFlags.WidthFixed
            | ImGuiTableColumnFlags.NoSort
            );
        ImGui.TableSetupColumn(SelectedWishlist == null ? "" : $"Items tracked {SelectedWishlist.ItemIds.Count}", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        ImGui.TableNextColumn();
        Draw_WishlistColumn();

        ImGui.TableNextColumn();
        if (SelectedWishlist != null)
        {
            Draw_ItemsColumn();
        }

        ImGui.EndTable();
    }

    private void Draw_WishlistColumn()
    {
        var windowSize = ImGui.GetWindowSize();
        var wlChildSize = new Vector2(300, Math.Max(50 * ImGui.GetIO().FontGlobalScale, windowSize.Y - ImGui.GetCursorPosY() - 10 * ImGui.GetIO().FontGlobalScale) - 10);
        ImGui.BeginChild("wlscroll", wlChildSize, true);
        ImGui.BeginTable("wishlisttables", 3, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("##wishlistname", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("##editwishlistname", ImGuiTableColumnFlags.WidthFixed
            | ImGuiTableColumnFlags.NoSort
            | ImGuiTableColumnFlags.NoReorder
            | ImGuiTableColumnFlags.NoResize
            , 17);
        ImGui.TableSetupColumn("##deletewishlistname", ImGuiTableColumnFlags.WidthFixed
            | ImGuiTableColumnFlags.NoSort
            | ImGuiTableColumnFlags.NoReorder
            | ImGuiTableColumnFlags.NoResize
            , 17);
        foreach (var wishlist in Plugin.Config.LocalWishlists.OrderBy(x => x.Id))
        {
            var selected = false;
            if (SelectedWishlist != null)
            {
                selected = SelectedWishlist.Id == wishlist.Id;
            }

            ImGui.TableNextColumn();
            if (ImGui.Selectable(wishlist.Name, selected))
            {
                SelectedWishlist = wishlist;
            }

            if(ImGui.IsItemActive() && !ImGui.IsItemHovered())
            {
                var n = wishlist.Id;

                int n_next = n + (ImGui.GetMouseDragDelta(0).Y < 0.0f ? -1 : 1);
                if(n_next >= 0 && n_next < Plugin.Config.LocalWishlists.Count)
                {
                    var selectedWishlist = Plugin.Config.LocalWishlists.First(x=>x.Id == n);
                    var otherWishlist = Plugin.Config.LocalWishlists.First(x => x.Id == n_next);

                    selectedWishlist.Id = n_next;
                    otherWishlist.Id = n;

                    ImGui.ResetMouseDragDelta();
                    Plugin.Config.Save();
                }
            }

            if (selected)
            {
                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Text($"{(char)FontAwesomeIcon.Edit}");
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Edit Wishlist Name");
                }
                if (ImGui.IsItemClicked())
                {
                    if (SelectedWishlist != null)
                    {
                        IsEditingWishlist = true;
                    }
                }

                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.Text($"{(char)FontAwesomeIcon.Trash}");
                ImGui.PopFont();
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip("Delete wishlist");
                }
                if (ImGui.IsItemClicked())
                {
                    IsDeletingWishlist = true;
                }
            }
            else
            {
                ImGui.TableNextColumn();
                ImGui.TableNextColumn();
            }

        }
        ImGui.EndTable();
        ImGui.EndChild();

        ImGui.SetCursorPos(new Vector2(293, 31.5f));
        ImGui.PushStyleColor(ImGuiCol.Button, 0x00000000);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0x00000000);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0x00000000);
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Button($"{(char)FontAwesomeIcon.Plus}");
        ImGui.PopFont();
        ImGui.PopStyleColor(3);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Add new Wishlist");
        }
        if (ImGui.IsItemClicked())
        {
            IsAddingNewWishlist = true;
        }


        if (IsAddingNewWishlist)
        {
            WishlistName = "";
            DialogError = DialogError.None;
            ImGui.OpenPopup("AddWishlist");
        }

        if (IsEditingWishlist)
        {
            WishlistName = SelectedWishlist.Name;
            DialogError = DialogError.None;
            ImGui.OpenPopup("AddWishlist");
        }

        if (IsDeletingWishlist)
        {
            ImGui.OpenPopup("DeleteWishlist");
        }



        Draw_WLNamePopup();
        Draw_WLDeletePopup();
    }
    private string ItemSearch = "";
    private void Draw_ItemsColumn()
    {
        ImGui.BeginTable("##search", 2);
        ImGui.TableSetupColumn("##searchbar", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("##clearbar", ImGuiTableColumnFlags.WidthFixed
            | ImGuiTableColumnFlags.NoSort
            | ImGuiTableColumnFlags.NoReorder
            | ImGuiTableColumnFlags.NoResize
            , 17);
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##itemsearch", "Search for items", ref ItemSearch, 100);
        ImGui.PushStyleColor(ImGuiCol.Separator, 0xFF3b3b40);

        ImGui.TableNextColumn();

        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Button($"{(char)FontAwesomeIcon.SquareXmark}");
        ImGui.PopFont();
        if(ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Clear search field.");
        }
        if(ImGui.IsItemClicked())
        {
            ItemSearch = "";
        }

        ImGui.EndTable();
        ImGui.Separator();
        ImGui.PopStyleColor();

        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1);
        ImGui.BeginChild("itemscroll", new Vector2(0, 0));


        var foundItems = Service.Items.Where(x =>
                    (ItemSearch.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToList())
                    .All(s => x.Name.ToString().Contains(s, StringComparison.CurrentCultureIgnoreCase))).ToList();
        List<Item> ActiveItemDisplay = new();
        List<Item> InactiveItemDisplay = new();

        if (SelectedWishlist.ItemIds.Count > 0)
        {
            ActiveItemDisplay.AddRange(
                Service.Items.Where(x=>SelectedWishlist.ItemIds.Contains(x.RowId)).ToList());
        }
        InactiveItemDisplay.AddRange(
            foundItems.Where(x => !SelectedWishlist.ItemIds.Contains(x.RowId)).ToList());
        
        var windowSize = ImGui.GetWindowSize();
        if (ActiveItemDisplay.Count > 0)
        {
            
            ImGui.BeginChild("selectedItemsScroll", new Vector2(0, Plugin.Config.ItemSeparatorSize), true);
            ImGuiListClipperPtr ActiveClipper;
            unsafe { ActiveClipper = new(ImGuiNative.ImGuiListClipper_ImGuiListClipper()); }
            ActiveClipper.Begin(ActiveItemDisplay.Count);
            while (ActiveClipper.Step())
            {
                for (int i = ActiveClipper.DisplayStart; i < ActiveClipper.DisplayEnd; i++)
                {
                    var sItem = ActiveItemDisplay[i];

                    ImGui.BeginGroup();

                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7);
                    ImGui.PushFont(UiBuilder.IconFont);
                    ImGui.Text($"{(char)FontAwesomeIcon.Heart}");
                    ImGui.PopFont();


                    ImGui.SameLine();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 7);
                    Service.DrawService.DrawIcon(sItem.Icon, new Vector2(32, 32));

                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, Helper.Rarity(sItem.Rarity));
                    ImGui.TextUnformatted($"{sItem.Name}");
                    ImGui.PopStyleColor();

                    ImGui.EndGroup();
                    if (ImGui.IsItemClicked())
                    {
                        Service.WishlistService.RemoveItem(sItem.RowId, SelectedWishlist);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    }
                }
            }

            ImGui.EndChild();
            ImGui.Button("##spacer", new Vector2(windowSize.X, 10));
            if(ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNS);
            }
            if(ImGui.IsItemActive() && !ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNS);
                var move = ImGui.GetMouseDragDelta(0).Y;
                Plugin.Config.ItemSeparatorSize += move;
                if(Plugin.Config.ItemSeparatorSize < 150)
                {
                    Plugin.Config.ItemSeparatorSize = 150;
                }
                if(Plugin.Config.ItemSeparatorSize > windowSize.Y * .5f)
                {
                    Plugin.Config.ItemSeparatorSize = windowSize.Y * .5f;
                }
                ImGui.ResetMouseDragDelta();
                Plugin.Config.Save();
            }
        }

        ImGui.BeginChild("unselectedItemScroll", new Vector2(0, 0), true);


        ImGuiListClipperPtr InActiveClipper;
        unsafe { InActiveClipper = new(ImGuiNative.ImGuiListClipper_ImGuiListClipper()); }
        InActiveClipper.Begin(InactiveItemDisplay.Count);
        while (InActiveClipper.Step())
        {
            for (int i = InActiveClipper.DisplayStart; i < InActiveClipper.DisplayEnd; i++)
            {
                var uItem = InactiveItemDisplay[i];
                ImGui.BeginGroup();

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 7);
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF000000);
                ImGui.Text($"{(char)FontAwesomeIcon.Heart}");
                ImGui.PopStyleColor();
                ImGui.PopFont();


                ImGui.SameLine();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 7);
                Service.DrawService.DrawIcon(uItem.Icon, new Vector2(32, 32));

                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, Helper.Rarity(uItem.Rarity));
                ImGui.TextUnformatted($"{uItem.Name}");
                ImGui.PopStyleColor();

                ImGui.EndGroup();
                if (ImGui.IsItemClicked())
                {
                    Service.WishlistService.AddItem(uItem.RowId, SelectedWishlist);
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                }
            }
        }
        ImGui.EndChild();
        ImGui.PopStyleVar();

        ImGui.EndChild();
    }


    //popups


    private bool isAdd;
    private void Draw_WLNamePopup()
    {
        if (ImGui.BeginPopup($"AddWishlist", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if (ImGui.IsWindowAppearing())
            {
                EnterOrOkPressed = false;
                isAdd = IsAddingNewWishlist;
                IsAddingNewWishlist = false;
                IsEditingWishlist = false;
            }
            ImGui.Text("Enter name for new wishlist:");
            if (DialogError == DialogError.None)
            {
                ImGui.Text("");
            }
            if (DialogError == DialogError.Duplicate)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
                ImGui.Text("Wishlist name already exists. Try again.");
                ImGui.PopStyleColor();
            }
            if (DialogError == DialogError.EmptyOrNull)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
                ImGui.Text("Wishlist name cannot be empty. Try again.");
                ImGui.PopStyleColor();
            }
            if (ImGui.IsWindowFocused() && !ImGui.IsAnyItemActive())
            {
                ImGui.SetKeyboardFocusHere();
            }

            if (ImGui.InputText("##WlName", ref WishlistName, 30, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
            {
                EnterOrOkPressed = true;
            }
            ImGui.PopAllowKeyboardFocus();
            if (ImGui.Button("Ok", new Vector2(120, 0)))
            {
                EnterOrOkPressed = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
            }


            if (EnterOrOkPressed)
            {
                EnterOrOkPressed = false;
                var wishlist = Plugin.Config.LocalWishlists.FirstOrDefault(x => string.Equals(x.Name, WishlistName, StringComparison.InvariantCultureIgnoreCase));
                if (wishlist != null)
                {
                    DialogError = DialogError.Duplicate;
                }
                else if (string.IsNullOrWhiteSpace(WishlistName))
                {
                    DialogError = DialogError.EmptyOrNull;
                    WishlistName = "";
                }
                else
                {
                    if (isAdd)
                    {
                        Service.WishlistService.AddWishlist(WishlistName);
                    }
                    else
                    {
                        var oldWishlist = Plugin.Config.LocalWishlists.First(x => x.Id == SelectedWishlist.Id);
                        Service.WishlistService.EditWishlist(WishlistName, oldWishlist);
                    }
                    WishlistName = "";
                    ImGui.CloseCurrentPopup();
                }
            }

            ImGui.EndPopup();
        }
    }

    private void Draw_WLDeletePopup()
    {
        if (ImGui.BeginPopup($"DeleteWishlist", ImGuiWindowFlags.AlwaysAutoResize))
        {
            if(ImGui.IsWindowAppearing())
            {
                IsDeletingWishlist = false;
            }
            ImGui.Text("Are you sure you want to Delete this wishlist?");

            if (ImGui.Button("Yes", new Vector2(120, 0)))
            {
                Service.WishlistService.DeleteWishlist(SelectedWishlist);
                ImGui.CloseCurrentPopup();
                SelectedWishlist = null;
            }
            ImGui.SameLine();
            if (ImGui.Button("No", new Vector2(120, 0)))
            {
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }
}
