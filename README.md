# God Tower

A mobile climbing game developed in Unity as part of a technical assessment.

The player climbs a procedurally assembled tower by moving between handholds while progressing through five levels. The project also includes an HTTP webhook system that allows external events to interact with the player during gameplay.

The required `/bump` webhook triggers a full-screen boxing-glove event that knocks the player down the tower. The webhook architecture was also extended with additional configurable events for testing and demonstration purposes.

---

## Unity Version

- **Unity:** 6000.3.12f1 LTS
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Input:** Unity Input System
- **Target Platform:** Android

---

## Android Version

- **Minimum Android API Level:** Android 7.1 'Nouglat' (API level 25)
- **Target Android API Level:** Automatic (highest installed)
- **Architecture:** ARM64

The project is designed to run in portrait orientation.

---

## Controls

### Mobile

The game is controlled using swipe gestures.

- **Swipe Up** — Climb to a higher handhold
- **Swipe Left / Right** — Move around the tower to an adjacent handhold

The player automatically grabs valid handholds and transitions between climbing positions.

### Unity Editor

Mouse input can be used to simulate the same swipe gestures while testing in the Unity Editor.

- **Left Click** — Climb to a higher handhold
- **Left / Right Arow OR A / D Keys** — Move around the tower to an adjacent handhold

---

## Gameplay

The objective is to climb from the bottom of the tower to the summit.

The game contains **five levels**, with level configuration handled through `LevelConfig` ScriptableObjects.

When the player reaches the summit, they must remain there for **5 seconds** to complete the level.

External webhook events can interrupt the climb and move the player vertically. If the player is knocked away from the summit during the survival period, the countdown is cancelled and must be completed again.

---

## Architecture

The project separates networking, gameplay, presentation, level configuration, and progression responsibilities.

### Webhook Flow

```text
HTTP Request
     │
     ▼
BumpWebhookServer
     │
     │ Main-thread queue
     ▼
WebhookEventManager
     │
     ├───────────────┐
     ▼               ▼
ClimbingPlayer   WebhookPresentationController
                     │
              Event Presentation
              ├── Gloves
              ├── Car
              ├── Rockets
              └── Helping Hand
```

### Main Components

#### `ClimbingPlayer`

Owns the player's climbing state machine, handhold movement, falling, recovery, summit detection, and webhook-driven vertical movement.

The main gameplay states are:

```text
Climbing
Summit
Won
Hit
Falling
Recovering
```

#### `TowerHandholdGenerator`

Generates and manages the available climbing handholds around the tower and provides handhold queries used by the climbing system.

#### `LevelManager`

Coordinates the active level and configures the tower, player, HUD, environment, and win state using level-specific data.

#### `LevelConfig`

A ScriptableObject containing configurable level data, allowing the five levels to share the same gameplay systems while using different settings.

#### `BumpWebhookServer`

Runs a lightweight local HTTP listener on port `56789`.

Network requests are received outside Unity's main thread and gameplay actions are queued back onto the Unity thread before interacting with game systems.

#### `WebhookEventManager`

Coordinates webhook events between gameplay and presentation.

It controls event timing and requests the corresponding force from `ClimbingPlayer`.

#### `WebhookEventConfig`

A ScriptableObject describing a webhook event, including:

- Route
- Event name
- Vertical force
- Movement duration
- Presentation type
- Sound effect
- Presentation/impact timing
- Flash and camera-shake settings

This allows webhook behavior to be configured without hard-coding individual routes into the gameplay system.

#### `WebhookPresentationController`

Coordinates shared webhook feedback such as:

- Sound
- Screen flash
- Camera shake
- Event-specific presentation

Individual presentation components handle the animations for gloves, cars, rockets, and the helping hand.

---

## Webhook

The webhook server listens on:

```text
Port: 56789
```

The required endpoint is:

```text
/bump
```

Both `GET` and `POST` requests are accepted.

The `/bump` event displays the boxing-glove presentation and knocks the player downward.

Webhook gameplay is ignored when the player has already completed the level.

---

## Testing with cURL

While the game is running in the Unity Editor:

```bash
curl http://localhost:56789/bump
```

On Windows PowerShell, `curl.exe` can be used explicitly:

```powershell
curl.exe http://localhost:56789/bump
```

A successful request returns an HTTP `200` response.

### Additional Webhook Events

The webhook system was generalized using `WebhookEventConfig` ScriptableObjects. Three additional events are included to demonstrate the extensibility of the system:

```text
/car
/rockets
/hands
```

They can be tested with:

```powershell
curl.exe http://localhost:56789/car
curl.exe http://localhost:56789/rockets
curl.exe http://localhost:56789/hands
```

These additional events reuse the same networking and gameplay pipeline as `/bump`.

---

## Android Webhook Testing

Because the webhook server runs inside the Android application, ADB can be used to expose the device's port to the development computer.

Connect the Android device through USB and confirm that it is detected:

```bash
adb devices
```

Then forward port `56789`:

```bash
adb forward tcp:56789 tcp:56789
```

With the game running on the connected device, the webhook can then be triggered from the computer using:

```bash
curl http://localhost:56789/bump
```

Additional events can be triggered in the same way:

```bash
curl http://localhost:56789/car
curl http://localhost:56789/rockets
curl http://localhost:56789/hands
```

To inspect active forward-port mappings:

```bash
adb forward --list
```

---

## Asset Sources

The project uses a combination of provided, free, and/or externally sourced assets.

### Character

- **Asset:** Amane Kisora-chan(FREE ver)
- **Source:** [Unity Asset Store link](https://assetstore.unity.com/packages/3d/characters/amane-kisora-chan-free-ver-70581)
- **License:** Standard Unity Asset Store EULA

### Tower / Environment

- **Asset:** Low Poly Trim Sheet Asset Collection
- **Source:** [Unity Asset Store link](https://assetstore.unity.com/packages/3d/props/low-poly-trim-sheet-asset-collection-246335)
- **License:** Standard Unity Asset Store EULA

### Sky / Background

- **Asset:** Midgard Skybox Prime
- **Source:** [Unity Asset Store link](https://assetstore.unity.com/packages/2d/textures-materials/sky/midgard-skybox-prime-394578)
- **License:** Standard Unity Asset Store EULA

### Audio

- **Asset(s):**
    - freesound_community-woosh_northern87-91714.mp3
    - floraphonic-automobile-honk-2-170424.mp3
    - dragon-studio-angelical-synth-pad-463203.mp3
    - alex_jauk-firework-whistle-190306.mp3
- **Source:** [Pixabay link](https://pixabay.com/es/)
- **License:** [Pixabay Content License](https://pixabay.com/service/license-summary/)

### Webhook Presentation Assets

Assets used for the boxing gloves, car, rockets, and helping-hand presentations:

- **Boxing Gloves:** [Link](https://gorillawearcom.myshopify.com/products/mosby-boxing-gloves-black?shpxid=774de1f3-9b68-46fc-a93b-bc8e24099129)
- **Car:** [Link](https://pngtree.com/so/car-view/3)
- **Rockets:** [Link](https://www.vexels.com/png-svg/preview/194637/striped-firework-rocket-element)
- **Helping Hand:** [Link](https://gallery.yopriceville.com/Free-Clipart-Pictures/Hands-PNG/Hand_PNG_Clipart_Picture)

There was no AI-generated assets used in this project.
---

## Licenses

All third-party assets remain subject to their respective licenses.

Assets included with this submission are used only under licenses that permit their use within the project.

For externally sourced assets, see the **Asset Sources** section above for attribution and licensing information.

[If license files were supplied with individual asset packages, mention their location here.]

---

## Assumptions and Design Decisions

### Reference Matching

The project prioritizes matching the supplied reference in:

- Vertical level composition
- Character-to-tower proportions
- Camera framing
- Level progression
- Climbing pacing
- HUD presentation
- Summit countdown
- Webhook feedback

Where exact source assets were unavailable, visually similar free or generated assets were used while preserving the reference's overall composition and gameplay readability.

### Climbing

Climbing is handhold-based rather than free movement.

The player selects valid nearby handholds based on their current position and movement direction. This keeps player movement predictable while maintaining the vertical climbing flow shown in the reference.

### Summit

Reaching the highest climbing area does not immediately complete the level.

The player must survive at the summit for **5 seconds** before the level is considered complete.

A negative webhook event during this period cancels the summit state and knocks the player back down.

### Webhook Architecture

Although `/bump` is the required webhook, the implementation was intentionally made data-driven rather than hard-coding a single event.

Webhook routes are represented by `WebhookEventConfig` ScriptableObjects and share the same networking, gameplay, and presentation pipeline.

The additional `/car`, `/rockets`, and `/hands` events demonstrate this architecture without changing the core HTTP server.

### Networking

The webhook implementation is intended for local testing and technical-assessment use.

The server provides the HTTP functionality required by the assignment but is not intended to replace a production web server or public-facing networking backend.

---

## Build and Delivery

The submitted Android APK should install and run directly on a compatible Android device.

For webhook testing on a physical device:

1. Install and launch the APK.
2. Connect the device through ADB.
3. Run:

```bash
adb forward tcp:56789 tcp:56789
```

4. Trigger the webhook:

```bash
curl http://localhost:56789/bump
```

---

## Project Structure

The main project scripts are organized by responsibility, including:

```text
Scripts/
├── Gameplay/
├── Level/
├── Networking/
└── UI/
```

Configuration data is stored in ScriptableObjects where appropriate, while reusable presentation elements are separated from core gameplay logic.

---

## Notes

The project was developed specifically for this technical assessment, with emphasis on:

- Visual fidelity to the supplied reference
- Responsive climbing controls
- Clear gameplay feedback
- Robust webhook handling
- Mobile performance
- Maintainable Unity architecture