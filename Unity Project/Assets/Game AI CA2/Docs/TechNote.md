# Technical Note

## Behaviour Tree Structure

The Behaviour Tree is structured around six core behaviour branches:

- **Patrol**: The AI moves toward randomly generated patrol points.
- **Chase**: Triggered when the AI can see the player.
- **Attack**: Triggered when the player is within ability range.
- **Search**: Triggered when the AI loses sight of the player or hears a noise. The AI moves to the last known or heard position.
- **Flee**: Triggered when the AI’s health drops to or below 15% of its maximum health.
- **Death**: Triggered when the AI reaches 0 health.

These branches are evaluated in priority order, with Death and Flee overriding all other behaviours. Chase and Attack are driven by perception and combat evaluation.

---

## Decorators and Timing

Sensory nodes (Vision, Hearing, CombatSense, Health checks) are continuously evaluated using repeating execution, ensuring the AI reacts in real time.

A timeout mechanism is used in the Search behaviour. If the AI cannot rediscover the player within a set duration, it abandons the search and returns to Patrol. This prevents the AI from becoming stuck in a search state indefinitely.

---

## Perception System

The AI uses both vision and hearing:

- **Vision**
  - Distance: 12 units
  - Angle: 90 degrees
  - Uses raycasting every frame to confirm line of sight

- **Hearing**
  - Radius: 30 units
  - Detects nearby noise sources

- **Last Known Position**
  - Stored as a Transform
  - Passed to the Search behaviour when the player is lost
  - Allows the AI to move to the last seen or heard location instead of stopping

---

## Blackboard Usage

The Behaviour Tree uses a shared Blackboard for communication between nodes. Key variables include:

- `CanSeePlayer`
- `IsDead`
- `IsLowHealth`
- `IsInRange`
- `HearsNoise`
- `HasLastPlayerPos`

These values are updated by sensory nodes and used by decision logic to select appropriate behaviour branches.

---

## Design Trade-Off

A fully animated character system was initially planned to visually represent each state more clearly. However, this was not implemented due to time constraints and because it did not directly contribute to the assessment criteria. Focus was instead placed on behaviour correctness, robustness, and debugging clarity.