# Hon Viet Thu Thanh Team Workflow

## 1. Project Overview

Hon Viet Thu Thanh is a Unity 3D tower defense + strategy game.

The project uses an event-driven module architecture so each developer can work on one independent module without tightly coupling code across the whole project. Communication between modules should happen through shared enums, interfaces, data assets, and events.

Team structure:

- 5 developers
- 5 independent modules
- 1 shared contract area
- 1 integration scene controlled by Dev5 or the team leader

## 2. Folder Ownership

| Folder | Owner | Purpose |
|---|---|---|
| `Assets/Project/Shared` | Team / leader-reviewed | Common enums, interfaces, events, and data. Change carefully and notify the team before editing. |
| `Assets/Project/Dev1_GridPlacement` | Dev1 | Grid system and hero placement. |
| `Assets/Project/Dev2_EnemyWave` | Dev2 | Enemy spawning, enemy movement, and wave system. |
| `Assets/Project/Dev3_Combat` | Dev3 | Combat logic, projectiles, targeting, and hero skills. |
| `Assets/Project/Dev4_GameManager` | Dev4 | GameManager, UI, economy rules, win/lose rules, and gameplay state. |
| `Assets/Project/Dev5_Art` | Dev5 | Scene setup, art, prefabs, audio, visual polish, and integration scene. |

## 3. Scene Ownership

| Scene | Owner |
|---|---|
| `Assets/Project/Dev1_GridPlacement/Scenes/Scene_Dev1_Placement.unity` | Dev1 |
| `Assets/Project/Dev2_EnemyWave/Scenes/Scene_Dev2_EnemyWave.unity` | Dev2 |
| `Assets/Project/Dev3_Combat/Scenes/Scene_Dev3_Combat.unity` | Dev3 |
| `Assets/Project/Dev4_GameManager/Scenes/Scene_Dev4_UI.unity` | Dev4 |
| `Assets/Project/Dev5_Art/Scenes/Scene_Dev5_Art.unity` | Dev5 |
| `Assets/Project/Dev5_Art/Scenes/Scene_Integration.unity` | Dev5 / leader only |

## 4. Git Workflow

Branches:

- `main`: stable release only
- `develop`: integration branch
- `dev1-grid-placement`
- `dev2-enemy-wave`
- `dev3-combat-skill`
- `dev4-gamemanager-ui`
- `dev5-art-integration`

Do not push directly to `main` or `develop`. Each developer works on their own module branch and merges updates from `develop` before coding.

## 5. Daily Workflow Commands

Start of day:

```bash
git checkout develop
git pull origin develop
git checkout devX-module
git merge develop
```

After work:

```bash
git status
git add only your module files
git commit -m "feat(devX): short description"
git push origin devX-module
```

Replace `devX-module` with your real branch name, for example:

- `dev1-grid-placement`
- `dev2-enemy-wave`
- `dev3-combat-skill`
- `dev4-gamemanager-ui`
- `dev5-art-integration`

## 6. Conflict Prevention Rules

- Do not edit `Scene_Integration.unity` unless you are Dev5 or the leader.
- Do not edit other developers' scenes.
- Do not edit `Assets/Project/Shared` without notifying the team.
- Commit small and often.
- Pull `develop` before coding.
- Add only the files from your own module unless the team has agreed otherwise.
- Keep scenes clean and avoid adding temporary test objects to shared or integration scenes.
