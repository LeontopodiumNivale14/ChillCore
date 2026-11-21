using Dalamud.Game;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.DalamudServices;
using ECommons.Logging;
using Lumina.Data.Files;
using System.Collections.Concurrent;
using System.IO;

namespace ChillCore.Gui;

public static partial class GameIcons
{
    private static readonly ConcurrentDictionary<CacheKey, IDalamudTextureWrap?> Cache = [];

    private const int LanczosLobes = 3;

    private static IDalamudTextureWrap? Get(GameIconLookup lookup, int width, int height, bool grey)
    {
        if (width <= 0 || height <= 0 || width > MaximumSize || height > MaximumSize) return null;
        var key = new CacheKey(lookup.IconId, lookup.ItemHq, lookup.HiRes, lookup.Language, width, height, grey);
        if (Cache.TryGetValue(key, out var cached)) return cached;

        IDalamudTextureWrap? texture = null;
        try
        {
            if (TryGetTexFile(lookup, out var file) && file.Header.Width > 0 && file.Header.Height > 0)
                texture = Resample(file, lookup.IconId, width, height, grey);
            else
                PluginLog.Warning($"[GameIcons] Could not find icon {lookup.IconId}");
        }
        catch (Exception e)
        {
            PluginLog.Warning($"[GameIcons] Could not resample icon {lookup.IconId} to {width}x{height}:\n{e}");
        }

        Cache[key] = texture;
        return texture;
    }

    private static IDalamudTextureWrap Resample(TexFile file, uint iconId, int width, int height, bool grey)
    {
        var bitmap = ResizeBgra(file.ImageData, file.Header.Width, file.Header.Height, width, height, grey);

        var suffix = grey ? " (grey)" : "";
        return Svc.Texture.CreateFromRaw(RawImageSpecification.Bgra32(width, height), bitmap, $"ECommons.GameIcons {iconId}@{width}x{height}{suffix}");
    }

    /// <summary>
    /// Lanczos3 resize on premultiplied alpha, with optional grayscale (BT.709) / brightness / opacity.
    /// Input and output are BGRA, 4 bytes per pixel.
    /// </summary>
    private static byte[] ResizeBgra(byte[] src, int srcW, int srcH, int dstW, int dstH, bool grey)
    {
        if (src.Length < srcW * srcH * 4)
            throw new InvalidOperationException($"Image data too small: {src.Length} bytes for {srcW}x{srcH}");

        // Premultiply so transparent pixels don't bleed dark/colored fringes into edges.
        var premul = new float[srcW * srcH * 4];
        for (var i = 0; i < srcW * srcH; i++)
        {
            var o = i * 4;
            var a = src[o + 3] / 255f;
            premul[o] = src[o] / 255f * a;
            premul[o + 1] = src[o + 1] / 255f * a;
            premul[o + 2] = src[o + 2] / 255f * a;
            premul[o + 3] = a;
        }

        var xWeights = BuildWeights(srcW, dstW);
        var yWeights = BuildWeights(srcH, dstH);

        // Horizontal pass: srcW x srcH -> dstW x srcH
        var horizontal = new float[dstW * srcH * 4];
        for (var y = 0; y < srcH; y++)
        {
            for (var x = 0; x < dstW; x++)
            {
                var (start, weights) = xWeights[x];
                float b = 0, g = 0, r = 0, a = 0;
                for (var k = 0; k < weights.Length; k++)
                {
                    var s = (y * srcW + start + k) * 4;
                    var w = weights[k];
                    b += premul[s] * w;
                    g += premul[s + 1] * w;
                    r += premul[s + 2] * w;
                    a += premul[s + 3] * w;
                }

                var d = (y * dstW + x) * 4;
                horizontal[d] = b;
                horizontal[d + 1] = g;
                horizontal[d + 2] = r;
                horizontal[d + 3] = a;
            }
        }

        // Vertical pass: dstW x srcH -> dstW x dstH, then write final pixels
        var output = new byte[dstW * dstH * 4];
        for (var y = 0; y < dstH; y++)
        {
            var (start, weights) = yWeights[y];
            for (var x = 0; x < dstW; x++)
            {
                float b = 0, g = 0, r = 0, a = 0;
                for (var k = 0; k < weights.Length; k++)
                {
                    var s = ((start + k) * dstW + x) * 4;
                    var w = weights[k];
                    b += horizontal[s] * w;
                    g += horizontal[s + 1] * w;
                    r += horizontal[s + 2] * w;
                    a += horizontal[s + 3] * w;
                }

                WritePixel(output, (y * dstW + x) * 4, b, g, r, a, grey);
            }
        }

        return output;
    }

    private static void WritePixel(byte[] dst, int o, float b, float g, float r, float a, bool grey)
    {
        // Lanczos rings slightly negative/over 1, so clamp before un-premultiplying.
        a = Math.Clamp(a, 0f, 1f);
        if (a <= 0f) // a is zero or below: fully transparent, leave the pixel as 0,0,0,0
            return;

        b = Math.Clamp(b / a, 0f, 1f);
        g = Math.Clamp(g / a, 0f, 1f);
        r = Math.Clamp(r / a, 0f, 1f);

        if (grey)
        {
            var luma = Math.Clamp((0.0722f * b + 0.7152f * g + 0.2126f * r) * GreyBrightness, 0f, 1f);
            b = g = r = luma;
            a = Math.Clamp(a * GreyOpacity, 0f, 1f);
        }

        dst[o] = ToByte(b);
        dst[o + 1] = ToByte(g);
        dst[o + 2] = ToByte(r);
        dst[o + 3] = ToByte(a);
    }

    private static byte ToByte(float v) => (byte)(v * 255f + 0.5f);

    /// <summary>For each destination index: first source index + normalized Lanczos3 weights.</summary>
    private static (int Start, float[] Weights)[] BuildWeights(int srcSize, int dstSize)
    {
        var scale = (double)srcSize / dstSize;
        // When downscaling (scale > 1, i.e. source is bigger than destination) widen the kernel to avoid aliasing.
        var filterScale = Math.Max(scale, 1.0);
        var support = LanczosLobes * filterScale;

        var result = new (int, float[])[dstSize];
        for (var i = 0; i < dstSize; i++)
        {
            var center = (i + 0.5) * scale;
            var start = Math.Max((int)Math.Floor(center - support), 0);
            var end = Math.Min((int)Math.Ceiling(center + support), srcSize);
            if (end <= start)
            {
                start = Math.Clamp((int)center, 0, srcSize - 1);
                end = start + 1;
            }

            var weights = new float[end - start];
            var sum = 0f;
            for (var j = 0; j < weights.Length; j++)
            {
                var w = (float)Lanczos((start + j + 0.5 - center) / filterScale);
                weights[j] = w;
                sum += w;
            }

            if (sum != 0f)
            {
                for (var j = 0; j < weights.Length; j++)
                    weights[j] /= sum;
            }
            else
            {
                weights[weights.Length / 2] = 1f;
            }

            result[i] = (start, weights);
        }

        return result;
    }

    private static double Lanczos(double x)
    {
        if (x == 0) return 1;
        if (Math.Abs(x) >= LanczosLobes) return 0; // |x| at or beyond 3: outside the kernel
        var px = Math.PI * x;
        return LanczosLobes * Math.Sin(px) * Math.Sin(px / LanczosLobes) / (px * px);
    }

    private static bool TryGetTexFile(GameIconLookup lookup, out TexFile? file)
    {
        file = null;
        if (!Svc.Texture.TryGetIconPath(lookup, out var path)) return false;

        var substituted = Svc.TextureSubstitution.GetSubstitutedPath(path);
        if (substituted != path && Path.IsPathRooted(substituted))
        {
            if (substituted.EndsWith(".tex", StringComparison.OrdinalIgnoreCase) && File.Exists(substituted))
                file = Svc.Data.GameData.GetFileFromDisk<TexFile>(substituted, path);
        }
        else
        {
            file = Svc.Data.GetFile<TexFile>(substituted);
        }

        file ??= Svc.Data.GetFile<TexFile>(path);
        return file != null;
    }

    private readonly record struct CacheKey(uint IconId, bool ItemHq, bool HiRes, ClientLanguage? Language, int Width, int Height, bool Grey);
}
