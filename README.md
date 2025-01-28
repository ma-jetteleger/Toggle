# Toggle

Toggle is a game about the simplest form of interaction: selecting and activating something. It's a game about rules and interactions, or a game about pressing buttons.

This repository contains the code of the game and the process of its development.

## How to play:

The goal of the game is to change the colors of the middle interactive set of squares so they match the other array of squares underneath in the specified number of clicks shown at the bottom of the screen.

- You can sometimes complete a level in fewer clicks, but the real challenge resides in completing it in that exact number of clicks.

Squares can be in two states, represented by two different colors: black and white. Click on squares to toggle other squares (and/or themselves) between these two states.

- A diamond shape indicates that the squares targets itself to be toggled when clicked. 
- Left and right arrows indicate that the square targets its immediate neighbour(s). 
- Multiple shapes indicate that the square has multiple toggle targets which will all be toggled simultaneously when clicked.
- A dot on a square indicates that, when it is targeted and toggled by another clicked square, it will toggle its own target(s) subsequently as well

The buttons at the top-left side of the screen only feature placeholder sprites:

- The diamond button resets the level
- The left arrow button is an "undo" button
- The right arrow button is a "redo" button

A simple animation plays when you complete a level. Click the right arrow button that appears on the right side of the screen to generate a new level.