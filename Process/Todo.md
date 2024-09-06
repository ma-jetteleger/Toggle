# Todo

## 2024-09-02

- ~~Make an array and display a bunch of squares~~
- ~~Make the squares interactable~~
	- ~~Make them clickable~~
	- ~~Make them toggle other squares when clicked~~
- ~~Assemble an algorithm to propose a "goal arrangement" of squares and a number of clicks to acheive it~~
- ~~Notify the player that they won if they acheive the goal arrangement of a level~~

## 2024-09-04

- ~~Add a clicks counter (+ no more clicks remaining feedback + a feature switch to allow continue solving the puzzle)~~
- ~~Undo/redo/reset buttons~~ 
	- ? "order of clicks" indicators to help with the undo?
- ~~Next level button (debug is in place)~~

- ? Debug button to show the solution? (implement this in player-facing interface?)
- ? Telegraphed effect of clicking a button? Predicting its interaction with its targets?
- ? Figure out preliminary heuristics for the generation of "interesting" levels and goals
	- Don't generate "already solved" levels
- ? Test whether or not it's more interesting to let players click squares more than once and if not, implement that restriction and feedback for that feature

## 2024-09-06

- [tech] Handle the animation interruptions so they don't break when spammed