# Unity Event Architecture

Purpose:
Encourage event-driven architecture in Unity projects.

Guidelines:

* Systems should communicate through events whenever possible.
* Avoid direct dependencies between gameplay systems.

Event Options:

* C# events
* UnityEvents
* ScriptableObject Event Channels

Design Goals:

* Reduce coupling between systems.
* Allow systems to be replaced independently.
* Improve testability.

Examples:

Good:
EnemyDeathEvent -> LootSystem

Avoid:
Enemy -> direct call to LootSystem