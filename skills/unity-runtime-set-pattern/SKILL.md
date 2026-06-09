# Unity Runtime Set Pattern

Purpose:
Track active instances of gameplay objects using ScriptableObject runtime sets.

Concept:
A Runtime Set is a ScriptableObject that maintains a list of active objects during gameplay.

Benefits:

* Eliminates the need for FindObjectOfType.
* Provides centralized access to active objects.
* Improves performance and organization.

Implementation Guidelines:

1. Create a ScriptableObject containing a list of objects.
2. Objects register themselves on enable.
3. Objects unregister themselves on disable.

Typical Use Cases:

* Active enemies
* Active interactables
* Active NPCs
* Active projectiles

Example Structure:

EnemyRuntimeSet
List<Enemy>

Enemy
OnEnable -> register
OnDisable -> unregister

Design Goals:

* Keep runtime state centralized.
* Allow systems to query active entities easily.
* Avoid expensive scene searches.