## Help for Sutil Oxide Demo

### Getting Started

- Drag the tabs between different locations

- Hover over pane edges to resize

- Click on a tab to toggle its visibility

### About the demo

The demo app uses the docking system to present some basic controls:

- Toolbar
- Statusbar
- File Explorer
- Text Editor
- Message Log
- Markdown Preview
- Several dummy panes to play around with resizing and moving

### Toolbar

- `File` is a drop-down button menu, with tools to manage the files you see in the `File Explorer` (see below)

- `View` is a drop-down choice menu, allowing you change themes. Theming still a work-in-progress, and Dark mode will just annoy you at this point.

- `Help` is plain button that will show this pane if not already visible.


### Statusbar

This contains a simple text-based clock



### File Explorer

SutilOxide provides an IFileSystem interface, with a LocalStorage implementation (use your browser's devtools to see what's going on). At initialisation, README.md is uploaded into this file system and an edit session is started, which you can see in the centre pane. You can edit that file, and use `Ctrl-S` or `File->Save` to commit changes.


- Use `File->New File` to create a new file. Double-click on a file to start editing and previewing.

- Delete a file with `File->Delete`, and see `Modal` in action.

- Rename a file with `File->Rename`


### File Editor

This is the `Ace` editor, with a minimal wrapper that will adjust its syntax highlighting for the following file types: `.md`, `.css`, `.html`, `.js`

- Double-click on a file in the `File Explorer` to start editing.

- Use `File->Save` to save the current file. Notice that the pane header shows you when the file is edited.

- You can also save with `Control-S`


### Message Log

The `Messages` window will show any messages written to `SutilOxide.Logging.log`, and autoscroll to the end.


