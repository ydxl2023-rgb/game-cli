using System.Text.Json.Nodes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UnityCLI;

static JsonObject Slot(string purpose, string mode)
{
    return new JsonObject { ["slotId"] = "test", ["purpose"] = purpose, ["imageType"] = "simple", ["imageProcessing"] = new JsonObject { ["resizeMode"] = mode } };
}
static void Check(bool value, string message)
{
    if (!value) { throw new Exception(message); }
}
static void Reject(Action action, string message)
{
    try { action(); }
    catch (InvalidOperationException) { return; }
    throw new Exception(message);
}
using Image<Rgba32> source = new Image<Rgba32>(20, 10, new Rgba32(255, 0, 0, 255));
using (Image<Rgba32> result = UiImagePreparation.Render(source, Slot("icon", "contain"), new Rectangle(0, 0, 10, 5), 10, 5))
{
    Check(result.Width == 10 && result.Height == 5 && result[5, 2].R == 255, "Proportional resize failed");
}
Reject(() => UiImagePreparation.Render(source, Slot("icon", "contain"), new Rectangle(0, 0, 10, 10), 10, 10).Dispose(), "Stretched icon accepted");
Reject(() => UiImagePreparation.Render(source, Slot("icon", "contain"), new Rectangle(0, 0, 100, 50), 100, 50).Dispose(), "Excessive upscale accepted");
Reject(() => UiImagePreparation.Render(source, Slot("icon", "cover"), new Rectangle(0, 0, 10, 10), 10, 10).Dispose(), "Icon cropping accepted");
Reject(() => UiImagePreparation.Render(source, Slot("background", "cover"), new Rectangle(0, 0, 10, 10), 10, 10).Dispose(), "Excessive background crop accepted");
using (Image<Rgba32> result = UiImagePreparation.Render(source, Slot("background", "cover"), new Rectangle(0, 0, 18, 10), 18, 10))
{
    Check(result[0, 0].A == 255 && result.Width == 18, "Background crop failed");
}
using Image<Rgba32> panel = new Image<Rgba32>(10, 10, new Rgba32(0, 0, 255, 255));
for (int y = 0; y < 2; y++)
{
    for (int x = 0; x < 2; x++) { panel[x, y] = new Rgba32(255, 0, 0, 255); }
}
JsonObject sliced = Slot("surface", "nine-slice");
sliced["imageType"] = "sliced";
sliced["border"] = new JsonObject { ["left"] = 2, ["top"] = 2, ["right"] = 2, ["bottom"] = 2 };
sliced["imageProcessing"]!["sourceBorder"] = sliced["border"]!.DeepClone();
using (Image<Rgba32> result = UiImagePreparation.Render(panel, sliced, new Rectangle(0, 0, 20, 12), 20, 12))
{
    Check(result[1, 1].R == 255 && result[2, 1].B == 255, "Nine-slice corner size changed");
}
sliced["border"]!["top"] = 4;
Reject(() => UiImagePreparation.Render(panel, sliced, new Rectangle(0, 0, 20, 12), 20, 12).Dispose(), "Distorted nine-slice corners accepted");
using Image<Rgba32> edge = new Image<Rgba32>(10, 10);
for (int y = 0; y < 10; y++)
{
    for (int x = 0; x < 5; x++) { edge[x, y] = new Rgba32(255, 0, 0, 255); }
}
using (Image<Rgba32> result = UiImagePreparation.Render(edge, Slot("effect", "contain"), new Rectangle(0, 0, 20, 20), 20, 20))
{
    bool found = false;
    for (int x = 7; x < 13; x++)
    {
        Rgba32 pixel = result[x, 10];
        if (pixel.A > 0 && pixel.A < 255) { found = true; Check(pixel.R > 240, "Transparent edge developed black fringe"); }
    }
    Check(found, "High-quality sampling did not create smooth edge");
}
Console.WriteLine("Passed 9 checks: proportional scaling, upscale/aspect/crop rejection, background crop, nine-slice corners, border rejection and alpha edges.");
