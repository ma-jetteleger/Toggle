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
	- Wrap-around toggles (for the first and last square)
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

## 2024-10-10

Started putting some thoughts and reflections as in-code comments. It felt very intuitive to write about questions, uncertainties, options and reflections about a feature right where the very specific uncertain elements of that feature live in the code. It helped to write about something right in the local context of that thing, basically.

I'm unsure what to do about this new type of documentation. Do I round them up and detail them here? Do I leave them in the code until they're "resolved"? It's interesting to think about their lifespan in the code, as the code is something that shifts and changes a lot, so these bits of reflection might be more shortlived than others, the ones that appear in this journal, for example

Right now I decided to copy and paste these in-code reflections here, so I could comment on them

	// Should the order of clicks matter??? Right now it doesn't
	// I expect that this will depend on what toggle features are implemented in the future 
	// So I'll leave the code that handles permutations in, if the need to use it arises at some point
	
	After implementing some algorithm to test for all possible solutions to a particular puzzle, I stumbled onto a pretty big revelation. Turns out that, for any particular solution to a puzzle (the specific squares clicked to reach a desired state of the array), all of the other permutations of those clicks are also valid solutions! So it doesn't matter in what order you click those squares, just that you click the right ones. 

	I'm not sure if it totally breaks the game or not... there still is some challenge in figuring out which squares need to be clicked. And it's not even a guarantee that the player will catch on to this. I took running some code and looking at the debug for me to figure out, it definitely didn't *feel* like the order didn't matter. 

	This could also be solved by implementing some advanced toggle features like conditional toggles. Making the order of clicks matter. But this would also increase the complexity of the overall design of the game, so this kind of feature is kept on ice for now
	
	// Order the solutions to be displayed in descending order?
	// Feels like going from highest to lowest, in terms of gameplay, 
	// makes for a bit more of a "climactic" progression/finish
	
	Simple UI question. I'll definitely need to test this and ask players what they prefer
	
	// Decided to not compile solutions of the same length,
	// to avoid duplicate clicks count on the interface
	// and to avoid the complications of checking for exact
	// solution sequences instead of simple numbers of clicks
	
	This is an example of function following form, not the other way around. It felt weird to have the same number displayed for two different solutions. Since the only signifier we have to "identify" solutions visually for the players is a number, it makes it so that the same number == the same solution. So I cheated the algorithm and "cut" those solutions that have the same number of clicks. 
	
	I feel like it's a bit of a waste, since I find that the more solutions a level has, the more interesting it is (maybe not 100% correlated, they must be other factors, but this is still a contributing factor). So it's sad to cut out "content" for the game like that. I might try to find a solution to this (identifying solutions with more than just a number, so they can be identified)
	
	There's another reason I decided to cut these solutions though. They're harder to identify and validate in code as well! So this is also an example of content/features cut to simplify the development of the game

I'm also finding myself writing in unused/empty variables and functions in the code as reminders of features to implement. Using the code as self-communication is new for me, especially up to that point, but it's feeling pretty intuitive and effective to do so. Not "jumping out" of the project to talk to yourself, take note and document, but keeping in the flow and doing so right as you are coding 

Another thought: keeping the options for two mutually-exclusive features open (single-solution and multi-solution level completion): is it worth it?

	I worked a lot on implementing a way to keep two options for a feature I had in mind available and to be able to switch between them for 1: testing purposes and 2: figuring which one of these options, later on, would fit best with the rest of the game. It's basically my way of not committing to a change in the design space of the game, because I have no clear intuition on which one of the two is best
	
	So basically I implemented a feature switch to toggle (no pun intended) between these two "versions" of the game's design. It was pretty convoluted and cluttered up the code and I'm questioning whether or not it was actually worth it. I could have done the same thing by using the "branch" feature in git and I'm interesting in trying this out for a similar situation in the future. I'm unsure how brancing could have benefitted me, in this case, and how it would have allowed be to develop these two "versions" of the game in parallel like the feature switch does. We'll have to see in the future, to compare both approaches
	
	A question to ask (or to have had asked, before doing all that work to keep both versions of the feature alive) could be: is the value of keeping both versions alive worth all that work? Honestly, maybe not. In minimalist games, I find that I am drawn to exploring the entire design space of a game's idea. So I'm always tempted to use the power of minimalism (not having a lot of options to begin with) to do just that, by not committing to big changes in the design space to keep all options open. Well, I am drawn to that in the development of any type of games, it's just that minimalist game design makes that (seemingly) possible. There is definitely a limit to this, however, and this is why I'm asking: is it even worth it?

A thought that is more specific to the game: should I deactivate clicked squares? (that question keeps popping up since the beginning of the development of the game, this is probably a sign I should answer it soon) It feels like it would help in the understandabiity of the objective of the game. But that also means I can't have solutions which have the same square(s) clicked more than once in the future. Is that even something that we want? We should test that soon
	
	I decided to keep this thought in the journal, but in the meantime, I stumbled into a somewhat-game-breaking problem (see 2 commits ago) that might be solved by the deactivating clicked squares. So this could be my "excuse" to finally implement that feature

Sometimes, a minimalist game stays minimalistic by itself. It doesn't offer a too broad design space and the decisions to make are simple and few in number. Sometimes, I have to be very specific and deliberate to "keep it simple" despite the growing and complex potential features that emerge and that can make the game more interesting. Toggle is the of that second type

Final thought that's really unrelated to this iteration of the game: numbers and letters and words feel like I'm talking to the player. Icons and symbols feels like the game is talking about itself. I'd like to move away from using numbers are some points. Maybe using collections of dots like on the faces of a die? We'll see

## 2024-10-15

My current concern was to fix the "click a square multiple times to reach the specific amount of clicks for a solution" problem. I fixed it by making the squares uninteractable after being clicked once per level. I found that it also had the extra consequence of also emphasizing the "puzzle solving" aspect of the game by making the effect of clicking a square more clear than before. Basically, it enhanced the feedback. This new restriction makes the whole experience of a level feel more like a unique puzzle than a fiddly trial-and-error type of challenge.

Since the last change, I basically have to game "modes": single solution and multi-solution. I found it very weird that it was still possible to generate multi-solution puzzles in single solution mode, and vice versa. The difference in modes wasn't the generation of the levels, but only the player-facing goal/win condition. I changed that by forcing the algorithm to generate levels that can only fit the mode their in. That way, the game is/feels more different across those two modes.

I also started adressing the feedback I received from the playtests that were done last week. I definitely started with some low hanging fruits, like visual changes on the UI. Next I introduced a "new" mechanic: the wrap around toggles (first square can toggle the last of the array, and vice versa). It's not really a new mechanic, from my perspective, it's more like repurposing a buggy feature into a functional one. At the beginning, I had "useless" arrows pointing left from the first square of the array and right from the last, they toggled nothing. I removed them with some extra conditional coding. Well to add this "new" mechanic of wrap around toggles, I simply removed that conditional coding and quickly added the functionality to link the first and last square together as if adjacent. It's fun to think that minimalism allows me to repurpose and turn code around very quickly into another "new" feature since eveything is so simply coupled together.

Next on my list: 

- Progression through difficulty/complexity of levels
- A way to test different features and modes from the game itself
- Oh and make the clicks counter more prominent/clear (why am I avoiding some tasks like this one, which isn't new like the two I just mentionned? is it because it's not a new shiny thing to work on? is it because it's "return" and it's value in the project is less clear? is it because I don't have an immediate knowledge on how to tackle it? mmm)

## 2024-10-18

I introduced a very basic progression scheme for now that can be summarized as: more levels = more squares. It's definitely not enough, some of the features should be introduced progressively as well, like some of the more "complex" targeting arrows and the wrap around toggles, even the multi-solution levels should maybe be introduced a bit later on than right at the beginning.

After fixing the bug that broke level generation for multi-solution levels, I now realize that I can't rely on real-time level generation to create levels that always have multiple solutions or always just one. The random aspect of the level generation makes it so sometimes it's impossible for a configuration of square targets et toggle states to have multiple solution. I really want to make it that in multi-solution there are only multi-solution levels generated, otherwise the inconsistency it would feel very much like a bug. To fix this, I'm tempted to do the following:

- Pre-emptively generate every possible combination of square state/arrows for each amount of squares that I support in the game and build a master list of all the levels that indeed have multiple solutions 
- Store this static list somewhere in the code and pull from that list instead of generating levels in real time

This would ensure that the levels presented to the players are always valid according to the "level generation rules" that I set, without just guessing. It would also help me limit the number of possible levels from "infinite" to an actual concrete number, which could be useful in assessing the possibility space of the playable level configurations.

This sounds good intuitively, but I'm worried it might be too complex or even too computationally intensive to be actually feasible. 

In this project, minimalism is slowly manifesting itself as the most thorough/exhaustive exploration of a possibility space. Therefore, the role of a minimalist designer is to restrain the possibility space as much as possible so it's possible to explore it exhaustively?

## 2024-10-19

I won't do the whole exhaustive thing, I computed just the amount of possible levels (every possible configuration of toggle states and target schemes for each square + every possible solutions for every possible configuration) and it was in the trillions, and this is just with the basic features of the game. It doesn't make sense to store/iterate through/sift through that amount of data at any point.

The easier version of this is to still keep the random/brute force level generation algorithm and run it repetitively, storing the valid levels in a file, and pulling from those already validated levels in that file when it's not possible to generate a valid one at runtime.