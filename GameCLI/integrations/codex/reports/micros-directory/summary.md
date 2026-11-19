# Micros directory migration

Reusable modules moved through UnityCLI asset move to Assets/Data/Widgets/Micros/<module>. Page prefabs and C# ownership are unchanged. The legacy Micros/ItemSlot.prefab remains distinct from Micros/ItemSlot/ItemSlot.prefab.

- 187 moved files retain identical bytes; 100 GUIDs retained.
- 27 prefabs checked, 26,107 serialized reference properties inspected, no missing references/scripts.
- Home currency/profile/shortcuts load and bind; Mail attachment resolves the new ItemSlot path; Shop loads six cards with nested item images.
- 270 moved template material entries match local hashes.
- 14 template versions published; Home/Mail/Shop instances synchronized in GameCLIApp revision 66.
- Template readback matches candidate scripts/configurations. No old paths in active delivery files. Historical migration maps intentionally preserve their source paths; each template includes a new micros-directory-migration.json.
- This is a path migration, not a new full behavior/animation acceptance. Prior known framework asynchronous image limitations are unchanged.
