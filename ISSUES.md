## Known Issues

- Theming is naive
- Toolbar buttons are based on `<a>` elements, `<button>` would prevent URL appearing in browser bottom-left
- Using `<button>` breaks menu navigation (dependent on `focus-within`)
- Placement of submenus brittle (attempts to use only CSS)
- Pane content can cause overflow in main container when resized (which makes everything look broken)
- Pane states are not persisted
- Totally non-responsive
- No accessibility support