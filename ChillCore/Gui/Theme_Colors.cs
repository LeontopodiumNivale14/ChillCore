namespace ChillCore.Gui;

internal static class Theme_Colors
{
    // ---- Base palette ----
    public static readonly Vector4 IceBlue = new Vector4(0.7f, 0.85f, 1.0f, 1.0f);
    public static readonly Vector4 DarkIceBlue = new Vector4(0.3f, 0.5f, 0.7f, 1.0f);
    public static readonly Vector4 DeepIceBlue = new Vector4(0.15f, 0.25f, 0.4f, 1.0f);
    public static readonly Vector4 FrostWhite = new Vector4(0.95f, 0.97f, 1.0f, 1.0f);
    public static readonly Vector4 TranslucentIce = new Vector4(0.8f, 0.9f, 1.0f, 0.15f);
    public static readonly Vector4 DarkSlate = new Vector4(0.12f, 0.14f, 0.18f, 0.9f);
    public static readonly Vector4 LightSlate = new Vector4(0.22f, 0.26f, 0.33f, 0.9f);

    // Helpers to make the colors fit in with the rest of the base colors from above ^

    private static Vector4 WithAlpha(Vector4 c, float alpha)
        => new Vector4(c.X, c.Y, c.Z, alpha);

    // Relative lighten (percentage of remaining headroom to white) —
    // scales sanely whether the base is dark or already fairly light
    private static Vector4 LightenRel(Vector4 c, float t)
        => new Vector4(
            c.X + (1.0f - c.X) * t,
            c.Y + (1.0f - c.Y) * t,
            c.Z + (1.0f - c.Z) * t,
            c.W);

    // Standard interaction ramp: same t-values everywhere, so any
    // base color hovers/actives with the same *relative* punch
    private const float HoverT = 0.15f;
    private const float ActiveT = 0.28f;

    private const float HeaderHoverT = 0.08f;
    private const float HeaderActiveT = 0.15f;

    // Sidebar Accent (specifically for the table cell)
    public static readonly Vector4 SidebarAccent = IceBlue;

    // On/active indicator accent — reuses the same swap pattern as SidebarAccent
    public static readonly Vector4 OnAccent = FrostWhite;
    public static readonly Vector4 OnAccentSubtle = LightenRel(DarkIceBlue, 0.45f); // brighter ice-blue, not a clashing hue

    // Buttons
    public static readonly Vector4 ButtonBg = DarkIceBlue;
    public static readonly Vector4 ButtonHovered = LightenRel(DarkIceBlue, HoverT);
    public static readonly Vector4 ButtonActive = LightenRel(DarkIceBlue, ActiveT);

    // Header
    public static readonly Vector4 HeaderBg = WithAlpha(DarkIceBlue, 0.55f);
    public static readonly Vector4 HeaderHovered = WithAlpha(LightenRel(DarkIceBlue, HeaderHoverT), 0.6f);
    public static readonly Vector4 HeaderActive = WithAlpha(LightenRel(DarkIceBlue, HeaderActiveT), 0.65f);

    // Frame
    public static readonly Vector4 FrameBg = WithAlpha(LightSlate, 1.0f);
    public static readonly Vector4 FrameBgHovered = LightenRel(FrameBg, HoverT * 0.6f);
    public static readonly Vector4 FrameBgActive = LightenRel(FrameBg, ActiveT * 0.6f);

    // Child
    public static readonly Vector4 ChildBg = WithAlpha(DarkSlate, 0.7f);
    public static readonly Vector4 PopupBg = WithAlpha(DarkSlate, 1.0f);
    public static readonly Vector4 PopupBorder = WithAlpha(DarkIceBlue, 0.9f);
}
