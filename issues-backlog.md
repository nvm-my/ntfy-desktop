# Issues & backlog

Known bugs and small deferred items not yet worth a roadmap entry.

**Lifecycle:** open items live under **Open** (newest first); when a fix is in flight,
add a `Status:` line naming the branch rather than removing the item. Once a fix is
**confirmed and merged**, move the item to **Resolved** with a one-line resolution and a
reference (branch/PR, plus the version once it's tagged). Resolved entries double as
release-note material.

## Open

### Bugs

_None currently._

## Resolved

#### Newly-added topic's feed was empty on first click (until restart)

A topic added in-session (most visibly one dropped into a brand-new group folder) didn't
switch the feed when clicked the first time — only an app restart fixed it. Cause: WPF-UI's
`NavigationView` doesn't reliably register a dynamically-inserted item for selection, so the
first click never raised `SelectionChanged` and `FeedViewModel.CurrentTopicId` never changed.
**Fix:** rebuild the rail on `TopicAdded` (`MainWindow.OnTopicAdded` → `RebuildTopicItems`)
instead of incrementally inserting, matching the existing `OnTopicMoved` rebuild. Branch
`fix/new-topic-feed-refresh`. First seen v0.6 while testing action buttons.
