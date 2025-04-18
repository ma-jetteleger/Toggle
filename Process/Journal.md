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

### The list

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

## 2024-10-24

Enforcing consistency in the level generation algorithm turned out to be a really challenging technical hurdle. I really focused on this issue because I felt like progression, extra features and visual/experiential polishing was secondary to a consistent goal.

Having to fix this issue instead on working on expanding the design of the game really made me reflect on my priorities as far as which features the end-version of the game will include. In the beginning of the project, I was thinking about all these possible features and how they might make the game more fun, more challenging, longer and more interesting. Being in the weeds on the technical side of the game helped me gain perspective and re-scope the project.

Well, re-scoping is not exactly easy and done, but I know that I'll prioritize teaching the game through a intelligent progression through the available features first, then maybe add a feature or two (Not the ones that are about more than arrows and toggles. Maybe cascading toggles, maybe distance toggles, maybe adding squares in 2D space). My intuition is that the decision to include extra "maybe" features should be based on which features are already partially supported in the current system, as opposed to the ones requiring a brand new system or the complicated transformation of one.

I'd also like to take the time to reflect on minimalism more generally, outside of very specific and concrete applications in this game. 

## 2024-11-01

Today I coded in a simple system to exclude features from the levels generated by the algorithm until a certain "progression index" (simply the number of completed levels) is reached

This made me reflect on what constitutes a feature (or a mechanic) in the game. I originally thought of all the possible targeting arrow schemes as one standalone feature but when testing different ways to set up the progression, I realized it made more sense to separate them in terms of complexity. Self targeting and targeting squares only to the left or right side are the most intuitive mappings, so presenting them to the player first makes the most sense. Then, self + side targeting is the next logical step, before the "target everything" feature is introduced. 

Wrap-around toggles are also introduced a bit later than the first level so the player can get acustomed to simpler, more linear configurations before that.

This also makes for a progression through levels that isn't only more gradual and easier to grasp, it also keeps the player interest alive a bit longer. This allows them to discover new ways the game can be interacted with gradually and over time, as opposed to seeing the whole game and all it offers all at once and feeling like they "got" everything they can get from it very early on.

Next, I want to add more features (maybe only just one more?) to the game so that this interest is renewed at least once more. 

From this reflection on this small handful of mechanics, I also am slowly realizing that, honestly, I think the game is a bit stale. It's not bad or empty or anything, but the core loop isn't making the player "hold on" to the game for a very long time. I'm not sure what the best approach is: accepting that the game offers a very short experience that doesn't need to be expanded, or continue to dig and try to find a way to make the core loop more interesting in itself or a mechanical twist that would make the base experience reach a bit of a peak and hoping that this provides satisfaction and closure to the player?

The reflection on minimalism that I want to materialize here is a bit daunting so I'm pushing it back for later. I'll jot down a list of ideas (general "aspects of minimalism" that take form within this game) first to make it possible to jump back in later:
- Static, completely and exhaustively visible interface in one screen
- One interaction mechanic, one objective
	- (How can we do progression through that?)
- No movement
- Discreet positioning
- Algorithmically generated levels
	- Dehumanized level design, empty of concrete decisions and "marks" by the author
- No text 
	- (Possibly and ideally even no numbers?)
- No representation of outside "objects"

## 2024-11-04

I want to add a feature that's going to feel like a twist on the whole concept. In the "nice to have"s list of features that I wrote in here a while ago, I included "cascading toggles", meaning squares that, when clicked and toggling their target(s), will also trigger their target's like a click would, toggling their respective target(s) as well

I feel like this kind of twist would be a very interesting evolution on the game's core mechanism, as well as being very minimalistic in nature since it doesn't change or add a new way to interact with the game. Rather, it changes how the game resolves the same mechanic. (That thought is an interesting way to think of minimalism: one mechanic but different ways the game reacts to the same mechanic)

But there's an issue: how do we show that resolution of multiple simultaneous toggles? It has to be a chain of animated feedback, not just all happening at once. Otherwise, there's no way the player is going to understand the "chain" of toggles. I need to find a way to animate that chain minimalistically or drop that feature entirely

## 2024-11-04.1

I ended up implementing the cascading toggles immediately, and it works, although there is no animation or feedback yet. I still have no idea how to make this feature look and feel like what it does functionally but I at least wanted to test the functionality, to play with it and figure out if it was interesting or not.

I feel like it's interesting enough to justify exploring this further. Many questions and design implications emerge from the initial prototying of this feature:

- Do I need the animation/feedback? I got used to having none eventually... Will players also get used to it? We should at least try to add _some_ feedback
- It makes the whole game a lot more complex, not just a little, especially without feedback. Is that level of complexity something that we want?
- Should the cascading indicator be on the clicked square that triggers the cascade? Or on the toggled square that was triggered by a click from another one? 
	- Right now it's the second option. Should we test both with a feature switch?
	- On the second option, squares that can't receive a toggle from another shouldn't be allowed to be cascading
- Not every square is cascading, just some random ones
	- Should there be a random number of cascading squares (none is boring, all is excessive (maybe all is just super complex and good for later levels))? A random range that grows with progression?
	- There should be some kind of progression to tie in the cascading toggles and the targeting schemes. Making the most complex arrows also cascading on the first levels that introduce cascading toggles is a bit much
- We should make sure that the placement of the indicator dot looks good alongside every possible targeting scheme
- Cascading toggles can trigger others in succession
	- This is good. This is a piece of emergent design that I didn't anticipate and it opens up complex player anticipatory play without me planning for it
	- This should be accounted for in the progression and "short" cascades should be generated/introduced before "long" ones
- Should there be something in/around/near the prediction rectangle (or at least something that happens on hovering a square) when a cascade is predicted? 
	- Clear and precise feedforward? Or some vague/simple indicator that something's about to happen
		- This is a good question for the basic prediction as well, maybe the one we have now it "too" clear and informative
	- Simply highlight the cascading indicator dot, maybe?
- The cascade trigger stays active even if the square is uninteractable (after the square is clicked), should that be a thing? Can we test the "deactivation" of a cascade trigger?
	- How can we communicate that (the fact that it stays active or that it goes inactive) as clearly as possible?
	
The design implications (and the questions that emerge from them) are a lot, but otherwise I'm really happy that this was a relatively simple addition to the game and that it didn't blew up the scope even though it's a pretty huge change (at least in terms of complexity)

The code was pretty much already all there, I just needed to add a line or two to re-call the "click" function on a toggled square to make the cascades happen, and some checks to make the level generation algorithm be able to anticipate the cascades. This is sort-of proof that this is a minimalist addition to the game: I was able to implement it without changing or adding an entire system, I just needed to extend one that already existed

## 2024-11-18

Forcing the level generation algorithm to output specific features doesn't work. The conditions under which some features are valid are too specific and ensuring the generation of those features makes it so other features (or possible combinations/configurations of features) will be ignored. I'm tempted to go another route: to pre-generate as many levels purely randomly like before at edit-time, and to drop the use of the level generation algorithm at runtime altogether. Instead, I'll write a search algorithm that will crawl through the list of pregenerated levels and pool the ones that fit the specific progression requirements. Then the search algorithm will select one of those at random for the player to play. This is basically what's already happening when the generation algorithm fails, but I want to lean into this approach and pivot to using it more deliberately and reliably as the main way levels are provided to the player

## 2024-11-22

I powered through and managed to find a way to make my "ideal algorithm" work... I haven't included every feature in it yet and it's definitely not bug-free as of now, but I'm hopeful...

This is requiring a lot of work but I can't seem to prioritize anything other than consistency right now. Consistency in the progression, that is. And that can only be achieved by an level generation algorithm that is supported by a progression system which allows for inputing and choosing which features are introduced, and when, without fail

## 2024-11-25

I managed to get all the features supported by the new and improved progression system + level generation algorithm. Even the cascading toggles and whether or not they are adjacent to other cascading toggles. However, there are limitations:

- For most features, I can only control the minimum number of that particular feature that is included in a level. So for any particular targeting scheme, and for wrap around toggles, I can only set it to "fully random (including none)" or "at least this number"
- For the cascading toggles, it's either fully random or a fixed number
- For adjacent cascades, they are either allowed (not forced) or disallowed (forced not to appear in the level)

Having full control of a numbered range for all features, including the possibility of random within flexible lower and upper bounds, proved to be too complex for the scope of this overhaul

- There is also an issue with the wrap around toggles' targeting schemes being set at random between all the possibly valid targeting scheme for the square, not prioritizing any targeting scheme over others. This makes it so that, in very small arrays: a random, not prioritized targeting scheme might take the place of a supposedly prioritized one. I haven't found a fix for that and it will only ever happen in small arrays, so I'm treating it as a very minor, and therefore ignorable, concern for now

The only thing that's left to be done about this whole task is to re-work the "get level from file" part of the algorithm to fetch and overwrite a failed attempt at generating a new level from scratch with the new rules for generation

With this complete, I think I'll be ready to consider this game mechanically feature-complete. I'll work on the UX and finally choose what to do with the two different modes, but I won't consider adding any new mechanic to the gameplay loop

## 2024-12-05

The game is feature complete! Both in terms of the features/mechanics included and the progression system through which these features are introduced to the players. 

Of course there are many many many things I could have added, as it is always the case in game development, but I'm going to stop there. The work that's left to be done resides now in tweaks in values and parameters here and there, in some visual/presentation choices and in the flow of the game (menus, saving and loading, that sort of things)

Note that the progression "values" themselves are not in their final states. Also, I had a player test the game for their first time today and the progression was very satisfying up until the cascading squares were introduced! That point is a way too steep difficulty spike, especially because the cascading squares can be of any targeting scheme, making it way harder to understand the feature if it's introduced on a complex targeting scheme rather than a simple one. I just said that the game is system-complete, but I might go and try to revisit that quickly before calling it really done...

A visual indication for the cascading mechanism could also help a lot... This is the last work to be done, I promise...

I also chose to go with the single-solution mode as the main mode of the game. Even though I really liked the multi-solution approach conceptually, the feedback I received made it clear that it was superior in terms of player experience and flow. I might offer the multi-solution mode as an alternative way to play the game (through a menu option or something similar) as it's still fun and some players might enjoy it.

## 2025-02-12

Let's go back in time a little to address the changes and the thoughts I had while continuing the development of this game while I was too overwhelmed to write them punctually

In the last journal entry before this one, about two months ago, I was mentioning that the game was finished (at least in terms of features to be added) and that very little work was still needed to complete the project. This was both true and false

### 2024-12-09.1

In this project, I'm trying to get away with making as few decisions as possible. Hand-crafting levels would be making a lot of micro-decisions so I've opted to craft a level generation algorihm that handle both the generation of levels and the progression through the mechanics included in the game. This way, all I have to do is to make a "few" macro decisions about how levels should be configured and when/how mechanics should be integrated in those levels. Examples of such macro decisions include:

	- the level's objective configuration should be reachable by clicking a specific amount of squares
	- levels shouldn't be in a solved state when presented to the player
	- an extra square should be added to the array every couple of levels to increase difficulty
	- Wrap around toggles sould be introcued when the player has already beaten a couple of levels to understand the basics of the game
	- the player should be presented self-targeting squares and simple left/right target schemes before being introduced to multi-arrow target schemes and cascading toggles
	- cascading toggles should be introduced in isolation
	- cascading toggles shouldn't be adjacent to each other when first inroduced 
	- squares with cascading toggles should always be targeted by at least another square
	
Implementating, writing and making sure that these macro decisions are respected by the game's systems in a bug-free way is a lot of work. And the system that takes all of these decisions into consideration while generating levels is very complex, requiring a ton of (technical) decisions. However, I still consider this way of working minimalistic since it enables me to make fewer (design) decisions. 

This way, I don't have to decide on each and every level to be included in the game, I also don't have to decide on the initial toggled state of each square, their target scheme, and their relation with the objective of the level. I only have to decide on the rules of the game, not their implementation.

This reflection came to life when realizing that at least half the amount of time working on this game has been focused on working on the level generation/level progression algorihm. A ton of technical work to avoid a ton of design decisions

### 2024-12-09.2

I was giving away too much information with the prediction rectangles that helped predict in which toggled state would be a square if you pressed the currently hovered one. Not only this, but even though it was a "helpful" feature, it added to the stack of this the player had to understand/internalize to complete their mental model of the game.

By changing it to a very austere black dot instead of a colored rectangle, I made it so player still had a hint at knowing which square would be toggled by the other squares, but they still had to "do the work" of understanding the end color of the toggled square

I also acheived two unintended consequences with this change:

- Without the coloration of the rectangles, and with the smaller size of the dot, it's now possible to "stack" those prediction dots, making it possible to support the prediction of of a series of cascading toggles with a stack of dots. Minimalism as a way of making things (like signifiers, in this case) more versatile in an emergent way
- Gathering feedback on this change revealed something that was already there: there are two very different kinds of player feedback I'm getting every time I playtest this game. The first is feedback according to which the game does not inform the player about what's going on, what to do and what will be the outcomes of their actions. The second is that lack of information makes the game more interesting, that there is a lot of enjoyment and satisfaction from figuring out the mechanisms of the game without much help. 

The dichotomy in the second point is become more and more apparent as I introduce features intended to minimalistically signify possibilities of interactions (affordances, signifiers) and outcome of interactions (feedforward, feedback) in the game and I'm pretty much always getting responses ranging from: "this is helpful, but not enough to understand" to: "this is too helpful and it's taking away the enjoyment of figuring it out by myself"

In choosing to go with cryptic black dots instead of colored rectangles, I'm trying to strike a balance between those two different kinds of engagement with the game

### 2024-12-12

I started adding some animation in the game. Originally, I thought I could get away with having absolutely on movement in the game. It felt very satisfying to have a completely static interface with only instant state changes

It was satisfying for me, as a minimalist designer, but not very satisfying for the players

Many players reported either not seeing some essential visual element, not having their attention pulled enough by it, or not being able to "map" it to the correct game mechanism and/or other connected elements of the game

In order to solve this, I added a "punch" animation to some elements of the UI I think are essential to understand: the clicks counter and the levels cleared counter. Both now "punch" when they are increases or decreases as a consequence of a player action (clicking on a square for the clicks counter and beating a level for the levels cleared counter)

At some point previously, I attempted to create a connection between beating a level and the levels cleared counter by putting a known symbol (a trophy) next to the counter. It proved useful but I quickly removed it when I realized it broke the minimalist intent of non-representation

By connecting/timing the animation of the number to the player action, I'm leverage the (more minimalistic) concept of causality, guiding the player toward establishing a cause and effect relationship between their action and the "punched" visual outcome. Hopefully, this will draw enough attention to these elements and help players map them to the correct mechanism without "real life" symbolic representation

### 2024-12-26

By writing the commit reflection entries, I'll quickly catching on on some patterns. One of these patterns in the yearning for consistency

I recently wrote about the "rules" for level generation I wrote the algorithm to follow. One of these rules is that levels shouldn't come pre-solved

In another recent update, I updated the progression of the game with the intent to guide the player through a smooth level curve, especially for the first couple of levels of the game. This led me to introduce some very particular exception to the first two levels specifically. I wanted the first level to have only one square, and the second to have two

Unfortunately, I originally wrote the level generation algorithm to generate levels with 3 to 9 squares. Having fewer squares broke the algorithm, so I had to write some very specific exceptions for the first two levels. The second level was even more exceptional since I wanted a "two click" solution for it. Following this intent, it was mathematically impossible to have such a level (2 squares, 2 clicks) without presenting it to the player pre-solved. I wrote a couple of exceptions in the code to make sure the player could still solve it by themselves, but basically the level came with the center array already configured to match the objective array

I knew I broke my rule but I figured it was for a good reason: to teach the game through a ever increasing gradual number of clicks. And maybe players wouldn't notice? Unfortunately I received a lot of feedback afterward about players not only noticing but also feeling very weird about being presenting a pre-solved level

I had to back pedal and change the second level's exceptional configuration. I made it have 2 squares, but a one-click solution. I figured I can make exception and compromises on by ideal progression scheme, but not on the principle of consistency in interaction design

### 2025-01-03

When a new player plays for the first time, there is a good chance they'll be very confused about some of the elements they see on screen for the first time. The undo/redo/reset buttons don't show what their specific functions are if you're not familiar with puzzle game, the levels cleared counter is just a number and could mean many different things, and the objective array has frequently been associated with wildly different things, other things than what just "showing the objective of the level"

So I continued making presentational changes to reinforce action/reaction mappings for these elements:

- Hiding things (UI elements) until the specific point in time when it's used. One example is hiding the levels cleared counter until the first level is beaten. I did that to reinforce the mapping between the displayed number and the "action" to beat a level. I also did the same with the history buttons (undo, redo, reset) so that they only appear after the first player "move", again to reinforce the mapping between affordance (the button's function) and the related player action (making a move)

- Squeezing the objective array closer to the center interactive array to try and solidify the relation/mapping between a middle square and its equivalent objective rectangle. I toyed with multiple potential solutions like a connecting line between the two or sticking the objective rectangle directly underneath but both attempts were very visually unapealing. Reducing the distance between the two is a good compromise  

- Adding checks and crosses to hammer down on the mapping between the same elements mentioned above. These "evaluation graphics" will also help in understanding their relation to the objective of the game. In adding these symbols, I'm getting dangerously close to breaking the non-representation minimalistic constraint. But I figured I can get away with using abstract symbolism for binary information (good or bad) as opposed to using images of real world objects to represent complex ideas

- Adding more "punch" animations to put emphasis on related elements. The more I integrate those animation, the more I'm realizing the power of "timing" animations and punches to relate elements to each other and to player action (combined with the power of interactive causality). For example, by making a square from the center array "punch" at the same time as a rectangle from the objective array, these two elements are immediately put in relation to each other and to whatever the player did to cause that effect. I'm even making the new checks and crosses punch with their related square and rectangle to make the most out of this feature.

I'm happy to be working on visual/aesthetic stuff instead of features, it makes me feel like the end of the project is near. I think this is because that, when working on a feature, you're effectively expanding the design space of the game by making decisions, opening up new possibilities and triggering new ideas by uncovering more areas of this creative space. When you're working on visuals, decisions don't lead to an expansion of the design space, making it feel like you're stepping closer to the end of a creative space, as opposed to making steps that always open it up more and more.

### 2025-02-05

I've experimented with animations further. I originally had a very strict intention of only using these "punch" animations since they introduced very minimal movement and they also happened very "instantly", keeping to the intention of only presenting "discrete" state changes, not actual "continuous" movement. The consequence was, however, that everything in the state change (the click, the toggling, the decrementing of the clicks counter, the check/cross evaluation, the possible chained cascading toggles, the possible "beating" of the level, the possible increment of the levels cleared counter) was happening and was represented by a punch all at the same time.

The solution was to introduce delays in the "click" sequence. Even a small delay between the multiple effect of a click makes it so players can focus on each effect separately and can therefore better understand the cause and effect relationship between their actions and the reaction of the game elements.

The introduction of these delays gave me room to experiment with more complex animations. With that available space, I made the arrow indicators move (in the direction of the arrow, toward the square to be toggled) to clarify their role in the toggling mechanism. That changed everything   

With that animation taking center stage after clicking a square, the "targeting of other squares" effect is much clearer and, immediately, I received feedback about there being a lot of redundant information in the game. Information that was redundant, but also making the game more confusing. 

The prediction dots, specifically, had to go away. They were technically present to help the player predict (feedforward) but also understand the targeting effect of clicking a square (feedback). However, playtests revealed two things: the first is that players didn't need them to understand the effect of a click (as feedback) since that information was now present and felt through the arrow animation. The second is that most players not only did not understand their function as feedforward (prediction), but they added visual clutter and made it feel like they were part of another system the player had to grasp. In the words of a playtester, they made the game "not minimalistic enough". Even though these dots were meant to simplify an existing system, they ended up complexifying the game by adding extra unnecessary information for player to put and try and fit in their mental model of the game

The same reflection applied to checks and crosses. As much as I meant for them to be helpful and guide the players to understand an existing system, they ended up making the player have to think harder about how these extra graphics fit their mental model/understanding of the game, scattering the actual grasp they had of the game's systems. They had to go

I extended this reflection on visual clutter to the dichotomy of player types I mentioned in an earlier entry. I had made the decisions to introduce predication graphics and evaluation checks and crosses to try and help players understand the mechanisms making up the game. But it turns out that they not only hindered their building of a mental model for the game, but they also deteriorated the enjoyment of another type of players: the ones that yearned to figure it all out by themselves. By clearing out the clutter, it also had the consequence of me leaning 100% into the minimalist intention of giving away as little information as possible to the player. For their enjoyment of course 

It's interesting how decisions lead to other decisions: adding simple delays in animation -> made room for adding a small amount of extra feedback -> removing redundant information -> reflecting on user engagement ->  reinforcing the main design intention

While playing with the objective array after removing the checks and crosses, I experimented with different configurations, distances but also shapes. I made the objective array squares again (last time they were square was in the first week of development) and it made me reflect on mappings again. I originally made them rectangle so that they "fit" directly under the square. But that came at the cost of having them a different shape. Then, is a "shape-based" mapping between elements better than a "distance-based" or a "position-based" mapping? I don't know if there's a general answer to this but in this specific situation, I feel like both arrays being the same shape (squares) makes for a better mapping between the two elements. They basically feel "more like the same array"

I'm still happy to be working on visuals, even though there's a lot of coupling with technical/gameplay stuff, it's not as overwhelming as making feature-related decisions. For the same reasons as the last entry  

### 2025-02-12

Back to the present. Back to saying I'm done with this project while still maintaining a list of things I'd like to include in it in my mind. These things are:

- Colors
- Sounds
- Communicating the "intended length" of the game
- Maybe something about saving the player's progress to pick up the game again later through some menus. (This is the only thing missing that's stopping me from considering this like a "real" game... Does it have to be, though?)

There's difference now, though. The difference is that I'm comfortable with keeping these things half implemented and in the back of my mind while still considering and calling the game done.

In fact, I've already started experimenting with changing the color scheme of the game and with different ways to communicate the relative progress of the player/the length of the game. These experiments are on standby since I want to start reflecting on the design of the game and on minimalism. Even though the todo list isn't all crossed out, I feel like the game is in a stable enough, rich enough, understandable enough, satisfying enough and compelling enough state to use it as a showcase for an investigation into, through and about minimalism

I was recently asked what my point was, by making this game. It's somehow very easy for me to answer this question since I have a million different ideas and questions and intuitions and vectors of curiosity about all things games and minimalism. However, it's hard to give a definitive answer. I've been making this game to:

- find out what is a minimalist game, what is minimalist game design
- find out what constitudes a minimalist game design process
- find out how far I can go into minimalism while making a game
- find out how much minimalism is too much
- prove that minimalist games can be interesting
- prove that minimalist games can be fun
- prove that minimalist games can exist
- make a minimalist game so we can talk about it
- materialize an idea that spawned in by head about an interactive system
- gamify an interactive system 

But, weirdly enough, even though I got really close to fulfilling these "reasons" to make a game (and even actually fulfilling some of theme), I never considered my mission done until recently. I think there's a specific intention I had that, when fulfilled, made it really easy to call the project done. It also made it easier to have a definitive reason for making that game, that I can now communicate with the people that ask me this question. In reality, my goal was to:

- Make a minimalistic game that's compelling and functional enough that players want to experiment with its interface and rules so we can have a conversation about its mechanics, its systems and the process of designing it

It's not a pretty sentence, but it's a sentence

I got hit by surprise when I fulfilled this goal without having it fully formulated and, by the same account, finally feeling some closure even though all features are not implemented yet.

So the game isn't wrapped up in a neat little box with pretty ribbons on top yet, but its (interactive) design is done. I'm somehow confident that the last few minor things that keep existing in the todo list will be quickly taken care of once I have a clear idea for their implementation and once I have the energy to make them happen (without them being in the way of writing the follow up thesis). I'm somehow confident that they won't bubble up into another feature or idea. That they won't expand the design space of the game and trigger me into wanting to investigate the newly created nooks and crevasses of that space once I integrate them in the game. Or will they?

## 2025-02-21

Looking back through this journal, I am reminded of an intention I had several times along the process of designing this game: reflection on minimalism as a whole. That reflection got pushed back a few times for multiple reasons, mainly because it was daunting and it seemed to get in the way of actually designing the game. Well now that the game is done, I have no excuse to push it back further

Here's a mishmash of my final thoughts on this project and how my vision toward minimalism and toward my own act of making games have evolved through this project

### Revisiting the minimalist intention

On october 18th, after fixing a bug in the level generation algorithm, I reflected on an idea I had to generate every single possible level out of the different features, mechanics and level configuration parameters. My intention was to go through a thorough, algortihmically exhaustive exploration of the design space of the possible levels of the game. 

The reason I got tempted into doing that is because, to me, minimalism is about the exhaustiveness of the exploration of a possibility space. It's about setting up and maintaining a set of constraints on a design space so it's possible to explore it fully, in every and all its possibilities

Algorithmically generated levels was a constraint I set up and decided to maintain for that project a while ago. However, there were also others very important constraints that I decided to maintain in order to maintain the minimalistic intention for the game. On november 1st, I listed some of these constraints, namely:

- Static, completely and exhaustively visible interface in one screen
- One interactive object, one interaction mechanic, one objective
- No movement
- Discreet positioning of game objects
- Algorithmically generated levels
- No text 
- No representation of outside, real world, "objects"

Intuitively, I figured that this small list of constraints would reign in the size of the possibility space of the design of this game while still keeping it rich/interesting enough

Keeping that list of contraints in mind also helped with staying in the right mindset to be able to call the game done. With this list of minimalistic constraints basically managing what is possible to include/work on in the game, it's easy(er) to see and feel when your game is done. Other/extra features will (more) easily feel like distinct "other games"

### Constraints and possibility spaces

Each of these constraint brought in a challenge of its own for the design of the game:

- A static, wholy complete interface introduced a limit on the size of levels
- A singular mechanic made made it challenging to make the game dynamic and interesting through the progression of its levels
- No movement made it really hard to communicate affordances through limited feedback and feedforward
- Same with no text and no outside object representation

Algorithmically generated levels was definitely the one that required the most, mainly technical, work. Mostly because I had to design rules for how mechanics and features were introduced along and through increasing levels of difficulty. This is what took up so much of my time while developping this game

I did manage to make it work though, as this constraint was very important to me since, by maintaining it, I satisfied the most important aspect of a minimalist design project: the exaustiveness of the exploration of its design space

By having an algortihm "design" levels for me according to a complex set of rules, as opposed to crafting levels manually one by one, I'm basically allowing the game to virtually explore its level design space for me.

The issue with minimalism, however, is that possibility spaces can be different for different people. In other words, a game can be minimalistic from the designer's point of view, and it can be minimalistic from the player's point of view. Both these points of view have their own possibility spaces, spawning from their own sets of constraints.

From the design POV, I'm finding that constraints lead up to a satisfying and interesting (creative) exploration of a design space. However, is that equally true for the players? Do players enjoy the exhaustiveness of a highly constrained play space? Can an exhaustive and highly constrained play space be compelling enough to call for play? For how long? Does the length of time a game is interacted/played with truly matter? 

Also, players learn about and interpret constraints throughout their experience of the game, maintaining a mental model of the game's systems and constraints that might (and will) be incomplete and imperfect, but will also evolve through time. How, then, can we design a game that keeps a set of *evolving, interpreted* constraints to a minimum? How can we keep that evolution and interpretation to a minimum? 

### Abstraction as a tool

A general hypothetical theoretical direction to follow to maybe start answering these questions resides in the concept of abstraction. All games are basically born from layering levels of abstraction onto a real-world activity, operation, or action. Game design is about enabling interaction through these layers of abstraction by introducing interesting and playful affordances, signifiers and feedback. I feel like minimalist game design is simply an extention of that concept, brought to its extreme. Layering extra levels of abstraction to the point of the designed interaction being completely divorced from an "outside" relation or association to a "real world" activity or operation

It's either: layer so many levels of abstraction on top of a "real world" inspired action so that action completely disappears from view. And then work to make the resulting interaction playful and fun

Or: craft a system of interaction that, from the ground up, has no possibility of connection to a real work activity or system

Perhaps, then, with a truly minimalist game system, truly and completely abstracted from the real world, we can introduce a set of constraints so simple that it can't get interpreted or evolved out of its minimalism possibility space

### Designing vs. playing a game

The tension between the design possibility space (design space) vs. the play possibilitiy space (play space) might also be the reason behind this new dichotomy: I often say that I enjoy designing/creating the games I make much more than actually playing them. This surprises a lot of people but I think it follows very logically from this reflection: I enjoy the thorough exploration of an  possibility space and, by *designing* a game, I am objectively and entirely exploring its systems. *Playing* the game is another, different way to explore it and its systems, but it bores me because that's not the way I choose to explore it. Me designing the game is basically preparing the exploration of its systems for and by other people: players. 

Players don't have access to the objective, abstract, conceptual systems behind the mechanical functionings of the game. I write code and rules so that players have a front facing, playful and approachable access to the game's system, so we can both interact with the game (with my ideas) from different POVs. But the POV I'm preparing for the player is usually bound to bore (or even annoy) me, as it's represented by an imperfect, slower interface filled with possibilities for errors and misinterpretations 

This also explains why I often try and gather feedback about the game by showing a screenshot of its interface, as opposed to having people actually play it. I think I want people to get the game (or at least part of the game, the main idea, the system I have in mind, or at least the part of the system that's represented) through a quick look at its main interface. *Playing the game*, to me, sometimes feel like extra work that's unnecessary to get the system(s) that's behind it

This also might explain why I'm growing into a tendency to make games that are simply compeling and functional enough to convey the idea behind an interactive system, as opposed to making "fun"/robust/replayable games. I just want people to get the idea behind this system. 

Unfortunately, in order to acheive that feeling of "getting the idea" behind a game, it's actually necessary to have a somewhat robust, fun enough, motivating enough, bug free software to play with. Also, I do feel that, beyond getting the idea behind the game, seeing people continue playing my game is extremely satisfying and gratifying. So this does motivate me to create interfaces that go beyond being compeling, but also are fun and rich enough to keep the attention of players for a relatvely extended length of time

