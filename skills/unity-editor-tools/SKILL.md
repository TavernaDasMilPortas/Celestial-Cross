# Unity Editor Tools

Purpose:
Improve workflow and usability of gameplay systems inside the Unity Editor.

Guidelines:
When systems require complex configuration, generate editor tools.

Possible Tools:

* CustomEditors
* PropertyDrawers
* EditorWindows
* Validation helpers

Rules:

* Editor scripts must be placed in Editor folders.
* Editor code must not affect runtime builds.

Goals:

* Simplify configuration.
* Improve debugging.
* Allow designers to interact with systems without code.