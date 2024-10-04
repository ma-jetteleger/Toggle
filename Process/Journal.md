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

## 2024-09-06

Implementing undo and redo features this early in development is kind of bothersome. I'd be more interesting in testing out different algorithms for level generation and even more targeting and toggling mechanics, but I do feel like undoing and redoing is important in these types of puzzles and that advanced mechanics and level balance don't really matter if the basic "feel" for solving puzzles (which includes the possibility of players navigating puzzle solving trajectories/histories) isn't there in the first place

The implementation of these features have made me reflect deeply on their implications in playing with interactive stateful puzzles. Undo and redo actions still have to make sense interactively, even though their effects are a bit disjoint from the game's interface compared to the "regular" actions they revert back. What I mean by that is that "clicking a button and seeing a reaction" is one type of interaction, and "seeing the button un-clicked and the reaction un-done" is different, specifically because the player doesn't "unclick" the button directly. They press the undo button, which enacts the "unclicking" indirectly. More careful considerations must be put in place to make sure that the interactivity stays robust and discoverable through feedback and signifiers in this more "indirect" interaction with the interface of the game.

Additionally, I have implemented what I call "clicks restrictions" in the game's system. Which made me reflect on questions that I already had in my head that surround those restrictions:

- ? Should it be possible to beat the level but in more clicks? 
	- If yes, should it give partial rewards? 
		- What are rewards, in this game? Going to the next level? 
		- When the reward is just being able to go to the next level, what could be a "partial reward"? 
			- Just the fact that the player knows they haven't beaten the level is the optimal amount of clicks?
	- If not, players shouldn't be able to click on more squares after clicks counter == solution clicks
		- Or, at least, players shouldn't be able to progress to the next level (as a reward) if they "busted" the goal amount of clicks to beat a level
- ? Also what if the player beats the level with fewer clicks than computed? 
	- Extra rewards? What are those?
		- Same thing as partial reward? The intrinsic satisfaction that you've "broken" the game?
	- If we don't want to address this, we should make sure to compute the actual optimal number of clicks to get to the goal instead of the dumb brute force algorithm we have now
	
Additionally additionally, a thought for a MUCH later time: if complex levels seem way to complicated by themselves, maybe a "level" could be a succession of levels that lead up to a very complex one, than the "next level" would start back with a simple array, propose a bunch of simple-to-complex arrays and culminate in a super complex one, rinse and repeat.

These multiple-array levels could be arranged vertically and scrollable, or in horizontal sequence, connected like in The Witness, maybe.

(I'm thinking about this because I'm anticipating the "late" levels in the game to either be "not enough" by themselves, presenting the player will too small bites that aren't satisfying as themselves, or conversely I'm anticipating the players to be overwhelmed by very complex levels and not have the proper "micro progression" within a short play session to overcome the later game challenges)

We will definitely revisit this at some point.

Oh and very quickly! Should the click counter count down or up? Should it "leave a trace" (i.e. show the goal amount of clicks) and show what it's actually counting to/from? I'll formulate a todo entry to try and not forget to try different things out for this.

## 2024-09-09

The most recent work involved changing the preliminary visuals and some bug fixing. It might sound counter-intuitive to spend time on visuals this early in a game development project, but when it comes to minimalism, the visuals are so directly connected to the player actions, that they should at the very least be iterated upon just as much as the interactions themselves. 

Also, the development cycles of a minimalist game project are treated differently than those of a regular development project. Cycles are a bit more executed in parallel as they are in normal productions. Prototyping, production and polishing are not kept separate as it would make little sense to do so when those stages, in this context, are so very short and little by themselves. The cost of "going back and forth" between stages of development is a lot less in minimalist game development, so I don't bother keeping the "visual polish" phase as an exclusively late stage of development. 

I'm interested in writing more about this topic later as now is not the time to write an entire essay (and this is the first time I actively reflect on that specific question).

As a seperate thought, if there are multiple ways to solve a level, we should probably reward the player for finding all the possible solutions
	(or generate levels that only have one solution)
		((but that might turn out to be too difficult or even maybe less interestng?))

## 2024-09-19

I'm thinking that I should spend some time soon on actively "finding the fun" in the game. I've been kind of playing around and implementing basic features, hoping for the fun to simply emerge from that (it sometimes does), but it doesn't seem like it's happening right now with this game, so I'll turn to a more active investigation.

Basically, I've been trying to refine the core loop to make the fun "pop out" of it. Maybe I need to do more than "refine" it. Maybe I need to twist it in a different direction instead. Or maybe I need to make the core loop a little less tiny (as a minimalist developer, that's a scary thought).

My first intuition is to brainstorm a list of different mechanics than the basic ones that I have implemented. First thoughts include:

- Having squares toggle other squares in a maybe less intuitive, more interesting manner(s)
	- Activating the toggle mechanisms of other squares when toggling them
	- Toggling squares not directly adjacent to them
- Clicking squares to do other things than just toggling other squares
	- Making other squares uninteractable/interactable
	- Destroying other squares
	- Moving other squares

The danger (risk?) with thinking of new mechanics to a make a minimalist game fun is that by adding mechanics, the game might not be minimalistic anymore. When/under what conditions that line would be crossed is very more unknown, at least in an explicit sense. More mechanics also mean a need to actually teach/explain/visually inform the player of those mechanics. That also might break the minimalist line.

So maybe a game that needs "too many" mechanics to be fun is not a minimalist game? 

Anyway. Basically, I'm finding the need to investigate the design space of the game in the hope of stumbling onto something that makes it fun. 

I'm not sure why the game isn't showing its fun by itself to me like some others do. Maybe it has something to do with its design space. The size of its design space? The way its traversed? It might not explainable at that point in time (or ever?).

The question of whether the core loop is too tiny or at what point minimalism breaks is scary and daunting. I could spend a lot of time reflecting on that so I'll stop for now and revisit this entry/thought in the future. 

# The list

- Toggle mechanisms
	- Cascading toggles (only certain squares? indicated by special arrows?)
	- Distance toggles (toggling squares not directly adjacent to a clicked square)
	- Conditional toggles (when white, do something different than when black)
	- Destroy other squares
	- Moving other squares (swap position with the clicked square)
- Other mechanisms
	- Squares clicked more than once in solution (all squares? with a special indicator?)
		- This involves implementing the opposite: make clicked squares uninteractable for the rest of an attempt for a level
	- Movable squares (drag and drop)
	- Non-interactable squares (permanent? toggled off = uninteractable?)
	- 2D arrays (multiple parallel arrays? grids?)
	- "programmable" clicks
	
I think this covers the possible espansion of the design space of the game as it is right now. Of course, implementing any of this is going to affect the potential design space further.	

In fact, I'm getting a new idea for a maybe different game while working on this list...

	- Same setup, same objective, you have an array of on/off squares with arrows in the middle and you need to match another array of on/off squares. But instead of activating the squares directly, you have a "deck" and a "hand" of squares, and you add them to the array, activating the new squares and affecting the array by doing so. Maybe then the game isn't to reach the "match" in a number of moves, but to do it as quick as possible, before an opponent. Maybe it's a card game.
	- On your turn, you can maybe choose to "tap" a square to activate its toggle mechanisms, or choose to add a card to the array, activating it
	- Do you play on your own array? Or on a common array with different personal objectives? (second option sounds really difficult to balance) 
	
## 2024-09-25

Today I worked in a very basic heuristic to generate more interesting levels: not putting useless arrows on squares. Also I made it possible to generate longer levels and added a telegraphing feature when hovering the squares (which will have to be different on mobile).

The rationale behind implementing these minor modifications before working on more substantial pieces (like new mechanics and diving deep into the level generation algorithm itself) is because I want to get the feeling of playing with the squares right before I make the game more complex/fine tuned. I feel like interesting levels won't "feel" interesting if the handling of the squares and the core mechanisms of the game don't feel good because players don't know what's going on.

I want players to understand what's going, on first and foremost. And it's proving more challenging than anticipated. Interactions with the squares and their resulting reactions don't feel as natural as I had originally hoped. So I'm working on that feeling to make it as natural and informative as possible before moving on to the balance/progression/variety/challenge part of the game