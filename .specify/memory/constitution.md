<!--
Sync Impact Report
Version change: (none) → 1.0.0
Modified principles: N/A (initial ratification)
Added sections:
  - Core Principles: I. Gameplay-First Design, II. Modular & Data-Driven Architecture,
    III. Test Coverage for Core Systems (NON-NEGOTIABLE), IV. Performance & Responsiveness Budgets,
    V. Simplicity & Iterative Scope
  - Technical Constraints
  - Development Workflow
  - Governance
Removed sections: N/A
Deferred TODOs:
  - TODO(TECH_STACK): Engine/language/framework not yet chosen; record here once decided.
Templates requiring follow-up: none (no dependent templates modified by this command)
-->

# Game RPG Constitution

## Core Principles

### I. Gameplay-First Design
Every feature MUST be justified by the player-facing experience it produces before any
technical design begins. Systems (combat, inventory, dialogue, progression, etc.) are
prototyped and validated for fun and clarity before being hardened for production.
Purely technical or infrastructure work MUST trace back to a gameplay need it unblocks.

**Rationale**: An RPG lives or dies on its feel and player experience; technical elegance
that does not serve gameplay is wasted effort.

### II. Modular & Data-Driven Architecture
Game systems (characters, items, quests, dialogue, encounters, stats) MUST be defined as
data (config files, resources, or structured assets) rather than hard-coded into engine
logic wherever feasible. Each system MUST expose a clear boundary/interface so it can be
built, tested, and iterated independently of other systems (e.g., combat logic must not
directly reach into UI code).

**Rationale**: RPGs grow large content sets (items, enemies, quests); data-driven, modular
systems let content and balance change without code changes and keep systems testable in
isolation.

### III. Test Coverage for Core Systems (NON-NEGOTIABLE)
Core game logic — combat resolution, stat/formula calculations, inventory rules, save/load,
and quest state transitions — MUST have automated tests covering normal and edge cases
before being merged. Rendering, animation, and pure presentation code are exempt from this
requirement but MUST NOT contain gameplay-affecting logic.

**Rationale**: Numerical/state bugs in core systems (damage formulas, save corruption,
quest soft-locks) are costly to find late and directly break the player experience.

### IV. Performance & Responsiveness Budgets
Each target platform MUST have an explicit frame-rate/latency budget agreed before a
system is built, and that system MUST be measured against the budget before being marked
done. Input-to-action latency (movement, attacks, menu navigation) is treated as a
first-class requirement, not an afterthought.

**Rationale**: An RPG with hitches, input lag, or unpredictable frame times undermines
combat and exploration regardless of how correct the underlying logic is.

### V. Simplicity & Iterative Scope
Start with the simplest system that satisfies the current milestone's gameplay goal
(YAGNI). New abstractions, engines, or frameworks MUST be justified by a concrete,
current need — not a hypothetical future one. Cut scope before cutting quality on
shipped systems.

**Rationale**: Game projects are especially prone to scope creep (extra systems,
mechanics, content); unjustified complexity slows iteration and risks the project
never shipping.

## Technical Constraints

TODO(TECH_STACK): Engine, language, and target platform(s) have not yet been chosen for
this project. Once decided, this section MUST be updated to record: the engine/framework,
primary language(s), minimum supported platforms, and any third-party dependencies that
are pre-approved vs. requiring review.

All technical choices MUST otherwise remain consistent with Principles I-V above: prefer
data-driven content, keep systems modular, and keep the toolchain as simple as the project
genuinely needs.

## Development Workflow

- All changes to core game systems (see Principle III) MUST go through code review before
  merging, with the reviewer explicitly checking test coverage and adherence to the
  modular boundaries defined in Principle II.
- New features MUST be scoped as a spec (via `/speckit-specify`) before implementation
  when they introduce a new gameplay system or materially change an existing one; small
  fixes and content-only changes (e.g., tuning numbers, adding items via data files) are
  exempt.
- Performance-sensitive changes (Principle IV) MUST include a note on expected impact and,
  where practical, a before/after measurement.

## Governance

This constitution supersedes all other project practices and templates where they
conflict. Amendments are made via `/speckit-constitution` and MUST include: the proposed
change, a rationale, and the resulting version bump per semantic versioning:

- **MAJOR**: Backward-incompatible removal or redefinition of a principle or governance
  rule.
- **MINOR**: A new principle or materially expanded section is added.
- **PATCH**: Wording clarifications, typo fixes, or non-semantic refinements.

All feature specs, plans, and task lists produced by other Spec Kit commands MUST be
checked for compliance with these principles; unresolved conflicts MUST be flagged and
resolved before implementation proceeds. Complexity that violates Principle V (Simplicity)
MUST be explicitly justified in the relevant plan document.

**Version**: 1.0.0 | **Ratified**: 2026-08-25 | **Last Amended**: 2026-08-25
