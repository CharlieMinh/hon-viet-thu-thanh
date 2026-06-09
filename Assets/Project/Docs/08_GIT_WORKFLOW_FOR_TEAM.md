# 08 - Git Workflow For Team

## Branches

| Branch | Purpose |
|---|---|
| `main` | Stable shared baseline. |
| `develop` | Integration branch. |
| `feature/dev1` | Dev1 Grid & Hero Placement. |
| `feature/dev2` | Dev2 Enemy & Wave. |
| `feature/dev3` | Dev3 Combat & Hero Skills. |
| `feature/dev4` | Dev4 GameManager, UI & Rules. |
| `feature/dev5` | Dev5 Scene, Art, Prefab, Audio & Integration. |

## Daily Start

Each developer should update from `develop` before coding.

```bash
git checkout develop
git pull origin develop
git checkout feature/devX
git merge develop
```

Replace `feature/devX` with your own branch:

- `feature/dev1`
- `feature/dev2`
- `feature/dev3`
- `feature/dev4`
- `feature/dev5`

## After Work

Check files before commit:

```bash
git status
```

Stage only your module files:

```bash
git add Assets/Project/DevX_ModuleName
```

Commit:

```bash
git commit -m "feat(devX): short description"
```

Push:

```bash
git push origin feature/devX
```

## Pull Request Target

Feature branches should be merged into:

```text
develop
```

Do not open normal feature PRs directly into `main`.

## Files To Avoid Editing

Do not edit unless assigned:

- Another developer's scene.
- `Assets/Project/Shared`.
- `Assets/Project/Dev5_Art/Scenes/Scene_Integration.unity`.
- `ProjectSettings`.
- `Packages`.

## Unity Generated Folders

These should not be committed:

- `Library/`
- `Temp/`
- `Obj/`
- `Logs/`
- `UserSettings/`
- `Build/`
- `Builds/`

If any of these appear in `git status`, stop and ask for help.

## Commit Message Examples

Dev1:

```bash
git commit -m "feat(dev1): add grid cell click detection"
```

Dev2:

```bash
git commit -m "feat(dev2): add enemy lane movement"
```

Dev3:

```bash
git commit -m "feat(dev3): add projectile damage"
```

Dev4:

```bash
git commit -m "feat(dev4): add base hp event handling"
```

Dev5:

```bash
git commit -m "feat(dev5): setup integration camera and lighting"
```

## Conflict Prevention

- Pull `develop` before coding.
- Commit small and often.
- Do not edit shared scenes at the same time.
- Tell the team before changing shared contracts.
- Review `git status` before every commit.
- Avoid committing generated files or unrelated changes.

