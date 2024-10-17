# Questions

## 2024-09-04

- Should the solution include the possibility of clicking the same square more than once?

## 2024-09-06

- Should it be possible to beat the level but in more clicks? 
	- If yes, should it give partial rewards? 
		- What are rewards, in this game? Going to the next level? 
		- When the reward is just being able to go to the next level, what could be a "partial reward"? 
			- Just the fact that the player knows they haven't beaten the level is the optimal amount of clicks?
	- If not, players shouldn't be able to click on more squares after clicks counter == solution clicks
		- Or, at least, players shouldn't be able to progress to the next level (as a reward) if they "busted" the goal amount of clicks to beat a level
- Also what if the player beats the level with fewer clicks than computed? 
	- Extra rewards? What are those?
		- Same thing as partial reward? The intrinsic satisfaction that you've "broken" the game?
	- If we don't want to address this, we should make sure to compute the actual optimal number of clicks to get to the goal instead of the dumb brute force algorithm we have now
	
- Should the click counter count down or up? Should it "leave a trace" (i.e. show the goal amount of clicks) and show what it's actually counting to/from? 

## 2024-10-10

- Should the order of clicks matter?
- Order the solutions to be displayed in descending order?
- Should I deactivate clicked squares? (that question keeps popping up since the beginning of the development of the	- But that means I can't have solutions which have the same square(s) clicked more than once in the future. Is that even something that we want?
- I'd like to move away from using numbers are some points. Maybe using collections of dots like on the faces of a die?

## 2024-10-17

- Is it possible to generate a solution that has another shorter solution as the first steps of it? Would this be a problem?
- Should the preview rectangles show the whole predicted array, not just the "changed squares"?
	- This would make it easier for players to predict their next move
	- Do we want to make it easier for players to predict their next move?