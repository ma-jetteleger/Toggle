# Todo

## 2024-09-02

- ~~[feat] Make an array and display a bunch of squares~~
- ~~[feat] Make the squares interactable~~
	- ~~Make them clickable~~
	- ~~Make them toggle other squares when clicked~~
- ~~[feat] Assemble an algorithm to propose a "goal arrangement" of squares and a number of clicks to acheive it~~
- ~~[visual] Notify the player that they won if they acheive the goal arrangement of a level~~

## 2024-09-04

- ~~[feat/visual] Add a clicks counter (+ no more clicks remaining feedback + a feature switch to allow continue solving the puzzle)~~
- ~~[feat] Undo/redo/reset buttons~~ 
	- ? "order of clicks" indicators to help with the undo?
- ~~[feat] Next level button (debug is in place)~~

- [feat] Figure out preliminary heuristics for the generation of "interesting" levels and goals
	- ~~Don't generate "already solved" levels~~
- ~~[feat/visual] ? Telegraphed effect of clicking a button? Predicting its interaction with its targets?~~
- [tech] ? Debug button to show the solution? (implement this in player-facing interface?)

## 2024-09-06

- ~~[tech] Handle the animation interruptions so they don't break when spammed~~
- ~~[visuals] Change the arrows, they look ugly (away with the rounder corners)~~
	- ~~More importantly, change the "self targeting" graphic~~

## 2024-09-09

- ~~[feat] Find a way to let the player "play out" all the possible solutions solution of a level~~
	- ~~Handle both, and add a feature switch for this~~
	- ~~And generate levels that only have one solution in single solution mode~~
	- ~~And only generate levels with multiple solutions in multi-solution mode~~

## 2024-09-19

- ~~[feat] Brainstorm a list of "advanced"/"twisted" mechanics to be implemented in order to generate some fun dynamics for the game~~
- ~~[visual] Don't put useless arrows (a right arrow on the last square) (add a feature switch for that, it might not be a good idea, maybe the visual clutter is interesting, it could also be made unnecessary if implementing some of the features of the list)~~
- ~~[feat/visual] Experiment with longer (thinner) arrays~~

## 2024-09-25

- [visual] ? Add crosses and checkmarks to indicate that a square is in the right toggle state? (to make it easier to "read" a level)
- ~~[feat] Restrict squares to be clickable only once per level~~

## 2024-10-15

- ~~[feat] Organize levels (and the generation of levels) to follow some kind of progression curve~~
	- Test if this is enough to "teach" the game to first time players
- ~~[feat] Add "wrap around" toggle indicators~~
- ~~[visual] Change the placeholder UI arrows for the history buttons~~
- [feat] ? Make some squares always interactable? 
- [UI/feat] Add a menu with options to enable players/testers to test different modes and parameters 
- [visual] Mobile interface
	- Figure out how to convey the information that is conveyed when hovering squares without hover functionalities
	- Figure out how to maintain the visibility of the toggle indicators when using fingers to press on squares
- ~~[bug] Don't compile multiple solutions with the same amount of steps~~
- ~~[bug] Level generation algorithm is suggesting solutions that are impossible/broken~~

## 2024-10-18

- ~~[feat] Actually enforce multi-solutions/single-solution in their respective mode~~
- ~~[visual] Don't show the "level cleared" animation when re-inputing an already discovered solution~~
- ~~[feat] Involve more features in the progression (different targeting arrows, wrap around toggles)~~
	- ~~[tech] Make the progression be more easily editable, not just lines of code~~
- ~~[visual] Better (more prominently) display the clicks counter~~

## 2024-10-29

- ~~[bug] Solutions are generated that have the same amount of clicks as the level length in single-solution mode (or at least they're displayed as such)~~
	
## 2024-11-04

- [tech/feat] Save the progression + add a menu with the option to clear the saved progression
- ~~[bug] Prediction rectangles shake with the square when busting the clicks counter in single solution mode~~
- ~~[bug] SolutionClicksBoxes aren't destroyed sometimes...~~
- ~~[tech/feat] ? Rewrite the progression system to _force_ the introduction of some progression features on specific progression indices instead of _allowing_ their introduction?~~
- ~~[feat] Implement cascading toggles~~
- ~~[feat] Gradually introduce cascading toggles in the progression~~
	- ~~Maybe go back to shorter arrays to introduce cascading toggles and rebuild to longer arrays gradually~~
- [visual] ? Animate cascading toggles?
- ~~[visual] ? Highlight cascading toggles on hover?~~
- ~~[feat] Exclude squares that aren't toggled by others from being cascading~~
- ~~[tech] Write a function to re-order/cleanup the pregenerated levels files in ascending order of level lengths~~ 

## 2024-11-06

- ~~[bug] Next level button never hides on single solution mode~~
- [feat] Include number of solutions in the progression

## 2024-11-15

- [feat] ? Try solutions that have an amount of clicks that is different (fewer than) the amount of squares - 1?

## 2024-12-02

- ~~[feat] Include the basic targeting schemes (self-targeting and side-targeting) in the progression~~
- [visual] Try a less explicit feedback for the toggle predictions
- ~~[feat] Don't generate levels with all self targeting schemes~~
- ~~[tech] Don't save level to file if it already exists in file~~

## 2024-12-06

- [feat] Try and make sure that the new progression feature that's just being introduced is involved in the level's solution
- ~~[feat] Visual hint for cascaging toggle effect~~