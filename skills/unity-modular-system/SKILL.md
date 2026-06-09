# Unity Modular System Generator

Purpose:
Generate modular gameplay systems for Unity.

Architecture Rules:

* Systems must be reusable and modular.
* Avoid tightly coupled dependencies.
* Prefer interfaces when extensibility is expected.

Structure Guidelines:
Generated systems should include:

* Main system class
* Data containers
* Events or interfaces
* Configuration through inspector

Configuration:

* Prefer ScriptableObjects for data and configuration.

Goals:

* Easy to extend systems.
* Easy to replace modules.
* Reduce code duplication.