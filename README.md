# LunaPark VR

A Virtual Reality Arcade Game built with Unity and C# for Oculus Quest 2.

# Setup instructions

## Requirements

- Unity 2022 or newer
- Oculus Quest 2 headset
- XR Interaction Toolkit package

## Installation steps

1. Clone the Repository

<pre><code>
  git clone https://github.com/Greluche/LunaParkVR.git
  cd LunaParkVR
</code></pre>

2. Open Project in Unity
- Launch Unity Hub
- Open the project from the cloned directory

3. Install XR Interaction Toolkit

- Go to Window > Package Manager
- Find and install "XR Interaction Toolkit"

4. Configure Project for Oculus Quest 2
- Navigate to File > Build Settings
- Select Android platform and choose Oculus Quest as the run device
- Enable "Virtual Reality Supported" in Project Settings > XR Plug-in Management

# Scripts/assets for each custom feature

## Miscellaneous: Introduction scene
  
### Adapted

### Created
- TutorialProgress.cs

### As-is 
- [Apartment Kit](https://assetstore.unity.com/packages/3d/environments/apartment-kit-124055)
- [Home Interior Low Poly Pack](https://assetstore.unity.com/packages/3d/props/interior/pandazole-home-interior-low-poly-pack-203033)  
- [Woods Lifestyle Pack](https://assetstore.unity.com/packages/3d/environments/low-poly-woods-lifestyle-65306)

## Miscellaneous: Hub
  
### Adapted

### Created
- BlockingDialogue.cs
- BumperCarHighScore.cs
- CircularMotion.cs
- Dialogue.cs
- DialogueManager.cs
- FacePlayerNPC.cs
- FacePlayerUI.cs
- HighScoreManager.cs
- HubManager.cs
- HubPlayerSpawner.cs
- InteractButton.cs
- LoadBackrooms.cs
- NPCInteraction.cs
- PlayerInteract.cs
- PlayerSpawner.cs
- RandomSoundPlayer.cs
- SpawnPointManager.cs

### As-is 
- [Ambient Forest Sounds (YouTube)](https://www.youtube.com/watch?v=Q6DafqPSkNA&t=32s&ab_channel=BobPrivacy)
- [Quack.mp3](https://www.youtube.com/watch?v=Fw3RB7xnb80)
- [Free Hut Pack](https://assetstore.unity.com/packages/3d/props/free-hut-pack-130776)
- [NPC/Player Voice Pack (Male)](https://assetstore.unity.com/packages/audio/sound-fx/voices/effort-sounds-male-npc-player-audio-pack-285382)
- [Circus Music Album](https://assetstore.unity.com/packages/audio/music/circus-music-album-052818-119946)
- [Handpainted Grass & Ground Textures](https://assetstore.unity.com/packages/2d/textures-materials/nature/handpainted-grass-ground-textures-187634)
- [25+ Stylized Textures Pack](https://assetstore.unity.com/packages/2d/textures-materials/25-free-stylized-textures-grass-ground-floors-walls-more-241895)
- [Low Poly Park Pack](https://assetstore.unity.com/packages/3d/environments/urban/low-poly-park-pack-created-with-fastmesh-asset-292938)
- [POLYGON - Sampler Pack](https://assetstore.unity.com/packages/3d/environments/urban/low-poly-park-pack-created-with-fastmesh-asset-292938)

## Driving (bumping cars)

### Adapted
- [XRSteeringWheel.cs](https://gist.github.com/VRwithAndrew/ef21b23151c7efdace45efcc0341b005)
- [CameraReset.cs](https://www.youtube.com/watch?v=NOCXB_ETKrM&t=100s&ab_channel=ValemTutorials)

### Created
- HubBumperCar.anim
- AIBumperCar.cs
- BlinkingBumper.cs
- BlinkingPrimary.cs
- BumperBlinkControl.cs
- BumperCarGameManager.cs
- CarControl.cs
- RaceCountdownManager.cs

### As-is
- [Big Explosion.mp3]()
- [Race Start Countdown.mp3]()
- [BumperCar.fbx](https://sketchfab.com/3d-models/lunapark-bumper-cars-ffa24effe07e470cb0a207cc46b215a5)
- [Yughues Free Metal Materials](https://assetstore.unity.com/packages/p/yughues-free-metal-materials-12949)
- [AllSky Free Skybox Set](https://assetstore.unity.com/packages/2d/textures-materials/sky/allsky-free-10-sky-skybox-set-146014)
- [Free Engine Sound Pack](https://assetstore.unity.com/packages/audio/sound-fx/transportation/i6-german-free-engine-sound-pack-106037)
- [Comic Explosion Effect](https://assetstore.unity.com/packages/vfx/particles/fire-explosions/comic-explosion-effect-317348)

## Archery

### Adapted
- [BowString.cs](https://github.com/SunnyValleyStudio/VR-Archery-in-Unity-2022/blob/main/Vid%201-2/BowString.cs)
- [ForwardIndicator.cs]()
- [Shoot_arrow.cs]()

### Created
- ArcheryGameManager.cs
- ArcheryTutorialManager.cs
- Arrow.cs
- BowIsGrabbed.cs
- BowString.cs
- ForwardIndicator.cs
- GrabBow.cs
- MidPoint_phy.cs
- RubberDuckArchery.cs
- RubberDuckArcheryTutorial.cs
- Score.cs
- Shoot_arrow.cs
- Shooter.cs
- Target.cs
- TumbleWeed.cs
  
### As-is
- [Elven Long Bow/Arrow](https://assetstore.unity.com/packages/3d/props/weapons/elven-long-bow-fully-animated-18118)
- [TumbleWeed](https://sketchfab.com/3d-models/tumbleweed-e9fa341c64fe4626b5d5b0052b0c0b64)
- [Low Poly Simple Medieval Props](https://assetstore.unity.com/packages/3d/props/low-poly-simple-medieval-props-258397)
- [Cactus](https://assetstore.unity.com/packages/3d/vegetation/lowpoly-cactus-pack-291590)
- [Quiver](https://assetstore.unity.com/packages/3d/props/weapons/free-cartoon-weapon-pack-mobile-vr-23956)
- [Comic Explosion Effect](https://assetstore.unity.com/packages/vfx/particles/fire-explosions/comic-explosion-effect-317348)

## Fishing (ducks) 

### Adapted
- RiverBrush.brush (modified Unity built-in brush)
- [Hook.fbx](https://sketchfab.com/3d-models/hook-06c7515f62b64fb48c3546acad0a53b2)
- [Ring4Hoop.fbx](https://sketchfab.com/3d-models/ring-2fcf3f37074a409cbd8692b652bb96b5)
- [RubberDuck.fbx](https://sketchfab.com/3d-models/rubber-duck-6fac296036f64636a76324c60ec0f249)
- 
### Created
- DuckFishingGameManager.cs
- DuckFishingRod.cs
- FloatingDuck.cs
- TutorialButton.cs
- DuckTerrain.asset

### As-is
- [Duck Scream.mp3](https://pixabay.com/sound-effects/duck-quacking-type-1-293316/)
- [Forest Water Stream.mp3](https://www.chosic.com/download-audio/27957/)
- [LowPolyWater.cs](https://assetstore.unity.com/packages/tools/particles-effects/lowpoly-water-107563?aid=1100l7zKf)

## Joystick (claw machine)

### Adapted
- [Claw Machine.fbx](https://sketchfab.com/3d-models/claw-machine-1bb221027d914639907eb6ea7f1551af)
  
### Created
- ClawScript.cs
- MachineUI.cs
- ToyController.cs
- ClawButton.cs
- JoystickV3.cs

### As-is
- [teleportation.mp3](https://freesound.org/people/outroelison/sounds/150950/)
- [timeRunningOut.mp3](https://freesound.org/people/qubodup/sounds/211102/)
- [clawmove.wav](https://freesound.org/people/Audionauten/sounds/448375/)
- [gameOver.wav](https://freesound.org/people/tyeewhyee/sounds/527606/)
- [applause.wav](https://freesound.org/people/Littleboot/sounds/198089/)

## Miscellaneous: haunted house
  
### Adapted

### Created
- BackroomsManager.cs
- ButtonPush.cs
- ButtonRoom.cs
- ClimbingHandles.cs
- ClimbSocketTrigger.cs
- CloseDoor.cs
- DuckBowlJudge.cs
- DuckIdentity.cs
- EndingRoom.cs
- FinalRiddle.cs
- HandleToClimbable.cs
- RiddleDialogue.cs
- Screamer1.cs

### As-is
- [FIRE SOUND EFFECT.mp3]()
- [Free Horror Ambiance.mp3]()
- [Mongolian Throat Singing (YouTube)](https://www.youtube.com/watch?v=8V85MNUbd38&ab_channel=RoyaltyFreeSoundEffects)
- [ScreamerX.mp3]()
- [WIN.mp3]()
- [OldLondon SDF.asset](https://www.dafont.com/old-london.font)
- [Backrooms-like Asset Pack](https://assetstore.unity.com/packages/3d/environments/backrooms-like-asset-pack-254543)
- [Pile of Skulls (3D Model)](https://sketchfab.com/3d-models/pile-of-skulls-11d46d32494c44218a55192adc067e57)

## Badges

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
