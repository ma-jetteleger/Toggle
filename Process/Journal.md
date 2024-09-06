# Journal

## 2024-09-02

Today we start prototyping the game's concept! The idea is to present a bunch of squares to the player on a linear one dimensional array. When clicked, these squares will toggle a state change in themselves and/or other squares in the array. 

I haven't decided how these connections (between square clicking and square toggling) will be configured and how they will be displayed yet.

The goal for the player will be to have the squares match a "goal array" that show all the squares in the specific state they should be for the player to complete the level.

There will also be a total number of clicks for each specific level.

I'm thinking of generating those level procedurally first, as it'll make it way easier to iterate on different layouts and see if the idea is any interesting.

Let's do this

## 2024-09-04

After just having prototyped the core mechanics and core loop, already tons of (new) questions! Exciting!

The addition of a goal makes the core mechanic a little bit more interesting. But something is clearly missing. The lack of feedback and feedforward to make informed decision, maybe. And/or the lack of surprise or discovery or progression within the core mechanisms of the game, maybe. 

Done:
- Initializing levels so that squares's target scheme are fully random
- And their initial "toggle" state is random as well
- And the number of squares is random within a range
- And the solution is also random (the number of clicks for the solution is random within a range)
	- ? Should the solution include the possibility of clicking the same square more than once?

The level and goal generation is fully random (within some parameters). This is done because I have no idea of any heuristics that indicate whether some aspect or a level's configuration and its goal make it more interesting than another, those will have to wait.

(This isn't entirely true, I have already implemented some initial heuristics like: "the solution of a level shouldn't include only one click" and "the solution shouldn't be to click all the squares once". The second heuristic follows from the initial paper protype I made just for coming up and pitching the game idea although I'm not sure if it still applies, I should double check to make sure it has value. These heuristics follow the idea that playful interaction shouldn't be trivial/totally predictable)

The game is totally unplayable by someone who doesn't know the rules and the goal of the game. That's kind of a consequence of minimalism (and early development stages), there is no external reference to anything that could inform the user of what to do and what to do it for. I have to create those affordances, signifiers and visual mappings myself. The aspects of the game I'll need to make obvious are:
- Squares are clickable
- Squares have two possible states
- Squares can switch between these states of being through clicking squares
- You can reach a cumulative state of squares that is the goal of the level
- Big, middle, interactive squares are mapped to smaller, bottom, uninteractive "goal" squares

I'll need to make those affordances perceivable with the help of "knowledge in the world" (the game world, that is), in the form of feedback, feedforward and visual signifiers displayed on the elements of game's interface, or "knowledge in the head", in the form of a tutorial, formalizing the core ideas and mechanisms of the game in the head of the player. A challenge with the tutorial approach is that there is no guarantee that players will actually remember any of it if they come back and play again. Also there's something very much not minimalistic about a written out tutorial. My first attempt at "teaching" the game will therefore be through the design of perceivable minimalistic signifiers.

## 

? Should it be possible to beat the level but in more clicks? Giving partial rewards? What are rewards? When the reward is just being able to go to the next level, what could be a "partial reward"? If not, players shouldn't be able to click on more squares after clicks counter == solution clicks) 
	? Also what if the solution is possble to achieve with fewer clicks? Extra rewards? Should compute the optimal number of clicks to get to the goal and use that as a goal)
	
Implementing undo and redo features this early in development is kind of bothersome. I'd be more interesting in testing out different algorithms for level generation and even more targeting and toggling mechanics, but I do feel like undoing and redoing is important in these types of puzzles and that advanced mechanics and level balance don't really matter if the basic "feel" for solving puzzles (which includes the possibility of players navigating puzzle solving trajectories/histories) isn't there in the first place

Thought for a MUCH later time: if complex levels seem way to complicated by themselves, maybe a "level" could be a succession of levels that lead up to a very complex one, than the "next level" would start back with a simple array, propose a bunch of simple-to-complex arrays and culminate in a super complex one, rinse and repeat
	These multiple-array levels could be arranged vertically and scrollable, or in horizontal sequence, connected like in The Witness