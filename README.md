# Parking Panic

## Overview
The goal of the game is to navigate through various city levels, adhering to traffic rules, avoiding pedestrians and obstacles, and successfully parking your vehicle in a designated spot at the end of each level. The game features detailed driving physics, weather effects, and five progressively difficult levels.

## Team Members & Contributions

### Dariusz Krych
**Role:** Menu and Sound System Programmer & Designer
-   **Menus:** Created all menus in the game including the start, main, credits, audio, video and pause menues.
-   **Audio:** Created the audio player system which is used for all music and SFX. Where the music, SFX and main volumes can be adjusted separately with persistently saved settings.
-   **Video:** Persistently saves the display mode, resolution, framerate and AA chosen by the user.
-   **Pause:** Pauses physics, has audio settings, Retry, Resume and Quit buttons.
-   **Other:** Contributed to the design of various levels such, the enlarging sign feature, car sound, music for levels and the team logo design.

### Jacob Fenech
**Role:** Core Gameplay Programmer & Designer
-   **Car Throttle:** Variable torque application dependent on the gears, braking with (s) in first gear if vel<0 applies negative torque for reversing.
-   **Car Steering:** Input damping, velocity-dependent rotation and lateral friction.
-   **Car Damage:** A visual system which swaps the car sprite to a more damaged one upon first and second impact.
-   **AI:** Created vehicles and pedestrians for all levels using weighted graph traversal.
-   **Other:** Contributed to the design of various levels, proximity to player based AI vehicle honking logic and sound.

### Lorenzo Iabichella
**Role:** Core Gameplay Programmer & Designer
-   **User Interface:** Logic and design for the marking board, pass screen, fail screen. Along with accompanying SFX.
-   **Marking System:** Where player fails if too many marks are collected for mistakes such as detected collisions or traffic rule violations with minor and major infractions.
-   **Cameras:** Logic for the camera following the car and to a pre-set path along each level to pre-view it before with a skip option.
-   **Parking Detection:** Detects when the users vehicle is within the parking area for over 2 seconds with a visual guide and SFX.
-   **Game Logic:** Contributed to the design of various levels and fog at the edge of levels.

### Isaac Piscopo
**Role:** Dedicated Artist
-   **Level Design:** Did the vast majority of the design work on all 5 tile based levels.
-   **Asset Design:** Sourcing/Designing the vast majority of sprites used across the game.

## Game Features
-   **Driving Mechanics:** Acceleration, braking, steering, shifting gears 1-4, reversing.
-   **Traffic Rules:** Players must obey speed limits and traffic signs such as direction signs.
-   **Dynamic Obstacles:** Pedestrians and vehicles use AI specifically using weighted graph traversal logic.
-   **Weather System:** Rain and snow effects that impact car physics.
-   **Settings:** Comprehensive audio and video settings customization.

## Technical Details
-   **Engine:** Unity
-   **Duration:** 48 Hours (Game Jam)
-   **Platform:** PC (Linux/Windows/Mac)
