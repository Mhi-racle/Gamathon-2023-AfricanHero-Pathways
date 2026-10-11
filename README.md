# Path Puzzle — Ananse and the Gourds of Wisdom

A 3D puzzle game rooted in Ghanaian folklore. Kwaku Ananse, the clever spider of Akan tales, is gathering the world's wisdom into his gourds. The way to each gourd is a maze of blocks, and **tapping a block turns it a quarter turn**. Rotate the blocks to open a path, then guide Ananse to the gourd before time runs out.

Built solo in Unity for PC and Android, with low-end Android phones as the main target.

## How to play

| | PC | Android |
|---|---|---|
| Rotate a block | Left-click it | Tap it |
| Move Ananse | WASD / arrow keys | On-screen joystick |
| Pause | Esc | Pause button or Back |

- Reach the gourd before the timer hits zero. The timer turns red in the last 10 seconds.
- A block won't turn while Ananse is standing on it. It wobbles instead, so he can never be trapped inside a wall.
- From Level 11, some blocks **turn by themselves** on a steady beat, so timing matters as much as layout.
- Ananse comments (cheekily) when you pause, win or lose.

## Content

- **15 handcrafted levels** with rising difficulty: a dialogue-led tutorial in Level 1, larger mazes from Level 7 and auto-rotating blocks from Level 11.
- Level selection with saved progress. Beating a level unlocks the next, and progress never goes backwards.
- Every level is designed and play-tested by hand to be solvable. Auto-rotating blocks are chosen at random each attempt, but always the same number per level, so difficulty stays fair.

## Opening and building

- **Unity 6000.4.2f1** (Unity 6) with the **Universal Render Pipeline**.
- Open the project folder in Unity Hub and let it import. The first import takes a few minutes.
- Start from `Assets/Scenes/Main Menu.unity`.
- Build: *File → Build Profiles* → choose **Android** or **Windows**. The scene list must contain Main Menu, Level 1–15 and LevelSelection.
- Android: minimum API level 25.

## Architecture

All game code is in `Assets/Scripts`. Everything else under `Assets/` is art, audio or third-party tooling (see Credits).

```
Main Menu ──► LevelSelection (ManageLevels + levelSelector buttons) ──► Level N
                                     ▲                                  │
                                     └── LevelProgress (PlayerPrefs) ◄──┘ on win

Level N
  GameManager ─── owns GameState: Intro → Playing ⇄ Paused → Won | Lost
   │               enables/disables input, shows panels, loads scenes
   ├─ DialogManager / DialogueTrigger   intro conversation (Intro state)
   ├─ Timer                             counts down only while Playing → TimeUp()
   ├─ RotateBlock                       turns tapped blocks (mouse + multi-touch)
   │    └─ BlockSpin                    eased 90° turn, per-block lock, "blocked" wobble
   ├─ AssignAutoRotation → AutoRotation self-turning blocks in later levels
   └─ FunnyRemarksManager               Ananse's lines for pause / win / lose
  PlayerController                      joystick or keyboard movement, camera-relative
  target (gourd trigger)                → GameManager.TargetReached()
  CollectableSpawner                    guide line from Ananse to the gourd
```

Design notes:

- **One source of truth for game flow.** Scripts ask `GameManager.Instance.IsPlaying` instead of switching each other on and off, which removes a class of pause, win and dialogue state bugs.
- **Input is safe to spam.** A block ignores taps while it is turning, taps on UI never reach the maze, and the tap that closes a menu or dialogue is ignored for gameplay.
- **Mobile first.** Multi-touch lets one thumb hold the joystick while the other rotates blocks. Hot paths avoid per-frame allocations (the timer label only rebuilds when its number changes, and the guide line reuses a buffer).
- **Names in one place.** Scene names, tags and save keys live in `GameConstants`. Save data goes through `LevelProgress`.

| Folder | Contents |
|---|---|
| `Scripts/` | `GameManager`, `GameConstants`, `LevelProgress`, `Timer`, `target`, `BGMusic` |
| `Scripts/Path/` | Block rotation (`RotateBlock`, `BlockSpin`, `AutoRotation`, `AssignAutoRotation`) and the guide line |
| `Scripts/Player/` | Ananse's movement and camera helper |
| `Scripts/GamePlay/` | Dialogue and Ananse's remarks |
| `Scripts/Menus/` | Main menu and level selection |

## Tools and technologies

- Unity 6 · C# · Universal Render Pipeline · Cinemachine · TextMeshPro · Unity UI
- LeanTween for UI animation; Joystick Pack for the on-screen joystick
- Unity Device Simulator for testing phone screen sizes
- Git for version control

## Credits

Third-party assets are used under their respective licences: LeanTween, Joystick Pack, low-poly nature and village packs (DinV Studio, Downtown Game Studio, Polytope Studio, Stylized Flora Pack, LowlyPoly, Polygon Starter), Game GUI Vol1, Unity Particle Pack, and the music and sound effects in `Assets/sound`. All gameplay code, level design and writing are original.

## Future development

- **New folktale chapters with new tile mechanics**, for example *Ananse and the Pot of Wisdom* (tiles that crack after one crossing) or *How Ananse Got the Sky God's Stories* (story tiles that must be visited in order).
- **More folklore characters** as helpers and rivals: Ntikuma, Aso, Onyankopon and the trickster's animal adversaries, each with a signature tile.
- **Seasonal events** tied to Ghanaian festivals (Homowo, Aboakyir, Christmas), with themed mazes and limited-time gourds.
- **Procedurally generated endless mazes**: carve a guaranteed path first, then scramble the rotations, so every maze is solvable by construction.
- Stars and best times per level, settings for music, sound and vibration, and localisation into Twi, Ga and Ewe.
