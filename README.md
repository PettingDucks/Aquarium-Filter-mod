# Aquarium Filter

A tModLoader mod for Terraria. Adds a filter machine that slowly cleans Corruption, Crimson and Hallow anywhere in your world, like an automatic Clentaminator that runs on Green Solution.

> [!IMPORTANT]
> Versions before 0.4 can't load the filter in multiplayer. [Download the latest version](https://github.com/PettingDucks/Aquarium-Filter-mod/releases/latest/download/AquariumFilter.tmod) to fix it.

## Features

- Place the filter anywhere and load it with **Green Solution** by right-clicking it.
- Every so often it sprays Green Solution on a random infected block somewhere in the world. The game's own purification does the work, so nothing else is changed.
- One Green Solution cleans 40 blocks by default.
- Mining the filter gives back any Green Solution still inside it.
- Works in single player and multiplayer.

Three versions:

| Item | Cleans |
|---|---|
| Aquarium Filter | Corruption, Crimson and Hallow |
| Aquarium Filter (Evil) | Corruption and Crimson only (keeps your Hallow) |
| Aquarium Filter (Hallow) | Hallow only |

## Recipe

10 Green Solution, 10 Hallowed Bars, 10 Souls of Light and 10 Souls of Night at a Mythril or Orichalcum Anvil. Green Solution is sold by the Steampunker.

## How to install

1. **[Download AquariumFilter.tmod](https://github.com/PettingDucks/Aquarium-Filter-mod/releases/latest/download/AquariumFilter.tmod)** (latest version).
2. Put it in your tModLoader `Mods` folder:
   - Windows: `Documents\My Games\Terraria\tModLoader\Mods`
   - Linux: `~/.local/share/Terraria/tModLoader/Mods`
   - macOS: `~/Library/Application Support/Terraria/tModLoader/Mods`
3. Start tModLoader, open **Workshop > Manage Mods**, enable **Aquarium Filter** and reload.

## Config

**Aquarium Filter** (server side):

| Option | Default | What it does |
|---|---|---|
| Ticks between sprays | 20 (3 per second) | How often each filter sprays |
| Blocks per Green Solution | 40 | How many blocks one Green Solution cleans |

## Credits

Made by PettingDucks.
