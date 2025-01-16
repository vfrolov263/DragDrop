# DragDrop
Drag&Drop Mechanics.

Basic logic in:

Assets/Camera/Scripts/Scroller.cs - For level scrolling

Assets/Items/Scripts/DragDrop.cs - Dragging items on level

Some features:
- Dragging 3 different items around the level
- Items are drawn in the correct order depending on their height level
- Items are positioned on scene objects: tables, shelves, chairs, etc.
- You can scroll the level left and right
- Items do not hang in the air - they fall down if there is no surface underneath them
- Fall speed can be adjusted in the prefab (Assets/Items/Prefabs/Item.prefab)
- Items have animations and sound effects (..Items\Animations, ..\Sounds)
- Items can be placed in boxes on the level (toy box and trash box)
