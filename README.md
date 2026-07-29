# Room Transition CTD Fix - Synthesis Patcher

A [Synthesis](https://github.com/Noggog/Synthesis) patcher for Skyrim SE that fixes a confirmed crash in `OStim.dll`
(`Threading::ThreadManager::stopThreadNoLock`) that happens when the player crosses a teleport door while an OStim
scene is active or closing nearby - typically triggered by ambient addons like OStim NPCs or OSolo, where the player
isn't part of the scene and can move freely.

This is the Synthesis version of the ["OStim NPC / OSolo - Room Transition CTD Fix"](https://www.nexusmods.com/skyrimspecialedition)
Advanced Patcher (originally an xEdit script), rebuilt to run automatically as part of your Synthesis pipeline instead
of requiring a manual xEdit pass.

## What it does

Scans every placed reference in your full load order and, for each one that:

1. Has a base object that is a door (`DOOR`), and
2. Has teleport data (`XTEL`) filled in - meaning it's a real teleport door between cells (purely decorative doors
   that only open within the same cell are skipped on purpose)

...attaches the `OStimCellTransitionCrashFix` Papyrus script via VMAD (no properties, local flag), so it stops any
active OStim threads before letting the player cross. Doors that already have the script are left untouched.

Everything is written as overrides into your own Synthesis patch - it never touches your original plugins.

## Requirements

- [OStim NPC / OSolo - Room Transition CTD Fix](https://www.nexusmods.com/skyrimspecialedition/mods/186626) (base mod,
  provides the compiled `OStimCellTransitionCrashFix.pex` script this patcher attaches)
- [OStim Standalone](https://www.nexusmods.com/skyrimspecialedition/mods/98163)

## Installation

In the Synthesis app, add this as a patcher pointing at this repository's URL, same as any other gallery patcher.
It will build and run automatically as part of your pipeline from then on.
