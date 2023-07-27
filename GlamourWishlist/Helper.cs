using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlamourWishlist;
public static class Helper
{
    public static uint Rarity(int r)
    {
        switch (r)
        {
            case 1:
                return 0xFFF3F3D5;

            case 2:
                return 0xFFC0FFC0;

            case 3:
                return 0xFFFF9059;

            case 4:
                return 0xFFDF8CB3;

            case 7:
                return 0xFF8789D7;

            default:
                return 0xFFF3F3D5;
        }
    }
}
