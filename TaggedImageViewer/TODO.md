### Filesystem operations
- [ ] Watch filesystem for changes and update the list of images
- [ ] Update the image cache automatically when an image is renamed or moved
- [ ] Update the image cache automatically when image data is edited on disk
- [ ] Support PSD and Krita files as well
- [	] Move the selected root dir to the app titlebar

### Collection features
- [ ] Make keyboard navigation flow across lines instead of like a grid
- [ ] Context menu with shortcuts:
  - Open with associated app (Double click)
  - Open in explorer (Ctrl+E)
  - Copy full path (Ctrl+Shift+C)
  - Copy image thumbnail to clipboard (Ctrl+C)
- [ ] Rename collections and drawings
- [ ] Customize collection thumbnail
- [ ] Skip images with missing thumbnails (optionally) 

### Viewing features
- [x] Fix zoom in/out to center on the mouse cursor
- [ ] Flip images horizontally and vertically
- [ ] Maintain zoom
- [ ] Reset zoom to 100% (Middle click)
- [ ] Scroll mouse wheel to compare more than 2 images
- [ ] Support previewing gifs

### Metadata
- [ ] Set optional per-image metadata tags
  - Name(s)
  - Species
  - Gender
  - Miscellaneous tags: actions, emotions, etc. 
- [ ] Auto-extract list of colors used in the image, and assign as tags by factor
  - e.g. orange (30%), beige (20%), white (10%), cyan (1%)
- [ ] Tag-based image search (e.g. "orange fox")