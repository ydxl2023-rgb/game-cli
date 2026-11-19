# Settings v2 scoped independent review

No blocking defect found in this scoped removal. This review does not replace the v1 strict-prefab limitations.

Independently inspected current source and v2-function.png: Tencent customer-service entry and the four named notification controls are absent; ExtremeTrain and HomeDaily remain visible. Their VM/System option bindings remain, while the five removed entry identifiers are absent from active Settings source.

Persistence review: retained SettingsOption members explicitly keep original numeric values; deleted values 10–13 remain holes. Restoration first reads Dictionary<string,float>, then Enum.TryParse and IsValid skip removed/unknown keys rather than causing enum-dictionary deserialization to discard all known preferences. The old numeric identifiers therefore cannot shift to other options. Reviewed developer v2-persistence.json (True), covering named/numeric legacy keys and retained MusicVolume, Texture and ExtremeTrain values. This is developer-run evidence; the reviewer did not operate Unity.

Developer evidence also records 576 nodes, retained controls, 162 Metadata fields/334 references without missing targets and a passing code check. Publication and project-instance synchronization were still pending at this review; require final readback before claiming database completion. Historical source archives intentionally retain the original removed controls and should not be mistaken for the active implementation.
