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

## 2. Archery

## 3. Fishing Ducks

## 4. Claw Machine

**Core Components**:
- **Claw** : Box collider (is Trigger), Claw script, XR socket interactor, GrabPoint child 
- **Joystick Manette** : RigidBody (kinematic), XRJoystickController script, Capsule collider
- **Button** : Box collider, ClawButton script, XR Poke Filter (on x), XR simple interactable
- **Toys** : Capsule colliders, ToyController script, RigidBody(Gravity), XR grab interactable
- **Win Table** : Box collider to block the toys

### How to Use

You have 5 attemps to catch one of the two golden teddys

1. **Joystick Control**:
   - Grab the joystick with your VR controller using the near-far interaction
   - It controls the claw movement (in unity you can enable movement on the x or z axis in the XRJoystickController script) 
   - The joystick will automatically return to center when released

2. **Dropping the Claw**:
   - Position the claw over a prize
   - Press the drop button when you are satisfied with the position
   - Get closer and poke the button, the claw will automatically lower, attempt to grab a prize, and return

3. **Prizes**:
   - Successfully grabbed prizes will be delivered to the drop zone and teleported to the win table
   - Players can grab prizes from the win table with their VR controllers


## 5. Haunted House


## Badges

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)
