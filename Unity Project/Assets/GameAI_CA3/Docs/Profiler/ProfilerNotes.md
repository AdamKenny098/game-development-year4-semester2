# CA3 Profiler Notes

## Purpose

These profiler notes support the CA3 evidence pack for the Tyrant AI vertical slice. The aim is to show that the AI system was not only implemented, but also checked for performance and runtime stability during normal gameplay.


Capture files used:

```text
01_patrol_profiler.png
02_chase_attack_profiler.png
```

## AI System Profiled

The profiled AI encounter uses a four-state Tyrant behaviour model:

```text
Patrol → Search → Chase → Attack
```

The Tyrant uses Unity NavMeshAgent movement, repeated sensory checks, Blackboard values, and Behaviour Graph state switching. The main Blackboard-driven values observed during profiling were:

```text
CanSeePlayer
HearsNoise
CanAttack
IsInRange
CurrentTarget
LastKnownPlayerPosition
SearchTargetPosition
State
```

## Expected Main Cost

The main expected AI cost is perception, especially the vision check. Vision uses distance checks, field-of-view angle checks, and raycasting to determine whether the Tyrant can see the player. Hearing and attack range checks are cheaper because they mainly rely on distance comparisons.

The NavMeshAgent also contributes runtime cost when the Tyrant is chasing or searching, because destinations are updated and paths are recalculated during movement. The debug overlay also adds a small amount of UI cost because it updates live state, Blackboard, and NavMesh information during the encounter.

## Capture 1 — Patrol State

Profiler file:

```text
01_patrol_profiler.png
```

During patrol, the Tyrant follows a cyclic waypoint route through the building. The AI still runs sensory checks while patrolling so that it can react if the player enters vision or hearing range.

Observed behaviour:

```text
State = Patrol
CanSeePlayer = False
CanAttack = False
NavMeshAgent follows waypoint destination
Debug overlay remains active
```

This capture represents the baseline cost of the AI while no direct encounter is taking place. The system remained stable while the Tyrant followed its waypoint route and continued checking for player stimuli.

## Capture 2 — Chase / Attack State

Profiler file:

```text
02_chase_attack_profiler.png
```

During the active encounter, the Tyrant detects the player, moves into Chase, and then enters Attack when the player is within range. LastKnownPlayerPosition updates while the player is visible, and the debug overlay confirms that the AI is no longer in the default Patrol state.

Observed behaviour:

```text
State = Attack
CanSeePlayer = True
CurrentTarget = Player
IsInRange = True
CanAttack = True
NavMeshAgent stops during attack
```

This capture is important because active engagement is more expensive than patrol. During Chase, the NavMeshAgent destination updates toward the player. During Attack, the agent stops, faces the player, and runs the attack cycle.

## Spike Note

The visible profiler spikes occurred during the attack state and active engagement moments. This is expected to be heavier than patrol because several things happen at once:

```text
The Behaviour Graph switches into Attack
The Tyrant stops its NavMeshAgent path
The attack state updates IsAttacking
The Tyrant rotates to face the player
The debug overlay updates live combat and NavMesh values
```

The spikes were not treated as a hard failure because they occurred during the most active part of the encounter rather than during idle patrol. However, if this system were continued, the main optimisation target would be reducing unnecessary per-frame updates in the debug overlay and limiting repeated path or attack-state updates while the Tyrant is already attacking.

## Tick and Responsiveness Notes

During development, the sensory actions were changed to remain active so the Tyrant could continue detecting the player while patrolling, searching, chasing, or attacking. This improved responsiveness because the AI no longer became locked into patrol while ignoring new stimuli.

The final structure separates the AI into three clear layers:

```text
Senses update Blackboard values
Brain/state logic chooses Patrol, Search, Chase, or Attack
State actions execute movement or attack behaviour
```

This made the system easier to debug and explain in the video demonstration.

## Robustness Notes

The following behaviours were tested:

```text
Player enters vision range during patrol → Tyrant switches to Chase
Player gets close enough → Tyrant switches to Attack
Player is heard but not seen → Tyrant switches to Search
Player breaks line of sight → Tyrant searches using last known position
Search completes → Tyrant returns to Patrol
```

The debug overlay was used during testing to confirm the current state, perception values, combat range values, and NavMesh destination.

## Limitations of the Capture

These captures were taken inside the Unity Editor rather than in a standalone build, so the numbers include Editor, Game view, UI, and profiling overhead. The screenshots are therefore used as development evidence rather than as a final platform benchmark. A standalone build profile would provide cleaner runtime numbers, but for the CA3 evidence pack these captures demonstrate that the AI was tested under patrol and active encounter conditions.

## Final Evaluation

The Tyrant AI remained functional across the main encounter loop. The most important performance consideration was keeping perception responsive without allowing movement actions to block the Behaviour Graph. The debug overlay and profiler captures provide evidence that the AI was tested during patrol, chase, and attack conditions rather than only being shown in a static scene.

The main observed spikes appeared during the attack state. This is acceptable for the current vertical slice, but it is also the clearest target for future optimisation if the project were continued.
