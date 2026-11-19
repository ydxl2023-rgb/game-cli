# Login standard source and production materials

The C# files are exact development sources, not the reduced practice trial implementation.

Read dependencies.json before integration. This module owns Login and the ServerSelect subpage; it does not introduce a ServerSelectSystem. The standard editor has one design root, so the ServerSelect prefab, inline Fancy cells and their serialized Metadata are delivered as companion materials, not appended as visible children of the Login screen. The existing published main-screen design and animation are preserved.

materials.json uses schema gamecli-source-materials/1. Each entry has a unity, configuration or reference scope, relative path, base64 content and SHA256 of the original decoded bytes. It includes both original prefab trees, images, animation/controller files, importer metadata and the source workbook. Verify every decoded SHA256 before use; preserve original GUIDs. Never overwrite a target product's edited resources or replace its entire CommonText table. Merge the required aliases, preserve translations, then run the product's normal table exporter. Do not copy generated config classes from development.

For product adaptation use UnityCLI for prefab generation, component/Metadata binding and animation editing. These native files are the exact source reference/archive, not permission to hand-edit YAML. Inspect both root Metadata and the two inline cell bindings; VM field order is part of the delivery. Register SyncFancyGridScrollView and SyncFancyGridClick before opening the selector.

Reuse the target's shared RuntimeSystem, Master/events and Notice contracts, or explicitly adapt them to the product requirements. LoginEntity still declares Refresh(LoginResponse); the original Protocols.cs is archived as reference, not automatically installed. Preserve or adapt this dependency explicitly, and do not restore network calls merely to satisfy its type name. Do not automatically install another full system to resolve a type name. The local catalog is test data, not a real service. This synchronization proves source/material fidelity; it is not a fresh runtime acceptance of a new product.
