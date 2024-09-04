# Journal

## 2024-09-02

Today we start prototyping the game's concept! The idea is to present a bunch of squares to the player on a linear one dimensional array. When clicked, these squares will toggle a state change in themselves and/or other squares in the array. 

I haven't decided how these connections (between square clicking and square toggling) will be configured and how they will be displayed yet.

The goal for the player will be to have the squares match a "goal array" that show all the squares in the specific state they should be for the player to complete the level.

There will also be a total number of clicks for each specific level.

I'm thinking of generating those level procedurally first, as it'll make it way easier to iterate on different layouts and see if the idea is any interesting.

Let's do this

---


Should find  better way to indicate a self target than a "down" arrow

Done:
	- Initializing levels so that squares's target scheme are fully random
	- And their initial "toggle" state is random as well
	- And the number of squares is random within a range
	- And the solution is also random (the number of clicks for the solution is random within a range)
		- Should the solution include the possibility of clicking the same square more than once?
		 This is done because I have no idea of heuristics that indicate whether some aspect or a level's configuration and its solution make it more interesting than another, those will have to wait
	
Next todos:
	- Clicks counter (should it be possible to beat the level but in more clicks? Giving partial rewards? What are rewards? When the reward is just being able to go to the next level, what could be a "partial reward"? If not, shouldn't be able to click on more squares after clicks counter == solution clicks) (also what if the solution is possble to achieve with more clicks? Should compute the optimal number of clicks to get to a randomly setup solution)
	- Undo/reset buttons
	- Next level button (debug is in place)
	- Debug button to show the solution? (implement this in player-facing interface?)
	- Telegraphed effect of clicking a button? Predicting its interaction with its targets?
	
After just having prototyped the core mechanics and core loop, already tons of (new) questions! Exciting!

Totally unplayable by someone who doesn't know the rules and the goal of the game though. That's kind of a consequence of minimalism, there is no external reference to anything that could inform the user of what to do and what to do it for, I have to create those affordances, signifiers and visual mappings myself. The affordances I'll need to make obvious:
	- Squares are clickable
	- Squares have two possible states
	- Squares can switch between these states of being through clicking squares

I'll need to make those affordances perceivable with the help of "knowledge in the world" (the game world, that is), in the form of feedback, feedforward and visual signifiers displayed on the elements of game's interface, or "knowledge in the head", in the form of a tutorial, formalizing the core ideas and mechanisms of the game in the head of the player. A challenge with the tutorial approach is that there is no guarantee that players will actually remember any of it if they come back and play again. Also there's something very much not minimalistic about a written out tutorial. My first attempt at "teaching" the game will therefore be through the design of perceivable minimalistic signifiers.
	
	