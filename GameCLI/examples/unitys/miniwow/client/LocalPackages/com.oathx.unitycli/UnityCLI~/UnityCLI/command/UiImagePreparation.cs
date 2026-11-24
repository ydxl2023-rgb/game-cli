using System.Text.Json.Nodes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace UnityCLI
{
    /// <summary>Type-aware resampling. Invalid geometry requires new artwork, never forced distortion.</summary>
    internal static class UiImagePreparation
    {
        internal static Image<Rgba32> Render(Image<Rgba32> source, JsonObject slot, Rectangle target, int width, int height)
        {
            JsonObject settings = slot["imageProcessing"] as JsonObject ?? new JsonObject();
            string purpose = slot["purpose"]!.GetValue<string>();
            string mode = settings["resizeMode"]?.GetValue<string>()
                ?? (purpose == "background" ? "cover" : purpose == "surface" && slot["imageType"]?.GetValue<string>() == "sliced" ? "nine-slice" : "contain");
            string sampling = settings["sampling"]?.GetValue<string>() ?? "bicubic";
            double maximum = Limit(settings, "maxUpscale", 2, 1, 8);
            double distortion = Limit(settings, "maxAspectDifference", 0.05, 0, 0.25);
            double cropLimit = Limit(settings, "maxCropFraction", 0.15, 0, 0.5);
            double sx = (double)target.Width / source.Width;
            double sy = (double)target.Height / source.Height;
            if (Math.Max(sx, sy) > maximum)
            {
                throw new InvalidOperationException("Source resolution is too low; regenerate higher-resolution artwork: " + slot["slotId"]);
            }
            Image<Rgba32> output = new Image<Rgba32>(width, height);
            try
            {
                if (mode == "nine-slice")
                {
                    if (purpose != "surface" || slot["imageType"]?.GetValue<string>() != "sliced")
                    {
                        throw new InvalidOperationException("nine-slice requires a sliced resource slot.");
                    }
                    Slice(source, output, slot, settings, target, sampling, distortion, maximum);
                }
                else if (mode == "contain" || mode == "cover")
                {
                    if (purpose != "background" && mode == "cover")
                    {
                        throw new InvalidOperationException("Only backgrounds may crop artwork.");
                    }
                    if (mode == "contain" && Math.Max(sx, sy) / Math.Min(sx, sy) - 1 > distortion)
                    {
                        throw new InvalidOperationException("Aspect ratio differs from locked silhouette; regenerate instead of stretching: " + slot["slotId"]);
                    }
                    double scale = mode == "contain" ? Math.Min(sx, sy) : Math.Max(sx, sy);
                    int rw = Math.Max(target.Width, (int)Math.Round(source.Width * scale));
                    int rh = Math.Max(target.Height, (int)Math.Round(source.Height * scale));
                    if (mode == "contain")
                    {
                        rw = Math.Max(1, (int)Math.Round(source.Width * scale));
                        rh = Math.Max(1, (int)Math.Round(source.Height * scale));
                    }
                    double removed = 1 - (double)(target.Width * target.Height) / (rw * rh);
                    if (mode == "cover" && removed > cropLimit)
                    {
                        throw new InvalidOperationException("Background crop exceeds allowed fraction; regenerate composition.");
                    }
                    using Image<Rgba32> resized = Resize(source, rw, rh, sampling);
                    if (mode == "cover")
                    {
                        resized.Mutate(c => c.Crop(new Rectangle((rw - target.Width) / 2, (rh - target.Height) / 2, target.Width, target.Height)));
                    }
                    output.Mutate(c => c.DrawImage(resized, new Point(target.X + (target.Width - resized.Width) / 2, target.Y + (target.Height - resized.Height) / 2), 1f));
                }
                else
                {
                    throw new InvalidOperationException("Unknown resize mode: " + mode);
                }
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        private static Image<Rgba32> Resize(Image<Rgba32> source, int width, int height, string sampling)
        {
            if (sampling != "bicubic" && sampling != "lanczos3" && sampling != "pixel-art")
            {
                throw new InvalidOperationException("Unknown sampling mode: " + sampling);
            }
            return source.Clone(c => c.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Stretch,
                Sampler = sampling == "pixel-art" ? KnownResamplers.NearestNeighbor : sampling == "lanczos3" ? KnownResamplers.Lanczos3 : KnownResamplers.Bicubic,
                Compand = true,
                PremultiplyAlpha = true
            }));
        }

        private static void Slice(Image<Rgba32> source, Image<Rgba32> output, JsonObject slot, JsonObject settings, Rectangle target, string sampling, double tolerance, double maximum)
        {
            JsonObject sourceBorder = settings["sourceBorder"] as JsonObject ?? throw new InvalidOperationException("nine-slice requires measured sourceBorder in cropped-source pixels.");
            JsonObject border = slot["border"]!.AsObject();
            int[] a = new[] { sourceBorder["left"]!.GetValue<int>(), sourceBorder["top"]!.GetValue<int>(), sourceBorder["right"]!.GetValue<int>(), sourceBorder["bottom"]!.GetValue<int>() };
            int[] b = new[] { border["left"]!.GetValue<int>(), border["top"]!.GetValue<int>(), border["right"]!.GetValue<int>(), border["bottom"]!.GetValue<int>() };
            if (a.Any(v => v <= 0) || b.Any(v => v <= 0) || a[0] + a[2] >= source.Width || a[1] + a[3] >= source.Height || b[0] + b[2] >= target.Width || b[1] + b[3] >= target.Height)
            {
                throw new InvalidOperationException("Invalid or overlapping nine-slice borders.");
            }
            double[] ratios = Enumerable.Range(0, 4).Select(i => (double)b[i] / a[i]).ToArray();
            if (ratios.Max() > maximum || ratios.Max() / ratios.Min() - 1 > tolerance)
            {
                throw new InvalidOperationException("Nine-slice corner scales differ or exceed upscale limit; correct border specification.");
            }
            int[] xs = { 0, a[0], source.Width - a[2], source.Width };
            int[] ys = { 0, a[1], source.Height - a[3], source.Height };
            int[] xd = { target.X, target.X + b[0], target.Right - b[2], target.Right };
            int[] yd = { target.Y, target.Y + b[1], target.Bottom - b[3], target.Bottom };
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    using Image<Rgba32> patch = source.Clone(c => c.Crop(new Rectangle(xs[x], ys[y], xs[x + 1] - xs[x], ys[y + 1] - ys[y])));
                    using Image<Rgba32> resized = Resize(patch, xd[x + 1] - xd[x], yd[y + 1] - yd[y], sampling);
                    output.Mutate(c => c.DrawImage(resized, new Point(xd[x], yd[y]), 1f));
                }
            }
        }

        private static double Limit(JsonObject settings, string name, double fallback, double minimum, double maximum)
        {
            double value = settings[name]?.GetValue<double>() ?? fallback;
            if (!double.IsFinite(value) || value < minimum || value > maximum)
            {
                throw new InvalidOperationException("Invalid image processing limit: " + name);
            }
            return value;
        }
    }
}
