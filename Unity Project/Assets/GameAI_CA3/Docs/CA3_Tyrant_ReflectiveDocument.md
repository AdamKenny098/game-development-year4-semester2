# CA3 Reflective Document — Tyrant AI Vertical Slice

**Student:** Adam Kenny (20102588)  
**Module:** Advanced 3D Game Development / AI CA3  
**Project:** Tyrant AI Vertical Slice  
**Engine:** Unity 6  
**Scene:** `02_VerticalSlice`  
**Final AI States:** Patrol, Search, Chase, Attack  

---

## Section 1 — Integrated Technical Account

The final CA3 vertical slice is built around a single readable AI encounter: a Tyrant-style NPC that patrols the level, reacts to player stimuli, searches remembered locations, chases the player when detected, and attacks when in range. The purpose of the implementation was not to create a large enemy system, but to demonstrate a complete and observable AI pipeline. The final system integrates NavMesh movement, a high-level state model, Behaviour Graph logic, Blackboard values, perception, and a live debug overlay.

The movement layer uses Unity’s NavMeshAgent. The Tyrant follows a cyclic waypoint route using a `Vector3` list stored on the Blackboard. During Patrol, the AI moves through the route and loops back to the first point after reaching the final waypoint. This satisfies the navigation requirement while keeping the encounter controlled enough for testing and video demonstration. The patrol route is deliberately compact so that the player can trigger the AI’s senses during normal movement through the level.

The state layer was simplified during CA3 into four states: Patrol, Search, Chase, and Attack. Earlier CA work contained leftover health, fleeing, threat, damage, and ability-slot logic. Those systems made sense in a broader combat prototype, but they conflicted with the final Tyrant concept. The Tyrant is designed as an unkillable stalker NPC, so low-health and flee behaviour were removed. This produced a cleaner and more explainable state model. Patrol is the default behaviour. Search is used both when the player is heard and when the Tyrant loses the player’s last known position. Chase is used when the player is visible. Attack is used when the player is close enough to be caught.

The Behaviour Graph is organised into three practical layers. First, repeated sensory nodes update Blackboard values. Vision writes values such as `CanSeePlayer`, `CurrentTarget`, `LastKnownPlayerPosition`, and `HasLastKnownPlayerPosition`. Hearing writes `HearsNoise`, `LastHeardNoisePosition`, and `HasLastHeardNoisePosition`. Combat sensing writes `IsInRange` and `CanAttack`. Second, the Brain section evaluates these values in priority order: Attack, Chase, Search, then Patrol. Third, the execution section uses the current `State` enum to run the matching action. This separation became important because earlier versions had action scripts fighting the state logic. The final rule is that senses write perception data, the Brain writes the state, and actions execute movement or attack behaviour.

The Blackboard acts as the main communication layer between the systems. Instead of allowing each action to hold private target data, important values are exposed and visible: `CanSeePlayer`, `HearsNoise`, `CurrentTarget`, `SearchTargetPosition`, `LastKnownPlayerPosition`, `CanAttack`, and `State`. This made debugging much easier because the reason for each behaviour could be seen directly during play. The live HUD displays the Tyrant’s current state, target, perception values, combat range values, and NavMesh destination. This was a major improvement over relying on code or console logs.

The most important CA3 integration change was moving away from disconnected legacy behaviours and rebuilding the AI as one pipeline. The final system follows a clear chain: perception updates Blackboard values, the Brain interprets those values, the state enum changes, and the NavMesh or attack action responds. This is visible in the final encounter: the Tyrant patrols normally, hears or remembers the player and searches, sees the player and chases, then enters Attack once close enough.

---

## Section 2 — Performance and Robustness Evaluation

Performance testing was carried out using the Unity Profiler during active gameplay states. Captures were taken during Patrol and during active engagement, including Chase and Attack. The evidence is stored in `Docs/ProfilerCaptures/`, with accompanying profiler notes. The captures were taken inside the Unity Editor, so the results include Editor, Game view, UI, and profiling overhead. For that reason, the screenshots were used as evidence of relative runtime behaviour rather than as final build benchmarking.

The most significant expected AI cost is perception. Vision is heavier than the other senses because it performs distance checks, field-of-view checks, and raycasts to confirm line of sight. Hearing and attack range checks are cheaper because they mainly rely on distance comparisons. NavMesh movement also contributes cost during Chase and Search because destinations are updated and the agent needs to maintain paths through the level.

One important performance-related issue appeared during development: when sensory actions returned `Success` and movement actions completed immediately, the Behaviour Graph could stop re-evaluating in the expected way. When patrol returned `Running`, the opposite problem occurred: the Tyrant could become locked in patrol and ignore updated senses. The final fix was to run sensory nodes continuously and keep the Brain/state logic active separately from the movement actions. This made the AI responsive while still allowing the NavMeshAgent to continue moving between ticks.

Several robustness cases were tested. First, the Tyrant had to react when the player entered vision during patrol. This now works because the vision node updates `CanSeePlayer`, the Brain sets the state to Chase, and the Chase action updates the NavMesh destination toward the player. Second, the Tyrant had to continue behaving correctly after losing sight. This is handled by storing `LastKnownPlayerPosition`; when vision is lost, the Brain can move into Search rather than immediately returning to Patrol. Third, attack handoff had to be made reliable. Chase originally continued too strongly, preventing Attack from taking over. The final version allows the Attack state to execute when `CanAttack` becomes true.

The current system is stable enough for the vertical slice, but it is not production-perfect. The debug overlay and repeated sensory nodes add overhead, but they are justified for CA3 because they make the AI readable and testable. In a production build, the overlay would be disabled and sensory checks would likely use a managed tick interval rather than running at full rate. For the assessment, the priority was demonstrable correctness, responsiveness, and clear evidence.

## Section 3: AI in Modern Engines and Beyond

This project was implemented in Unity rather than Unreal Engine, so my comparison with Unreal Engine 5 is based on research into its AI tools rather than direct development experience. In Unity, the Tyrant AI was built around a component-based workflow: a NavMeshAgent handled movement, repeated sensory actions updated Blackboard values, and a Behaviour Graph/state layer selected between Patrol, Search, Chase, and Attack. This suited the project because Unity allowed me to build the AI as a set of small C# actions that could be tested and replaced quickly. Unity’s AI Navigation package supports navigation meshes, dynamic obstacles, and links for traversal, while Unity Behavior provides graph-based authoring and Blackboard variables for storing decision data (Unity Technologies, n.d.).

The main strength of Unity for this project was flexibility. I was able to decide exactly how perception, state selection, and movement should communicate. However, that flexibility also created more responsibility. A lot of the integration had to be managed manually: the sensory nodes had to continuously update Blackboard values, the Brain section had to write the current State enum, and the execution layer had to respond without overwriting state decisions. This became one of the main lessons of the implementation. Unity gives enough control to build a readable AI pipeline, but it does not automatically prevent poor architecture or leftover legacy systems from interfering with new behaviour.

Unreal Engine 5 appears to provide a more opinionated AI workflow. Its Behaviour Tree and Blackboard system are tightly integrated, with the Blackboard acting as the data source that the tree uses to make decisions (Epic Games, n.d.). Unreal also includes tools such as Environment Query System (EQS), which can be used to choose context-aware positions in the world, for example selecting cover, patrol points, or investigation locations. From a design perspective, this means Unreal provides more built-in AI authoring conventions, while Unity often expects the developer to assemble their own structure from components, packages, and C# scripts. I did not implement the project in Unreal, so I cannot claim practical experience with these tools, but the documentation suggests that Unreal’s workflow is stronger for teams who want a standardised visual AI pipeline, while Unity is strong when a project benefits from custom, lightweight systems.

The ideas used in this Tyrant AI also connect to serious games and training simulations. In a commercial horror game, an enemy like the Tyrant is judged mainly by player experience: it should feel threatening, readable, and fair enough that the player understands why they were caught. In a military or industrial training context, similar AI techniques may be used, but the priorities are different. The AI must often be repeatable, explainable, and suitable for assessment. For example, serious games in military training can use simulated scenarios to let trainees practise recognition, decision-making, and teamwork without the risk and cost of real exercises (Holmes, n.d.). In that context, believable AI behaviour is useful, but it also needs to support training objectives and after-action review.

Industrial and safety training shows a similar pattern. Serious games and VR simulations are used to place trainees in hazardous scenarios without exposing them to real danger. Research into industrial safety serious games highlights their use for realistic interactive scenarios, immediate feedback, and improved decision-making around workplace hazards (El-Raoui et al., 2023). This changes the role of AI. In a normal game, AI can sometimes cheat or simplify behaviour to create drama. In training, that is more risky. If an AI opponent, hazard, or simulated worker behaves unrealistically, the trainee may learn the wrong lesson. As a result, serious-games AI needs stronger fidelity, clearer logging, and better justification than entertainment AI.

This changed how I think about my own implementation. The Tyrant does not need to be complex to be useful, but it does need to be readable. The debug overlay became important because it made the AI’s internal decision process visible: perception updates Blackboard values, the Brain selects a state, and the selected behaviour drives NavMesh movement or attack logic. That kind of visibility is valuable in assessment and would be even more important in serious games, where designers, instructors, and trainees may need to understand not just what the AI did, but why it did it.

References to keep in your bibliography: Unity AI Navigation, Unity Behavior Blackboard documentation, Unreal Engine Behaviour Tree/Blackboard documentation, Unreal EQS documentation, Holmes on military training serious games, and El-Raoui et al. on manufacturing safety serious games. Key source basis:

---

## Section 4 — Carry-Forward and Professional Reflection

If the project continued, the main technical improvement I would make would be a more disciplined tick-rate and sensing architecture. During CA3, the sensory nodes were changed to run continuously because this made the Tyrant responsive under deadline pressure. That was the right decision for the vertical slice, but a cleaner long-term version would use a central perception controller with explicit update intervals for vision, hearing, attack range, and debug reporting. This would preserve responsiveness while making performance easier to reason about.

The Unreal Engine and serious-games research also changed my understanding of AI structure. Before this module, I mostly thought of game AI as enemies choosing actions. Across CA1, CA2, and CA3, I started to see AI as a pipeline: sensing, memory, decision-making, movement, feedback, and evidence. Unreal’s Behaviour Tree, Blackboard, EQS, and StateTree tools show how valuable structured workflows can be. Serious-games examples show that AI is sometimes required to be explainable and reviewable, not just believable.

My own development across the module was uneven but useful. Early versions of the AI had too many leftover systems from other prototypes: low health, fleeing, damage sources, ability slots, and unclear target variables. These made the behaviour harder to debug. The strongest improvement in CA3 was cutting the system down to what the vertical slice actually needed. The final Tyrant is not the most complex AI I could build, but it is much clearer: Patrol, Search, Chase, and Attack are visible in the graph, visible in the Blackboard, and visible to the player through behaviour.

The main professional lesson is that scope control is not a weakness. A smaller AI system with clean evidence is stronger than a larger system that cannot be explained. For future AI work, I would start with the debug overlay and Blackboard structure earlier rather than adding them near the end. That would make the implementation easier to test and would produce better evidence throughout development.

---

## References

Checa, D. and Bustillo, A. (2020) ‘A review of immersive virtual reality serious games to enhance learning and training’, *Multimedia Tools and Applications*. Available at: https://link.springer.com/article/10.1007/s11042-019-08348-9 (Accessed: 14 May 2026).

Dodge, J. et al. (2021) ‘After-Action Review for AI (AAR/AI)’, *ACM Transactions on Interactive Intelligent Systems*. Available at: https://web.engr.oregonstate.edu/~burnett/Reprints/TIIS21_AARAI-accepted-preprint.pdf (Accessed: 14 May 2026).

El-Raoui, H. et al. (2024) ‘Design of a serious game for safety in manufacturing industry using hybrid simulation modelling’, *Winter Simulation Conference*. Available at: https://www.research.ed.ac.uk/en/publications/design-of-a-serious-game-for-safety-in-manufacturing-industry-usi/ (Accessed: 14 May 2026).

Epic Games (2026a) ‘Behavior Trees in Unreal Engine’. Available at: https://dev.epicgames.com/documentation/unreal-engine/behavior-trees-in-unreal-engine (Accessed: 14 May 2026).

Epic Games (2026b) ‘Behavior Tree in Unreal Engine — User Guide’. Available at: https://dev.epicgames.com/documentation/unreal-engine/behavior-tree-in-unreal-engine---user-guide (Accessed: 14 May 2026).

Epic Games (2026c) ‘Environment Query System Overview in Unreal Engine’. Available at: https://dev.epicgames.com/documentation/unreal-engine/environment-query-system-overview-in-unreal-engine (Accessed: 14 May 2026).

Epic Games (2026d) ‘Environment Query System User Guide in Unreal Engine’. Available at: https://dev.epicgames.com/documentation/unreal-engine/environment-query-system-user-guide-in-unreal-engine (Accessed: 14 May 2026).

Epic Games (2026e) ‘StateTree in Unreal Engine’. Available at: https://dev.epicgames.com/documentation/unreal-engine/state-tree-in-unreal-engine (Accessed: 14 May 2026).

Holmes, J. (n.d.) ‘Using Serious Games to Enhance Recognition of Combat Vehicles’, *NATO Science and Technology Organization Meeting Proceedings*. Available at: https://publications.sto.nato.int/publications/STO%20Meeting%20Proceedings/STO-MP-MSG-133/MP-MSG-133-04.pdf (Accessed: 14 May 2026).

Tzioutzios, D. et al. (2023) ‘Safety Hunting: a Serious Gaming Approach for Industrial Safety Training’, *Chemical Engineering Transactions*. Available at: https://www.cetjournal.it/cet/23/100/104.pdf (Accessed: 14 May 2026).

Unity Technologies (2026a) ‘AI Navigation’. Available at: https://docs.unity3d.com/6000.1/Documentation/Manual/com.unity.ai.navigation.html (Accessed: 14 May 2026).

Unity Technologies (2026b) ‘Behavior Graphs’. Available at: https://docs.unity3d.com/Packages/com.unity.behavior%401.0/manual/behavior-graph.html (Accessed: 14 May 2026).

Unity Technologies (2026c) ‘Create and manage variables and Blackboards’. Available at: https://docs.unity3d.com/Packages/com.unity.behavior%401.0/manual/blackboard-variables.html (Accessed: 14 May 2026).

Unity Technologies (2026d) ‘Unity Behavior Overview’. Available at: https://docs.unity3d.com/Packages/com.unity.behavior%401.0/manual/index.html (Accessed: 14 May 2026).