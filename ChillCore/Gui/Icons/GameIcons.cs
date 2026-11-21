using ChillCore.Gui.IceGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;
using ECommons;

namespace ChillCore.Gui;

public static partial class GameIcons
{
    public static int MaximumSize { get; set; } = 512;

    // Applied on top of grayscale so "disabled" icons don't just look black-and-white.
    public static float GreyBrightness { get; set; } = 0.85f;
    public static float GreyOpacity { get; set; } = 0.5f;

    public static bool DrawInline(uint iconId, bool sameLine = true, bool grey = false) => DrawInline(new GameIconLookup(iconId), sameLine, grey);

    public static bool DrawInline(GameIconLookup lookup, bool sameLine = true, bool grey = false)
    {
        var size = MathF.Round(ImGui.GetTextLineHeightWithSpacing());
        if (!Draw(lookup, new Vector2(size), grey)) return false;
        if (sameLine) ImGui.SameLine();
        return true;
    }

    public static void DrawInlineOrIcon(uint? iconId, FontAwesomeIcon fallback, Vector4? fallbackColor = null, bool grey = false)
    {
        if (iconId is { } id && DrawInline(id, grey: grey)) return;
        ImGui_Ice.Icon(fallback, fallbackColor ?? ImGui.GetStyle().Colors[(int)ImGuiCol.Text]);
    }

    public static bool Draw(uint iconId, float size, bool grey = false) => Draw(new GameIconLookup(iconId), new Vector2(size), grey);
    public static bool Draw(uint iconId, Vector2 size, bool grey = false) => Draw(new GameIconLookup(iconId), size, grey);

    public static bool Draw(GameIconLookup lookup, Vector2 size, bool grey = false)
    {
        var width = (int)MathF.Round(size.X);
        var height = (int)MathF.Round(size.Y);
        if (Get(lookup, width, height, grey) is not { } texture) return false;
        var position = ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(new Vector2(MathF.Round(position.X), MathF.Round(position.Y)));
        ImGui.Image(texture.Handle, new Vector2(width, height));
        return true;
    }

    public static bool DrawButton(uint iconId, string id, Vector2 buttonSize, Vector2? iconSize = null, Vector4? hoveredColor = null, Vector4? activeColor = null, bool grey = false)
    => DrawButton(new GameIconLookup(iconId), id, buttonSize, iconSize, hoveredColor, activeColor, grey);

    public static bool DrawButton(GameIconLookup lookup, string id, Vector2 buttonSize, Vector2? iconSize = null, Vector4? hoveredColor = null, Vector4? activeColor = null, bool grey = false)
    {
        var cursorStart = ImGui.GetCursorScreenPos();

        bool clicked;
        using (ImRaii.PushColor(ImGuiCol.Button, Vector4.Zero))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, hoveredColor ?? new Vector4(1, 1, 1, 0.1f)))
        using (ImRaii.PushColor(ImGuiCol.ButtonActive, activeColor ?? new Vector4(1, 1, 1, 0.2f)))
        {
            clicked = ImGui.Button($"##{id}", buttonSize);
        }

        var size = iconSize ?? buttonSize;
        var iconPos = cursorStart + (buttonSize - size) * 0.5f;
        var afterButton = ImGui.GetCursorScreenPos();

        ImGui.SetCursorScreenPos(iconPos);
        Draw(lookup, size, grey);

        // restore cursor to where the button itself left it, so SameLine()/layout behaves normally
        ImGui.SetCursorScreenPos(afterButton);

        return clicked;
    }

    public static bool TryGetScaledIcon(uint iconId, int size, out IDalamudTextureWrap? texture, bool grey = false) => TryGetScaledIcon(new GameIconLookup(iconId), size, size, out texture, grey);

    public static bool TryGetScaledIcon(GameIconLookup lookup, int width, int height, out IDalamudTextureWrap? texture, bool grey = false)
    {
        texture = Get(lookup, width, height, grey);
        return texture != null;
    }

    public static void Invalidate(uint iconId)
    {
        foreach (var x in Cache)
        {
            if (x.Key.IconId != iconId) continue;
            if (Cache.TryRemove(x.Key, out var texture)) GenericHelpers.Safe(() => texture?.Dispose());
        }
    }

    public static void ClearAll()
    {
        foreach (var x in Cache)
        {
            GenericHelpers.Safe(() => x.Value?.Dispose());
        }
        GenericHelpers.Safe(Cache.Clear);
    }
}
