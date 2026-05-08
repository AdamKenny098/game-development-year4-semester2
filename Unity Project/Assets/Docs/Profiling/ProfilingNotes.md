# CA3 Profiling Notes

## What was measured

I profiled the Outpost Capture vertical slice using the Unity Profiler while the Photon Fusion session was running. The scene included the networked player, networked capture point, networked neutral guard, UI, lighting, and generated blockout level.

The profiling pass focused on CPU/GPU frame time and rendering cost because the level is built from many blockout objects and several runtime lights.

## Before optimisation

The before capture was taken during normal gameplay with the networked systems active. The Profiler showed a representative frame of approximately:

- CPU: 12.67 ms
- GPU: 10.15 ms
- Batches: 428
- SetPass Calls: 42
- Triangles: 9.84k
- Vertices: 20.84k
- Total Used Memory: approximately 504 MB

Screenshot: `before_screenshot.png`

## Change made

I made a small rendering-focused optimisation pass. The static level blockout geometry was marked as static so Unity could treat the non-moving floor, wall, and cover objects more efficiently. I also checked the scene lighting and ensured non-essential real-time shadow cost was reduced where it did not affect gameplay readability. Test-only local objects were kept disabled so they did not add unnecessary runtime behaviour during the final vertical slice.

## After optimisation

The after capture was taken under similar gameplay conditions with the same networked capture point, player, and guard active. The Profiler showed a representative frame of approximately:

- CPU: 10.60 ms
- GPU: 9.68 ms
- SetPass Calls: 42
- Triangles: 9.83k
- Vertices: 20.82k
- Total Used Memory: approximately 0.55 GB

Screenshot: `after_screenshot.png`

## Outcome

The optimisation produced a small but measurable improvement in CPU and GPU frame time while keeping the gameplay loop and visual readability intact. The geometry count remained effectively the same because the level layout itself was not reduced; the optimisation focused on how static scene content and avoidable runtime cost were handled.

## Reflection

This was a deliberately modest, evidence-based optimisation. The vertical slice is small, so the goal was not to aggressively reduce visual quality or redesign the level, but to demonstrate that the scene had been measured and improved based on profiling evidence. In a larger production project, I would continue this process with build-based profiling, deeper GPU analysis, occlusion checks, LOD planning, batching analysis, and platform-specific testing outside the Unity Editor.