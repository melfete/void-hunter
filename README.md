# Void Hunter

A 2D action platformer made with **Unity 6** and **C#**. Fight your way through a dark world full of shadow creatures, master precise platforming and reach the mysterious portal at the end of the level.

<!-- TODO: Gameplay-GIF hier einfuegen, z. B. mit ScreenToGif aufnehmen:
![Gameplay](Docs/gameplay.gif)
-->

## Features

- **Fluid movement:** running, jumping, wall sliding, wall jumps and a dash
- **Two attack types:** a fast melee attack and a ranged projectile attack
- **Healing** with a cooldown
- **Three enemy types with their own AI:**
  - *Flying Angel*: hovers through the air and attacks the player
  - *Burning Ghoul*: ground enemy that chases the player
  - *Wizard*: ranged enemy that casts fireballs
- **Portal finale:** the exit portal appears once the key enemies are defeated, followed by a camera zoom and fade-out ending sequence
- Main menu, pause menu with volume control, game over screen with restart

## Controls

| Action      | Keys                       |
|-------------|----------------------------|
| Move        | A / D or Arrow Left / Right |
| Jump        | Space                      |
| Wall jump   | Space while on a wall      |
| Dash        | Shift                      |
| Attack      | Left mouse button or Ctrl  |
| Projectile  | Right mouse button or Alt  |
| Heal        | H                          |
| Pause       | Esc                        |

## Play

Download the latest Windows build from the [Releases](../../releases) page, unzip it and start `Void Hunter.exe`.

## Code overview

All gameplay code is in [`Assets/Scripts`](Assets/Scripts):

| Script | Responsibility |
|--------|----------------|
| [`PlayerController`](Assets/Scripts/PlayerController.cs) | Movement, wall jump, dash, attacks, healing, animations and sounds |
| [`PlayerHealth`](Assets/Scripts/PlayerHealth.cs) | Health, damage, knockback, death |
| [`EnemyController`](Assets/Scripts/EnemyController.cs) | Enemy AI: patrol, chase, melee and ranged attacks, movement sounds |
| [`EnemyHealth`](Assets/Scripts/EnemyHealth.cs) | Enemy health and knockback, death event used by the portal |
| [`Projectile`](Assets/Scripts/Projectile.cs) | Projectiles for player and enemies, layer-based hit detection |
| [`PortalManager`](Assets/Scripts/PortalManager.cs) | Activates the exit portal once the required enemies are dead |
| [`GameEndManager`](Assets/Scripts/GameEndManager.cs) | Ending sequence: camera zoom and fade to black |
| [`GameManager`](Assets/Scripts/GameManager.cs) | Game state, game over screen, restart |
| [`CameraController`](Assets/Scripts/CameraController.cs) | Smooth camera follow with level bounds |
| [`PauseMenu`](Assets/Scripts/PauseMenu.cs) / [`MainMenuManager`](Assets/Scripts/MainMenuManager.cs) | Menus and volume settings |

## Tech

- Unity 6 (6000.1.1f1), Universal Render Pipeline 2D
- C#, Rigidbody2D physics, Tilemaps, Animator state machines
- Physics movement in `FixedUpdate`, input in `Update`
- Static C# events to decouple enemy deaths from the portal logic

## Open in Unity

1. Clone the repository
2. Open the folder with **Unity 6000.1.1f1** (or a newer Unity 6 version) via Unity Hub
3. Open `Assets/Scenes/MainMenu.unity` and press Play

## Credits

<!-- TODO: Quellen und Lizenzen der Assets eintragen, z. B.:
- Player sprites: "<Pack name>" by <Artist> (<Link>), <License>
- Enemy sprites: ...
- Backgrounds / Tileset: ...
- Sound effects and music: ...
-->

Game design, programming and level design by **melfete**.
