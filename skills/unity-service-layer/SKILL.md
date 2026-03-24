# Unity Service Layer Pattern

Purpose:
Separate gameplay systems from global services such as saving, networking, or player management.

Concept:
Services act as centralized systems responsible for specific domains.

Common Services:

* SaveService
* PlayerService
* InventoryService
* NetworkService
* AudioService

Guidelines:
Gameplay systems should never directly access external systems such as databases or networking layers.

Instead they should communicate with services.

Example Architecture:

Gameplay System
v
Game Service
v
Persistence / Networking

Benefits:

* Isolates infrastructure from gameplay logic.
* Makes systems easier to maintain.
* Simplifies testing and debugging.

Design Rules:

* Services should expose clear APIs.
* Avoid hardcoded dependencies between gameplay scripts and infrastructure.
* Prefer dependency injection or service registries.