# Todo

## 2024-09-02

~- Make an array and display a bunch of squares
~- Make the squares interactable
	~- Make them clickable
	~- Make them toggle other squares when clicked
~- Assemble an algorithm to propose a "goal arrangement" of squares and a number of clicks to acheive it
- Notify the play that they won if they acheive the goal arrangement of a level

## 2024-09-04

- Add a clicks counter 
	(should it be possible to beat the level but in more clicks? Giving partial rewards? What are rewards? When the reward is just being able to go to the next level, what could be a "partial reward"? If not, players shouldn't be able to click on more squares after clicks counter == solution clicks) 
	(also what if the solution is possble to achieve with fewer clicks? Extra rewards? Should compute the optimal number of clicks to get to a randomly setup solution)
- Undo/reset buttons
- Next level button (debug is in place)
? Debug button to show the solution? (implement this in player-facing interface?)
? Telegraphed effect of clicking a button? Predicting its interaction with its targets?
? Figure out preliminary heuristics for the generation of "interesting" levels and goals
? Test whether or not it's more interesting to let players click squares more than once and if not, implement that restriction and feedback for that feature