# GalleryManager

A modular gallery manager for Unity.  
Manages unlockable content — artworks, cutscenes, music tracks, and collectibles — with persistent unlock state via `PlayerPrefs`.  
Optionally integrates with SaveManager, AchievementManager, and EventManager.


## Features

- **Entry registry** — configurable list of `GalleryEntry` items (id, displayName, thumbnail, type, alwaysUnlocked)
- **Unlock persistence** — state stored in `PlayerPrefs` under `gallery_unlock_<id>`
- **Static unlock helper** — `UnlockStatic(id)` usable from gameplay scripts without a scene reference
- **Filter helpers** — `GetUnlockedEntries()`, `GetEntriesByType(type)`
- **OnEntryUnlocked event** — fired when a new entry becomes unlocked (for live UI updates)
- **SaveManager integration** — checks `gallery_<id>` save flags as an additional unlock source (activated via `GALLERYMANAGER_SM`)
- **AchievementManager integration** — mirrors achievement unlocks as gallery unlocks (activated via `GALLERYMANAGER_AM`)
- **EventManager integration** — fires `GalleryEntryUnlocked` as a named GameEvent (activated via `GALLERYMANAGER_EM`)
- **Custom Inspector** — live unlock/lock per entry, Unlock All / Lock All buttons


## Installation

### Option A — Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Enter:

   ```
   https://github.com/RolandKaechele/GalleryManager.git
   ```

### Option B — Clone into Assets

```bash
git clone https://github.com/RolandKaechele/GalleryManager.git Assets/GalleryManager
```

### Option C — npm / postinstall

```bash
cd Assets/GalleryManager
npm install
```

`postinstall.js` confirms installation. No additional data folders are required.


## Quick Start

### 1. Add GalleryManager to your scene

Create a persistent GameObject and attach `GalleryManager`. Define entries in the Inspector.

### 2. Unlock from gameplay

```csharp
// Instance method (when GalleryManager is in the same scene)
var gallery = FindFirstObjectByType<GalleryManager.Runtime.GalleryManager>();
gallery.Unlock("chapter_01_artwork");

// Static helper (usable from any scene — no reference needed)
GalleryManager.Runtime.GalleryManager.UnlockStatic("chapter_01_artwork");
```

### 3. Query state

```csharp
bool unlocked = gallery.IsUnlocked("chapter_01_artwork");

var allUnlocked   = gallery.GetUnlockedEntries();
var artworksOnly  = gallery.GetEntriesByType(GalleryManager.Runtime.GalleryEntryType.Artwork);
```

### 4. React to unlocks

```csharp
gallery.OnEntryUnlocked += id => Debug.Log($"Gallery entry unlocked: {id}");
```

### 5. TitleScreenManager integration (`TITLESCREEN_GM`)

When `TITLESCREEN_GM` is defined, `TitleScreenManager` delegates all gallery queries to `GalleryManager` instead of its own PlayerPrefs fallback.


## Runtime API

### `GalleryManager`

| Member | Description |
| ------ | ----------- |
| `Entries` | Read-only list of all defined `GalleryEntry` items |
| `IsUnlocked(id)` | True if the entry is unlocked (alwaysUnlocked, PlayerPrefs, or SaveManager flag) |
| `Unlock(id)` | Persist unlock, fire `OnEntryUnlocked`. Idempotent |
| `Lock(id)` | Remove PlayerPrefs unlock flag (testing/reset only) |
| `GetEntry(id)` | Return the `GalleryEntry` for an id, or null |
| `GetUnlockedEntries()` | All currently unlocked entries |
| `GetEntriesByType(type)` | Entries filtered by `GalleryEntryType` |
| `UnlockAll()` | Unlock all entries |
| `LockAll()` | Lock all entries (reset) |
| `UnlockStatic(id)` | *(static)* Unlock via PlayerPrefs without a scene reference |
| `OnEntryUnlocked` | `event Action<string>` — fires on new unlock; parameter is entry id |

### `GalleryEntry` fields

| Field | Description |
| ----- | ----------- |
| `id` | Unique string key |
| `displayName` | Label shown in gallery UI |
| `thumbnail` | Preview `Sprite` |
| `type` | `Artwork`, `Cutscene`, `Music`, or `Collectible` |
| `alwaysUnlocked` | Always available (e.g. title music, first artwork) |


## PlayerPrefs Keys

| Key | Value | Description |
| --- | ----- | ----------- |
| `gallery_unlock_<id>` | `0` / `1` | Unlock state per entry |


## Optional Integrations

### SaveManager (`GALLERYMANAGER_SM`)

Add `GALLERYMANAGER_SM` to **Edit → Project Settings → Player → Scripting Define Symbols**.  
`IsUnlocked()` additionally checks `SaveManager.IsSet("gallery_<id>")` on the active slot.  
Requires [SaveManager](https://github.com/RolandKaechele/SaveManager).

### AchievementManager (`GALLERYMANAGER_AM`)

Add `GALLERYMANAGER_AM` to Scripting Define Symbols.  
Achievement unlock events are mirrored as gallery unlocks when the achievement id matches a gallery entry id.  
Requires AchievementManager.

### EventManager (`GALLERYMANAGER_EM`)

Add `GALLERYMANAGER_EM` to Scripting Define Symbols.  
Fires the named GameEvent `"GalleryEntryUnlocked"` (value = entry id) on every new unlock.


## Dependencies

| Dependency | Required | Notes |
| ---------- | -------- | ----- |
| Unity 2022.3+ | ✓ | |
| SaveManager | optional | Required when `GALLERYMANAGER_SM` is defined |
| AchievementManager | optional | Required when `GALLERYMANAGER_AM` is defined |
| EventManager | optional | Required when `GALLERYMANAGER_EM` is defined |


## Repository

[https://github.com/RolandKaechele/GalleryManager](https://github.com/RolandKaechele/GalleryManager)


## License

MIT — see [LICENSE](LICENSE).
