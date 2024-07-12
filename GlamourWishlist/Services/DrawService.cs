using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace GlamourWishlist.Services;
public class DrawService
{
    public readonly Dictionary<ushort, ISharedImmediateTexture> textureDictionary;

    public DrawService()
    {
        textureDictionary = [];
    }

    public void DrawIcon(ushort icon, Vector2 size)
    {
        if (icon < 65000)
        {
            if (textureDictionary.TryGetValue(icon, out ISharedImmediateTexture value))
            {
                var tex = value.GetWrapOrEmpty();
                if (tex == null || tex.ImGuiHandle == IntPtr.Zero)
                {
                    ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                    ImGui.BeginChild("FailedTexture", size, true);
                    ImGui.Text(icon.ToString());
                    ImGui.EndChild();
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.Image(value.GetWrapOrEmpty().ImGuiHandle, size);
                }
            }
            else
            {
                ImGui.BeginChild("WaitingTexture", size, true);
                ImGui.EndChild();

                textureDictionary[icon] = null;

                var tex = GetSharedTexture(icon);
                if (tex != null)
                { 
                    textureDictionary[icon] = tex;
                }
            }
        }
    }

    private static ISharedImmediateTexture GetSharedTexture(ushort icon)
    {
        try
        {
            var tex = Service.TextureProvider.GetFromGameIcon(new GameIconLookup(icon));
            if (tex != null && tex.GetWrapOrEmpty().ImGuiHandle != IntPtr.Zero)
            {
                return tex;
            }
            else
            { 
                return null; 
            }
        }
        catch
        {
            return null;
        }
    }
}
