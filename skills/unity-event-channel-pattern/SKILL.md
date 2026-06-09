# Unity Event Channel Pattern

Purpose:
Implement event-driven communication between Unity systems using ScriptableObject event channels.

Concept:
An Event Channel is a ScriptableObject that acts as a mediator between systems.

Benefits:

* Removes direct dependencies between systems.
* Allows systems to communicate through shared event assets.
* Improves modularity and testability.

Implementation Guidelines:

When generating an event channel:

1. Create a ScriptableObject class representing the event.
2. Allow listeners to subscribe and unsubscribe.
3. Provide a method to raise the event.

Typical Structure:

* EventChannel ScriptableObject
* Listener components
* Systems raising the event

Example Use Cases:

* Enemy death
* Item collected
* Player damaged
* Quest updated
* UI notifications

Design Goals:

* Systems should not reference each other directly.
* Event channels should act as communication hubs.