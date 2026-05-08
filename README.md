# CA3 - Outpost Capture

Advanced 3D Game Development - CA3 Vertical Slice  
Student: Adam Kenny (20102588)  
Project: Outpost Capture  
Engine: Unity 6  
Networking: Photon Fusion 2, Shared Mode  
Target Platform: Windows PC

---

## 1. Project Overview

Outpost Capture is a small multiplayer 3D vertical slice built for CA3. The project focuses on a compact capture-the-point game mode with two networked players, one neutral NPC guard, authenticated session access, profiling evidence, and a final rendering/post-processing polish pass.

The goal of the vertical slice is not to build a full commercial multiplayer game. Instead, the project demonstrates a complete, testable, and evidence-backed gameplay loop that satisfies the CA3 requirements:

- A playable 3D vertical slice.
- Photon Fusion 2 Shared Mode networking.
- A meaningful networked gameplay feature.
- A NavMesh-based NPC that affects the gameplay loop.
- Unity Authentication before Fusion session startup.
- Azure-hosted authentication proxy.
- Photon/Fusion Custom Authentication configured to reject unauthenticated or invalid clients.
- Profiling before/after evidence.
- Rendering, lighting, and post-processing polish consistent with the CA1 rendering foundations.

---

## 2. Gameplay Loop

The game mode is a multiplayer capture-the-point scenario.

1. Blue and Red players join the session.
2. Each player spawns at their team spawn area.
3. Players move toward the central capture point.
4. Standing on the point captures it for that team.
5. If both teams are present, the point becomes contested.
6. A neutral guard patrols the arena.
7. The guard contests the point and prevents capture progress while present.
8. If the guard catches a player, that player is sent back to their team spawn.
9. Once a team owns the point, their score increases over time.
10. The first team to reach full score wins.

This creates a simple but complete competitive loop: move, contest, capture, avoid the guard, and control the point long enough to win.

---

## 3. Controls

| Input | Action |
|---|---|
| WASD | Move player relative to camera direction |
| Right Mouse + Mouse Movement | Orbit camera around the arena |
| Mouse Scroll | Zoom camera in/out |
| Stand on Point | Capture objective |
| Guard Contact | Player returns to team spawn |

---

## 4. Core Features Implemented

### Networked Gameplay

- Photon Fusion 2 Shared Mode session startup.
- Networked Blue and Red player spawning.
- Synced player movement.
- Camera-relative player movement.
- Networked capture point state.
- Networked team ownership, capture progress, score, and contested state.
- Networked neutral guard.

### NPC Behaviour

The neutral guard is governed by a NavMesh-based behaviour system. It can:

- Patrol between scene-defined points.
- Detect and chase players.
- Contest the capture point.
- Prevent capture progress while inside the objective zone.
- Send caught players back to their team spawn.

The guard acts as a third neutral party rather than belonging to either team.

### Authentication and Access Control

The project implements the CA3 authentication pipeline:

1. Unity Services initializes.
2. Unity Authentication signs the client in anonymously.
3. Fusion `NetworkRunner.StartGame` is only called after successful Unity Authentication.
4. The Unity Authentication access token is passed through Photon Custom Authentication.
5. Photon calls an Azure-hosted authentication proxy.
6. The Azure proxy validates the Unity token server-side.
7. Valid clients are accepted.
8. Invalid or missing tokens are rejected at the Photon/Fusion connection stage.

Evidence for the valid and invalid authentication paths is included in the `Docs/Screenshots/` folder and discussed in the reflective PDF.

### Rendering and Presentation

The final vertical slice uses a controlled blockout art style with a focused presentation polish pass:

- Perspective 3D recording camera.
- Orbit camera controls for presentation.
- Team-coloured spawn materials.
- Warm capture-point material and lighting.
- URP Global Volume post-processing.
- Bloom, vignette, color adjustment, and film grain.
- Improved lighting pass.
- Clean bottom-screen controls HUD.

The rendering pass is designed to support the CA1 rendering continuity requirement while keeping the scope controlled.

---

## 5. Build and Run Instructions

### Requirements

- Unity 6.
- Photon Fusion 2 package installed and configured.
- Unity Authentication package installed.
- Universal Render Pipeline configured.
- Internet connection for Unity Authentication, Photon, and Azure authentication proxy.

### Running in Unity

1. Open the Unity project.
2. Open the CA3 vertical slice scene under `Assets/Scenes/02_VerticalSlice/`.
3. Confirm `FusionBootstrap` is present in the scene.
4. Confirm `Use Photon Custom Auth` is enabled.
5. Confirm `Simulate Invalid Token For Rejection Test` is disabled for normal gameplay.
6. Press Play.
7. The client should authenticate, connect to Fusion, spawn a player, and begin gameplay.

### Running Two Clients

To test the networked feature, run two clients using the Unity Editor plus a build, or two separate builds if available.

Expected result:

- First player joins as Blue.
- Second player joins as Red.
- Both players move and sync across the session.
- Capture progress, score, guard contesting, and respawn behaviour are visible to both clients.

---

## 6. Authentication Test Instructions

### Valid Token Test

1. Open the scene.
2. Ensure `Simulate Invalid Token For Rejection Test` is disabled.
3. Press Play.
4. The console should show Unity Authentication sign-in success.
5. Fusion should start after authentication.
6. Azure logs should show the Unity player token being accepted.

### Invalid Token / Rejection Test

1. Enable `Simulate Invalid Token For Rejection Test` on `FusionBootstrap`.
2. Press Play.
3. Fusion should fail to start with a custom authentication failure.
4. Azure logs should show token validation failure.
5. Disable the rejection-test toggle immediately after capturing evidence.

This proves that invalid clients are rejected at the Photon/Fusion authentication stage rather than merely blocked by UI.

---

## 7. Profiling Evidence

Profiling evidence is stored in:

```text
Docs/CA3_ProfilingPack/
```

The profiling pack includes:

- `before_screenshot.png`
- `after_screenshot.png`
- `ProfilingNotes.md`

The optimisation pass focused on a small, evidence-based rendering improvement:

- Static level geometry handling.
- Reduced unnecessary real-time lighting/shadow cost.
- Test-only objects kept disabled during final runtime tests.

The after capture showed a modest improvement in CPU/GPU frame time while preserving the same gameplay loop and visual readability.

---

## 8. Evidence Included

The `Docs/Screenshots/` folder contains evidence screenshots for:

- Photon Custom Authentication dashboard.
- Azure proxy success and rejection logs.
- Unity authentication success console.
- Fusion rejection console.
- Two-client gameplay.
- Networked capture point behaviour.
- Guard contesting and respawn behaviour.
- Rendering/post-processing polish.
- Profiling before/after captures.
- Git commit graph and tag evidence.

These screenshots are referenced in the reflective PDF and/or the video submission.

---

## 9. Version Control

Development was carried out using small, focused sprints. Each sprint targeted a specific part of the final vertical slice, such as:

- Map generation and layout repair.
- Local capture point logic.
- Neutral guard behaviour.
- Fusion setup.
- Networked player spawning.
- Networked movement.
- Networked capture state.
- Guard synchronisation.
- Unity Authentication.
- Azure proxy and Photon Custom Authentication.
- Profiling pack.
- Rendering and presentation polish.

This approach kept the project stable and allowed each working checkpoint to be committed before moving on to the next feature.

Important milestone tags:

```text
ca3-profiling-pack
ca3-submit
```

The final submission state is marked with:

```text
ca3-submit
```

---

## 10. Submission Contents

Final CA3 submission includes:

- Unity project repository.
- Final playable build.
- CA3 reflective PDF.
- Video submission or video link.
- Profiling evidence under `Docs/CA3_ProfilingPack/`.
- Supporting screenshots under `Docs/Screenshots/`.
- Attribution / credits information.
- AI usage statement included in the reflective PDF.
- Final Git tag: `ca3-submit`.

---

## 11. Attribution and AI Usage

Third-party services and packages used include Unity, Unity Authentication, Universal Render Pipeline, TextMeshPro, Photon Fusion 2, Azure Functions, and Google Slides / Apps Script for slide generation.

The AI usage statement is included in the CA3 reflective PDF. AI assistance was used for planning, debugging support, code drafting, documentation structure, and wording refinement. Final implementation decisions, testing, integration, evidence capture, and submission remain the student's own work.

---

## 12. Known Limitations

This is a focused vertical slice rather than a full multiplayer game. Known limitations include:

- The project uses Photon Fusion Shared Mode rather than a dedicated authoritative server.
- Player movement and capture mechanics are intentionally simple.
- The guard AI is functional but minimal compared to a production behaviour tree or animation-driven AI system.
- The environment uses a controlled blockout art style rather than a fully detailed production asset set.
- Profiling was performed at vertical-slice scope, with a modest optimisation rather than a full production optimisation pass.

These limitations are intentional trade-offs made to keep the CA3 submission stable, understandable, and evidence-focused.

---

## 13. Final Notes

Outpost Capture demonstrates a complete CA3-ready multiplayer vertical slice: authenticated entry, networked gameplay, NPC involvement, rendering polish, profiling evidence, and a clean submission workflow. The project prioritises correctness, evidence, and controlled scope over unnecessary feature expansion.
