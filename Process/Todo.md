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

- Figure out preliminary heuristics for the generation of "interesting" levels and goals
	- ~~Don't generate "already solved" levels~~
- ~~? Telegraphed effect of clicking a button? Predicting its interaction with its targets?~~
- ? Debug button to show the solution? (implement this in player-facing interface?)

## 2024-09-06

- ~~[tech] Handle the animation interruptions so they don't break when spammed~~
- ~~[visuals] Change the arrows, they look ugly (away with the rounder corners)~~
	- ~~More importantly, change the "self targeting" graphic~~

## 2024-09-09

- ~~Find a way to let the player "play out" all the possible solutions solution of a level~~
	- ~~Handle both, and add a feature switch for this~~
	- ~~And generate levels that only have one solution in single solution mode~~
	- ~~And only generate levels with multiple solutions in multi-solution mode~~

## 2024-09-19

- ~~Brainstorm a list of "advanced"/"twisted" mechanics to be implemented in order to generate some fun dynamics for the game~~
- ~~Don't put useless arrows (a right arrow on the last square) (add a feature switch for that, it might not be a good idea, maybe the visual clutter is interesting, it could also be made unnecessary if implementing some of the features of the list)~~
- ~~Experiment with longer (thinner) arrays~~

## 2024-09-25

- ? Add crosses and checkmarks to indicate that a square is in the right toggle state? (to make it easier to "read" a level)
- ~~Restrict squares to be clickable only once per level~~

## 2024-10-15

- Better (more prominently) display the clicks counter
- Organize levels (and the generation of levels) to follow some kind of progression curve
	- Eventually: test if this is enough to "teach" the game to first time players
- ~~Add "wrap around" toggle indicators~~
- ~~Change the placeholder UI arrows for the history buttons~~
- ? Make some squares always interactable? 
	- I want to test the "feeling" of this, but it might just bring back the multiple solutions problem so.. eh..
- Add a menu with options to enable players/testers to test different modes and parameters 