# Street basketball UI V2

Applied 6 built-in imagegen masters to 59 distinct textures / 144 asset files. Orange/yellow primary surfaces, cyan secondary surfaces, dark screenprinted panels, basketball gold coins, cyan gems and orange/black gift crates. Generated prompts and originals are in generated/, derived texture candidates in textures/, pre-change backups in before/. result.json gives the exact applied mapping. build.py builds the material variants; finalize.py restores four functional-symbol sprites after review.

Ten large background and scenery layers were restored to template bytes and verified. All 1364 non-texture baseline files remain unchanged, including prefabs, animation clips, controllers and importer meta files. All image dimensions are preserved. Surface derivatives use original alpha; generated item icons are fitted to original visible bounds and preserve generated transparency.

This is a UI skin revision using reusable generated materials, not 144 individually generated drawings. Character art, baked labels, logos and other unmapped assets remain as before. Unity AssetDatabase refresh succeeded; Console reported zero errors. The captured current Game view is in client/UnityCLI-Screenshots/streetball-v2.png. Full per-screen visual acceptance has not been completed.
