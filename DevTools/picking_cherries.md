# Git Cherry-Picking Guide (Lessons Learned)

## What is Cherry-Picking?

Cherry-picking allows you to apply a commit from one branch onto another, without merging the entire branch. This is useful when you need specific changes without bringing in unrelated commits.

---

## Useful Commands

### Get the latest commit from a branch:
```bash
git rev-parse branch-name
```

### Cherry-pick that commit into your current branch:
```bash
git cherry-pick $(git rev-parse other-branch)
```

### If conflicts occur:
```bash
# Resolve conflicts, then run:
git add .
git cherry-pick --continue
```

### To cancel a cherry-pick:
```bash
git cherry-pick --abort
```

---

## Key Takeaways

- `HEAD` points to your latest commit
- `git rev-parse branch-name` retrieves the SHA at the tip of that branch
- After a cherry-pick, a **new commit** is created in your branch
- Your editor might not show changes immediately — but they're in Git (check with `git log`)
- Push your branch to sync the commit to GitHub:
  ```bash
  git push origin branch-name
  ```

---

## Case Study: Commit `447495844a2c3cf220f717631f0b9b912715ea44`

### Reason for Cherry-Picking
In the `feature/player-inventory-&-weapon-system-refactor` branch, a commit was made to update shell scripts for better maintainability. However, these updates were needed in the `feature/core-architecture-refactor` branch as well. Instead of merging the entire branch, which might bring in unrelated changes, a cherry-pick was performed to apply only the specific commit. Both branches needed the same utility shell scripts to work in the same exact way.

### Commit Details
- **Commit Message:** `chore: shell script maintenance updates`
- **Files Changed:**
  - `DevTools/shell_scripts/lint-code.sh`
  - `DevTools/shell_scripts/open-unity.fish`
  - `DevTools/shell_scripts/project-stats.sh`
- **Changes:**
  - 74 insertions
  - 15 deletions

---

## Understanding Commit Hashes (example)
After cherry-picking, the commit hash changes because Git creates a new commit with the same changes but a different parent. This is why the cherry-picked commit has a different SHA in the new branch.

---

## Final Steps
Once the cherry-pick is complete and you've verified the changes:

1. **Push your changes:**
   ```bash
   git push origin feature/core-architecture-refactor
   ```

2. **Merge or rebase as needed:**
   When merging or rebasing, Git will recognize the commit and handle it appropriately, avoiding duplicate changes.

---

-GK
