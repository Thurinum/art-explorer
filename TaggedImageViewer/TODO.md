### Filesystem operations
- [x] Watch filesystem for changes and update the list of images
- [x] Update the image cache automatically when an image is renamed
- [x] Update the image cache automatically when an image is edited
- [ ] Update the image cache automatically when an image is moved
- [ ] Update the image cache automatically when an image is deleted
- [ ] Update the image cache with changes made outside the app
- [ ] Also cache the thumbnails of collections
- [ ] Support PSD and Krita files as well

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
- [x] Flip images horizontally and vertically
- [x] Option to maintain zoom on select
- [x] Reset zoom to 100% (Middle click)
- [x] Scroll mouse wheel to compare more than 2 images
- [ ] Support previewing gifs

### Search and metadata
- [ ] Search for images by name and tag (e.g. "orange fox")
- [ ] Set optional per-image metadata tags
  - Name(s)
  - Species
  - Gender
  - Miscellaneous tags: actions, emotions, etc.
- [ ] Collection defaults
- [ ] Auto-extract list of colors used in the image, and assign as tags by factor
  - e.g. orange (30%), beige (20%), white (10%), cyan (1%)