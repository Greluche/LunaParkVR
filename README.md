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

For all references, refer to the report.

## Miscellaneous: hub
  
### Adapted

### Created
- RubberDuck.controller
- Arena.mat

### As-is 
- Ambient Sounds.mp3
- Nighttime Forest Sounds.mp3
- Quack.mp3

## Driving (bumping cars)

### Adapted

### Created
- HubBumperCar.anim
- BumperX.mat

### As-is
- Big Explosion.mp3
- Race Start Countdown.mp3

## Archery

### Adapted
- BowString.cs
- ForwardIndicator.cs
- Shoot_arrow.cs

### Created
- Arrow.cs
- BowIsGrabbed.cs
- GrabBow.cs
- MidPoint_phy.cs
- RubberDuckArchery.cs
- RubberDuckArcheryTutorial.cs
- Score.cs
- Target.cs
- TumbleWeed.cs
- ruber duck.controller
- 
### As-is
- Elven Long Bow/Arrow
- TumbleWeed
- SM_Prop_Stall_Table_01
- Explosion_Small_FX
- RuberDuck
- Cactus
- Quiver
- arrow.mp3
- arrow-twang.mp3
- explosion.mp3

## Fishing (ducks) 

### Adapted
- RiverBrush.brush
- Hook.fbx
- Ring4Hoop.fbx
- RubberDuck.fbx
- 
### Created
- Ground.mat
- Tutorial_Red.mat
- Water.mat
- DuckFishingGameManager.cs
- DuckFishingRod.cs
- FloatingDuck.cs
- TutorialButton.cs
- DuckTerrain.asset

### As-is
- Duck Scream.mp3
- Forest Water Stream.mp3
- LowPolyWater.cs

## Joystick (claw machine)

### Adapted
- JoystickV3.cs
- clawMachine prefab
  
### Created
- ClawScript.cs
- MachineUI.cs
- ToyController.cs
- ClawButton.cs

### As-is
- teleportation.mp3
- timeRunningOut.mp3
- clawmove.wav
- gameOver.wav
- applause.wav
- fabric_pattern_07.png
- gold-textured-background.jpg
- weathered_brown.jpg

## Miscellaneous: haunted house
  
### Adapted

### Created
- CloseDoor.anim
- Door.controller
- DuckGod.anim
- DuckGodReverse.anim
- HauntedDoor.anim
- ScreamerX.anim
- WrongAnswer.anim
- BackroomX.mat

### As-is
- FIRE SOUND EFFECT.mp3
- Free Horror Ambiance.mp3
- Mongolian Throat Singing.mp3
- ScreamerX.mp3
- WIN.mp3
- OldLondon SDF.asset

## Badges

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
