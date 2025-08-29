# Git Workflow for Unity Projects

This document outlines safe Git practices for working within my project (this project's git workflow), focusing on avoiding Library corruption and managing branches effectively. Based on Unity's cache-heavy nature, we prioritize isolation and clean state management.

## Key Principles
- **Always commit/stash changes** before branch operations.
- **Use worktrees** for isolated branch testing to avoid Library corruption.
- **Close Unity** when switching branches within the same workspace.
- **Reimport All** in Unity if assets seem off after switching.

## .gitignore Check
Your `.gitignore` is properly configured to ignore:
- `/Library/` (Unity cache – main corruption risk)
- `/Temp/`, `/Logs/`, `/UserSettings/`
- Build artifacts (`*.apk`, `**/[Oo]bj/`, etc.)
- OS files (`.DS_Store`, `Thumbs.db`)
- IDE files (`.vs/`, `.idea/`)

This protects against tracking auto-generated files.

## Safe Branch Switching

### Option 1: Direct Checkout (Riskier, Use with Caution)
1. Check status and commit/stash:
   ```fish
   git status
   git add -A && git commit -m "WIP: before switch"
   # OR: git stash push -m "stash"
   ```
2. Close Unity Editor.
3. Switch branch:
   ```fish
   git fetch origin
   git checkout feature/branch-name
   git pull origin feature/branch-name
   ```
4. Reopen Unity and reimport if needed (Project window > Reimport All).

### Option 2: Worktrees (Recommended for Unity)
Worktrees create isolated directories, preventing Library conflicts. You can have multiple Unity instances open simultaneously.

#### Creating Worktrees
```fish
# Create worktree for a branch
git worktree add ../Zombtoy-BranchName feature/branch-name

# Open Unity in the worktree
open -a "Unity" ../Zombtoy-BranchName
```

#### Managing Multiple Worktrees
- **List worktrees**: `git worktree list`
- **Remove worktree**: `git worktree remove ../Zombtoy-BranchName`
- **Clean up broken ones**: `git worktree prune`

#### Example: Testing Multiple Branches
```fish
git worktree add ../InventoryRefactor feature/refactor-inventory
git worktree add ../MusicFix feature/music-manager-fix
git worktree add ../MainTest main

# Open each in separate Unity instances
open -a "Unity" ../InventoryRefactor
open -a "Unity" ../MusicFix
open -a "Unity" ../MainTest
```

**Do you need to close Unity with worktrees?** No! Since worktrees are isolated, you can keep Unity open in one worktree while working on others. Only close Unity if switching branches *within the same worktree* (rare).

## Common Scenarios

### Creating a New Branch
```fish
git checkout main
git pull origin main
git checkout -b feature/new-feature
```

### Handling Conflicts
- Abort switch: `git checkout -- . && git clean -fd`
- Reset to safe state: `git reset --hard HEAD`

### Pushing and PRs
- Push branch: `git push origin feature/branch-name`
- Create PR via GitHub UI or CLI.

## Tips for Unity + Git
- **Backup branches**: `git branch backup/my-work` before risky operations.
- **Test in worktrees**: Avoid committing broken builds to main.
- **Rebase vs Merge**: Use `git rebase` for clean history; `git merge` for preserving context.
- **Force push safely**: `git push --force-with-lease origin branch-name` (avoids overwriting others' work).
- **Unity-specific**: If Library issues persist, delete Library folder (Unity regenerates it).

## Resources
- [Unity .gitignore Template](https://github.com/github/gitignore/blob/master/Unity.gitignore)
- Git Worktrees: `git worktree --help`

This workflow ensures safe, efficient development without Unity corruption risks.
