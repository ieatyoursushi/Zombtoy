# Git Stashing Guide (Lessons Learned)

## What is Git Stash?

Git stash allows you to temporarily save uncommitted changes in your working directory without committing them, so you can switch branches or pull updates safely. This is useful when you start working on one topic in a branch but realize the changes belong elsewhere.

---

## Useful Commands

### Save your changes to a stash:

```bash
git stash push -m "WIP: descriptive message"
```

### List all stashes:

```bash
git stash list
```

### Apply a stash to your current branch:

```bash
git stash apply stash@{0}
```

### Apply and remove a stash:

```bash
git stash pop stash@{0}
```

### Inspect a stash before applying:

```bash
git stash show -p stash@{0}
```

### Drop a stash:

```bash
git stash drop stash@{0}
```

### Stash only specific files:

```bash
git stash push path/to/file1 path/to/file2
```

### Stash interactively (choose hunks):

```bash
git stash push -p
```

---

## Key Takeaways

* A stash is **not a commit**; it’s a temporary snapshot of your changes.
* `git stash apply` just applies changes to your working directory — you still need to `git add` and `git commit`.
* You can have **multiple stashes** stored in a stack (`stash@{0}`, `stash@{1}`, etc.).
* Stashes are branch-agnostic: you can apply them to any branch, even one different from where you created them.

---

## Case Study: Accidental Feature Work

### Scenario

While working on the `feature/topic-A` branch, I accidentally implemented changes for `feature/topic-B`. Committing directly would have mixed topics in the wrong branch.

### Solution

1. Stashed the changes for Topic B:

```bash
git stash push -m "WIP: topic-B changes"
```

2. Switched to the correct branch:

```bash
git checkout feature/topic-B
```

3. Applied the stash:

```bash
git stash pop stash@{0}
```

4. Committed the changes on the correct branch:

```bash
git add .
git commit -m "Implement Topic B"
```

This allowed me to **“teleport” work between branches safely** without committing to the wrong branch.

---

## Practical Tips

* Always give your stash a **descriptive message** to remember what it contains.
* Use `git stash branch <new-branch>` to **create a new branch from a stash** if you want a cleaner workflow.
* You can combine multiple stashes, but be careful of conflicts.
* Think of stashes as a **temporary clipboard** — nothing is finalized until you commit.

---

## Final Steps

Once your changes are safely applied and committed:

1. **Push your branch:**

```bash
git push origin feature/topic-B
```

2. **Clean up old stashes** if no longer needed:

```bash
git stash drop stash@{0}
```

---

-GK

---

