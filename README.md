# Toggle

Toggle is a game about the simplest form of interaction: selecting and activating something. It's a game about rules and interactions, or a game about pressing buttons.

This repository contains the code of the game and the process of its development.

## How to play:

The goal of the game is to change the colors of the middle interactive set of squares so they match the color of the ones at the bottom in a specified number of clicks.

Squares can be in two states, represented by two different colors. Click on squares to toggle other squares (and themselves) between these two states.

- A "down" arrow indicates that the squares targets itself to be toggled when clicked. 
- Left and right arrows mean that the square targets its immediate neighbour(s). 
- Multiple arrows mean that the square has multiple toggle targets which will all be toggled when clicked.