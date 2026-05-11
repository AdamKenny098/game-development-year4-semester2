# Evidence Pack

## Screenshots

Five screenshots are included demonstrating key Behaviour Tree states:

- Patrol
- Chase
- Death
- Search
- Flee

Each screenshot includes the debug overlay to show active behaviour, Blackboard values, and navigation information.

---

## Video

A video is included demonstrating the full encounter:

- Patrol behaviour
- Player detection → Chase
- Attack behaviour
- Loss of player → Search
- Search timeout → Patrol
- Low health → Flee
- Death state

The video shows interrupt handling and fail-soft behaviour in real time.

---

## Technical Note

See `TechNote.md` for a full explanation of the Behaviour Tree structure, perception system, and design decisions.

---

## Learning Journal

See:
- `LearningJournal/EvidenceLog.md`
- `LearningJournal/CriticalDiscussion.md`