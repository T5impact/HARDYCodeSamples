# Code Samples for H.A.R.D.Y.

H.A.R.D.Y. is a simple 2D platformer with physics-based movement and was created for a month-long game jam as my very first game.

Below are some code samples for some of the systems I designed and created.

Despite this being my first game, I still prioritized clean, clear, and structured code which I have maintained to this day.

---

## Introduction Script C#

This script handles the intro cutscene and implements a custom dialogue manager.

Designed to show dialogue one letter at a time based on a customizable rate.

Plays corresponding voice line when dialogue shows up.

Allows for easy skipping of the cutscene.


## Move Script C#

This script handles the player's movement.

Implements a physics-based movement system that allows acceleration, momentum, and a max velocity independent of vertical velocity.

Integrates with animation system and particle effects that scale with velocity.

Allows for a jump that has a cooldown.

## Pirate Script C#

This script is the controller for the pirate enemy and implements a patrol-based AI.

Designed a procedural patrol system that uses collision detection to determine when the enemy should move in the other direction.

Uses a line-of-sight detection system based on the dot product to determine when a player is in range.
