# Issues & backlog

Known bugs and small deferred items not yet worth a roadmap entry.

**Lifecycle:** open items live under **Open** (newest first); when a fix is in flight,
add a `Status:` line naming the branch rather than removing the item. Once a fix is
**confirmed and merged**, move the item to **Resolved** with a one-line resolution and a
reference (branch/PR, plus the version once it's tagged). Resolved entries double as
release-note material.

## Open

### Bugs

#### "Messages while away" notification shown for already-read messages after delete

Repro: delete a message, then close and re-open the app — the "messages while away"
notification appears even though the deleted message was already read. Catch-up should
fetch only messages since the last-read timestamp, so a read-then-deleted message should
not count. If the message is *not* deleted, the notification correctly does not appear,
which points at the delete path mishandling the since-timestamp / read bookkeeping.

#### Toast click navigates to topic feed but rail selection isn't updated

Clicking a toast notification opens/shows the app and navigates to the topic's feed, but
the rail doesn't mark that topic as selected — "All topics" stays selected instead. Happens
whether the app is already open or closed when the toast is clicked.

### Small items

#### Main window should open maximized (or near-full-screen) from the tray icon

When the window is opened from the tray icon it should be maximized, or at least occupy
most of the screen, rather than its current size.
Status: fixed on `feature/window-maximized-on-open` (default maximized on fresh install +
full size/position/maximized-state persistence across restarts). Awaiting merge.

## Resolved

#### Newly-added topic's feed was empty on first click (until restart)

A topic added in-session (most visibly one dropped into a brand-new group folder) didn't
switch the feed when clicked the first time — only an app restart fixed it. Cause: WPF-UI's
`NavigationView` doesn't reliably register a dynamically-inserted item for selection, so the
first click never raised `SelectionChanged` and `FeedViewModel.CurrentTopicId` never changed.
**Fix:** rebuild the rail on `TopicAdded` (`MainWindow.OnTopicAdded` → `RebuildTopicItems`)
instead of incrementally inserting, matching the existing `OnTopicMoved` rebuild. Branch
`fix/new-topic-feed-refresh`. First seen v0.6 while testing action buttons.
