using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GlamourWishlist.Services;
public class DrawService
{
    public readonly Dictionary<ushort, TextureWrap> textureDictionary;

    public DrawService()
    {
        textureDictionary = new();
    }

    public void DrawIcon(ushort icon, Vector2 size)
    {
        if (icon < 65000)
        {
            if (textureDictionary.ContainsKey(icon))
            {
                var tex = textureDictionary[icon];
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
                    ImGui.Image(textureDictionary[icon].ImGuiHandle, size);
                }
            }
            else
            {
                ImGui.BeginChild("WaitingTexture", size, true);
                ImGui.EndChild();

                textureDictionary[icon] = null;

                Task.Run(() => {
                    try
                    {
                        var iconTex = Service.DataManager.GetIcon(icon);
                        var tex = Service.Interface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);
                        if (tex != null && tex.ImGuiHandle != IntPtr.Zero)
                        {
                            textureDictionary[icon] = tex;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                });
            }
        }
    }
}
