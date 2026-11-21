using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChillCore.Gui.IceGui;

internal partial class ImGui_Ice
{
    // Typically I JUST need the icon + string in the sameline,
    // so this works for me to throw the text -> Icon
    public static void Icon(FontAwesomeIcon icon, string? s = null)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();
        if (s != null)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted(s);
        }
    }

    public static void Icon(FontAwesomeIcon icon, Vector4 color, string? s = null)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, ImGui.ColorConvertFloat4ToU32(color)))
        {
            using (ImRaii.PushFont(UiBuilder.IconFont))
            {
                ImGui.TextUnformatted(icon.ToIconString());
            }
        }
        if (s != null)
        {
            ImGui.SameLine();
            ImGui.TextUnformatted(s);
        }
    }

    /// <summary>
    /// Icon => FontAwesomeIcon <br></br>
    /// Tooltip => Tooltip information, just a basic string <br></br>
    /// Sameline => Used to put it on the same line as the item before it. NOT AFTER. So it would be like... checkbox -> sameline -> tooltip
    /// </summary>
    /// <param name="icon"></param>
    /// <param name="tooltip"></param>
    /// <param name="sameLine"></param>
    public static void IconWithTooltip(FontAwesomeIcon icon, string? tooltip = null, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();

        if (tooltip != null && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(tooltip);
        }
    }
}
