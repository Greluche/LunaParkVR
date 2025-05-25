# LunaPark VR

A Virtual Reality Arcade Game built with Unity and C# for Oculus Quest 2.

## Table of Contents

- [Project Overview](#project-overview)
  - [Scenario](#scenario)
  - [Genre](#genre)
  - [Target audience](#target-audience)
- [Setup Instructions](#setup-instructions)
- [Mini Games](#mini-games)
  - [1. Bumping Cars](#1-bumping-cars)
  - [2. Archery](#2-archery)
  - [3. Fishing Ducks](#3-fishing-ducks)
  - [4. Claw Machine](#4-claw-machine)
  - [5. Haunted House](#5-haunted-house)

 
# Project Overview

LunaPark VR immerses players in an atmospheric funfair featuring interactive VR mini-games designed for Oculus Quest 2. Players navigate and solve challenges, such as the claw machine, archery, duck fishing bumping cars and the Haunted House to uncover the mysteries of the park and fins the way back home.

## Scenario 

One morning, you receive a very special invitation to the local funfair. Without anything better to do, you accept it and are transported across time and space to the greatest place in the world: Luna Park. If you wish to see your home again, you will have to get through the fair’s treacherous trials: duck fishing, archery challenge, bumping cars, and many more. Discover the park’s secrets and get thrown into a world of games and mysteries this summer in Luna Park VR, only on Oculus Quest.

## Genre

Arcade, escape game

## Target audience

Anyone who wants to get familiar with the mechanics of a VR game!

# Setup Instructions

## Requirements

- Unity 2022 or newer
- Oculus Quest 2 headset
- XR Interaction Toolkit package

## Installation Steps

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

# Mini Games 

## 1. Bumping Cars 

### Core Components
- **Steering Wheel** : SteeringWheel script, rigidbody, child sphere collider
- **Bumping Car** : Car control script, rigidbody, box collider, child XR rig + steering wheel
- **AI bumping car** : AI bumper car script, audio source, box collider, random sound player script

### How to use

You must destroy all other AI bumper cars as fast as possible

1. **Steering**:
   - Grab the wheel using the grip buttons to steer the bumper car
   - Wheel normalized angle defines bumper car rotation transform

2. **Movement**:
   - Primary button (right controller) will make the car gradually accelerate until a maximum speed
   - Secondary button (right controller) will make the car gradually accelerate in retromarch until a maximum speed

3. **Arena**
   - Colliding with an AI bumper car will instantly destroy it
   - Getting close to the arena boundaries will make the bumper car slow down gradually until it stops

### Scripts/Assets : 
- **Adapted**  : 
- **Created** : 
- **As-is** : 

## 2. Archery

### Core Components
- **Bow** : GrabInteractable, Rigidbody
- **String** : Child of bow, Linerenderer, midpoint (GrabInteractable)
- **Quiver** : SimpleInteractable, Spawns Arrow in bow
- **Arrow** : Rigibody, leaves trace when flying
- **Duck** : Explodes when hit by Arrow

### How to use
TO BE COMPLETED

### Scripts/Assets : 
- **Adapted**  : BowString.cs, ForwardIndicator.cs, Shoot_arrow.cs
- **Created** : Arrow.cs, BowIsGrabbed.cs, GrabBow.cs, MidPoint_phy.cs, RubberDuckArchery.cs, RubberDuckArcheryTutorial.cs,Score.cs, Target.cs, TumbleWeed.cs
- **As-is** : Elven Long Bow/Arrow, TumbleWeed, SM_Prop_Stall_Table_01, Explosion_Small_FX, RuberDuck, Cactus, Quiver

## 3. Fishing Ducks 

ARIEL COMPLETE

### Core Components

### How to use

### Scripts/Assets : 
- **Adapted**  : 
- **Created** : 
- **As-is** : 

## 4. Claw Machine

### Core Components
- **Claw** : Box collider (is Trigger), Claw script, XR socket interactor, GrabPoint child 
- **Joystick Manette** : RigidBody (kinematic), XRJoystickController script, Capsule collider
- **Button** : Box collider, ClawButton script, XR Poke Filter (on x), XR simple interactable
- **Toys** : Capsule colliders, ToyController script, RigidBody(Gravity), XR grab interactable
- **Win Table** : Box collider to block the toys

### How to use

You have 5 attemps to catch one of the two golden teddys

1. **Joystick Control**:
   - Grab the joystick directly with your VR controller, like you would do in real life
   - It controls the claw movement 
   - The joystick will automatically return to center when released

2. **Dropping the Claw**:
   - Position the claw over a prize
   - Press the drop button when you are satisfied with the position 
   - Get closer and poke gently the button or use a near/far grab interaction
   - The claw will automatically lower, attempt to grab a prize, and return to its initial position

3. **Prizes**:
   - Successfully grabbed prizes will be delivered to the drop zone and teleported to the win table
   - Players can grab prizes from the win table with their VR controllers

### Scripts/Assets : 
- **Adapted**  : JoystickV3.cs
- **Created** : ClawScript.cs, MachineUI.cs, ToyController.cs, ClawButton.cs
- **As-is** : None

## 5. Haunted House

### Core Components
- **Screamers**: box collider, screamer script
- **Button**: XR sample asset prefab
- **Bowl**: socket interactor, correct duck script, box collider
- **Ducks**: grab interactable, rigidbody, collider
- **Riddle Dialogue**: interactable buttons
- **Climbing Holds**: capsule collider, rigidbody, XR climb interactable (+ climb assistance teleport volume)

### How to use

1. **First challenge**:
   - Go through a corridor with a sequence of collider activated screamers
   - Poke the button at the end with the controller to enable the first climbing hold (plays a winning sound)
  
2. **Second challenge**
   - Select the correctly colored duck and put it in the bowl socket interactor
   - Putting a wrong colored duck will destroy it
   - Putting the correctly colored duck will enable the second climbing hold and play a winning sound

3. **Third challenge**
   - The duck God will face you and ask you a riddle through an interactable dialogue UI
   - Give the wrong answer and the duck God will try to scare you
   - Give the right answer to unlock the third and last climbing hold, again it'll play a winning sound

4. **Climbable pillar**
   - Once the three challenges complete, all the climbing holds should be activated
   - Climb the pillar using the grip button, you can help yourself by teleporting at the top
   - Escape the haunted house through the door at the end of the bridge
  
### Scripts/Assets : 
- **Adapted**  : 
- **Created** : 
- **As-is** :   


## Badges

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
