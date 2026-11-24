# UGUI Component Composition

Use this reference when a Web visual-ui to UGUI prefab task involves buttons, player info, resource bars, navigation entries, or any repeated UI control that might otherwise be exported as one bitmap.

## Core Rule

Never use a whole screenshot-like sprite as an interactive or bound control when the UI has separable semantics. A prefab should expose the same pieces a Unity UI engineer expects to bind, localize, skin, reuse, or click.

## Standard Button Hierarchy

For every normal button, generate this hierarchy:

```text
BtnX                 Button + Image, root hit area and background sprite
  BtnXIcon           Image, icon only
  BtnXText           Text, label only
```

Rules:

- The `Button` root owns click interaction and must use the visual background sprite as its own `Image` / `data-ui-sprite`.
- Do not create a separate `BtnXBg` child for the primary button background. A child background prevents the Button target graphic from tinting the actual visible button during pressed, highlighted, selected, or disabled states.
- The icon sprite must not contain label text, notification badges, background frame residue, or adjacent UI pixels.
- The text must be a real UGUI text node. With `USE_OLD_TEXT`, it must become `UnityEngine.UI.Text`.
- Center-label buttons can set the text rect to the full button size.
- Side-label buttons should place icon and text as sibling children, for example icon left and text right.
- Reuse common backgrounds through one manifest asset with multiple `usedBy` ids that point to the button root ids, not child `Bg` ids.

## Player Info Composite

Player profile areas must not be one baked `PlayerInfo.png`. Split them into bindable pieces:

```text
PlayerInfo                 Container, reusable player info root
  PlayerInfoBg             Image, profile background only
  PlayerAvatar             Image, avatar portrait
  PlayerNameText           Text, player name binding
  PlayerLevelBg            Image, optional level chip background
  PlayerLevelText          Text, level binding
  PlayerVipBarBg           Image, VIP/progress background
  PlayerVipFill            Image, VIP/progress fill
  PlayerVipText            Text, VIP binding
```

Rules:

- Name, level, and VIP must be independent text nodes so Metadata can bind them.
- Avatar must be a separate image node so it can be replaced by player data.
- Decorative backgrounds can be reusable images, but they must not bake text, avatar, or numeric data.
- If a field is not present in the target UI, omit the node rather than baking it into another sprite.

## Currency And Resource Bars

Repeated currency/resource bars should be represented as semantic item roots, not flattened sprites. Every currency/resource bar must have a numeric background frame, a currency icon, a real amount text node, and an add button. Do not flatten these pieces into one sprite.

```text
ResourceList                 Container or ListView
  ResGem                     Container, reusable currency bar root
    ResGemBg                 Image, shared currency numeric-frame background only
    ResGemIcon               Image, currency icon only
    ResGemText               Text, amount binding
    ResGemAddButton          Button + Image, add currency action and button background
      ResGemAddButtonIcon    Image, plus icon only
```

Mandatory rules:

- `ResGemBg` / `ResTicketBg` / `ResCoinBg` are the numeric-frame backgrounds only. They must not contain the currency icon, amount digits, plus icon, add button frame, or adjacent bar pixels.
- `ResGemIcon` / `ResTicketIcon` / `ResCoinIcon` are icon-only sprites on centered transparent canvases. They must not contain the dark numeric frame, text, glow from another control, or cut-off edges.
- `ResGemText` / `ResTicketText` / `ResCoinText` are real UGUI text nodes. Amounts are never baked into the background or icon.
- `ResGemAddButton` / `ResTicketAddButton` / `ResCoinAddButton` must be real `Button + Image` roots. If the add visual has a frame/circle, that sprite belongs on the add-button root; the plus symbol may be a child `Image` only when it is visually separate.
- If a currency bar has no visible add action in the reference, still model it as an optional add-button slot that can be hidden, not as a different flattened bar type.
- Currency bar item roots may be placed under a fixed `Container` or under a `ListView`, but each item must keep the same child structure.

Manifest pattern:

```json
{
  "id": "CurrencyBarBg",
  "usedBy": ["ResGemBg", "ResTicketBg", "ResCoinBg"]
}
```

Cutting rules:

- Crop the numeric-frame background separately from the currency icon and add button.
- Use minimum visual bounding rectangles and then center the asset on a transparent canvas. Large transparent margins are not allowed.
- For resource icons cropped from whole bars, work from the icon region, ignore the dark numeric-frame pixels, keep the largest relevant connected component, apply small padding, and trim alpha after cleaning.
- For add buttons, preserve the full button visual, such as a complete plus circle. Do not cut off the ring, and do not include the slot/card background behind it.

Use `ListView` only when the group is data-driven or should receive a layout component. For a fixed top resource strip, a `Container` group is acceptable, but each resource bar still needs its own composite item root.

## Converting Whole Assets

When replacing old whole button, player info, or resource images:

1. Copy old whole sprites into a local source folder before deleting them from production output.
2. Generate clean independent sprites: background, icon, avatar, optional badge, optional add button, optional progress fill.
3. Remove the old whole sprite from `asset-manifest.json` and from production `assets_png`.
4. Add new manifest entries for reusable pieces and map each to the generated child nodes.
5. Regenerate HTML, extract `visual-ui.json`, run `ugui --dry-run`, then formal `ugui`.

Do not leave both the old whole sprite and the new child sprites in production resources unless the old sprite is still used by another real node.

## Dirty Slice QA

Runtime screenshots are not enough to catch dirty cutouts. Inspect small generated assets directly before prefab generation.

Check for:

- label text included inside an icon sprite
- old dark bar/background pixels included inside an icon
- cut-off icon edges caused by too-tight bbox
- plus/add graphics still baked into a resource bar background
- avatar, level, VIP, or name baked into a player info background
- background frame residue inside a pure icon

If a slice is dirty, fix the source crop or extraction logic and rerun the full chain. Do not patch the prefab or scale the dirty sprite to hide the issue.

For resource icons cropped from whole bars, prefer component-aware extraction:

- work from the left icon region, not the full bar
- ignore dark low-brightness bar pixels
- keep the largest bright connected component
- apply a small padding
- trim alpha after cleaning

## Metadata And Binding QA

Every UI root that participates in widget binding must have a Metadata component and a VM script reference.

For composite nodes:

- Button click fields must bind to the `Button` root, such as `ResGemAddButton`.
- Text data fields must bind to the child text node, such as `PlayerNameText`, `PlayerLevelText`, `PlayerVipText`, `ResGemText`, `ResTicketText`, and `ResCoinText`.
- Binding targets and component references must be non-empty after prefab generation.
- With `USE_OLD_TEXT`, prefab and generated resources must not contain `TextMeshProUGUI`, `TMPro`, `SyncTextMeshPro`, or TMP script GUID references.

## Verification Checklist

Before final delivery:

```powershell
Library\UnityCLI\UnityCLI.exe compile --wait
Library\UnityCLI\UnityCLI.exe console --type error --count 80
Library\UnityCLI\UnityCLI.exe ugui UIName --json ... --manifest ... --prefab ... --dry-run
Library\UnityCLI\UnityCLI.exe ugui UIName --json ... --manifest ... --prefab ...
Library\UnityCLI\UnityCLI.exe search ResGemAddButton --count 5
Library\UnityCLI\UnityCLI.exe search PlayerInfo --count 5
Library\UnityCLI\UnityCLI.exe search UIName --count 1
Library\UnityCLI\UnityCLI.exe screenshot --dst UnityCLI-Screenshots\UIName_check.png --super-size 2
```

Also verify:

- UI root remains full-screen Stretch.
- List containers use real layout components and item roots.
- Composite item roots exist and contain expected children.
- Add buttons have actual `Button` components.
- Old whole assets no longer appear in the production resource folder or manifest.
- Metadata entries for buttons and text fields have non-empty targets and components.
- `rg 'TextMeshProUGUI|TMPro|SyncTextMeshPro|f4688'` returns no matches when `USE_OLD_TEXT` is enabled.
- Console remains empty after prefab generation and PlayMode screenshot.
