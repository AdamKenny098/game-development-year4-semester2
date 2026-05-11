# Critical Discussion

## Design Choices

A Behaviour Tree was chosen over a Finite State Machine due to its modular structure and ease of debugging. The Unity Behavior package provides clear visualisation of execution flow, making it easier to understand and debug complex AI behaviour compared to FSMs.

The behaviour was structured into distinct branches (Patrol, Chase, Attack, Search, Flee, Death) to ensure clarity and separation of responsibilities. This allowed each behaviour to be developed and debugged independently while still interacting through shared Blackboard variables.

---

## Challenges and Issues

The most significant issue encountered was the interaction between Chase and Attack. Initially, Chase would override Attack due to incorrect condition handling and lack of separation between "in range" and "can attack" logic. This resulted in unstable and unrealistic behaviour.

Another major issue was state interruption. Actions such as Chase continued running even when higher-priority states (Flee or Death) were triggered. This was resolved by adding explicit state checks within actions to ensure they terminate correctly when the AI state changes.

These issues took the longest to resolve and highlighted the importance of clear state ownership and robust guard conditions.

---

## Improvements for CA3

For future development, the system could be improved by:

- Adding smoother transitions between behaviours, such as turning animations or blending states to improve realism.
- Expanding the perception system with more nuanced awareness (e.g. gradual suspicion rather than binary detection).
- Integrating a more complete animation system to visually represent behaviour changes more clearly.

These improvements would enhance both realism and player experience.