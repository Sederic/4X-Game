# 4X-Game
 A 4X Game 

## My changes
* `MultidimensionalArray.cs` -- custom converter for `[,] _world` array
* `GameTile.cs` has simple constructor (`GameTile()`), UID w/ autoincrement, and `Dictionary<int, GameTile> _gameTileRegistry` for lookup by UID
* `Settlement.cs` -- addition of `_gameTileUIDs` for settled location and `_territoryUIDs`, `_workedTileUIDs`, `_lockedTileUIDs` UIDs for serialization

### Notes
- I think I touched other files it's in the commits. But some minor stuff like removed `abstract` from `Unit` class bc it was preventing it from serialization
- Also some other classes missing simple/basic constructor w/ no params required for auto-deserialization w/ Newtonian
- tested saving/loading w/ 1 city settled (or not) and works

### Does not save
* `Settlement.cs` -- projects
* `Civilization.cs` -- `TechnologyTree`

Reasoning is they both don't have functionality at the moment, (so hard to test), and I feel there might be a need to change these data structures later on, eg TechnologyTree is not a tree data type.
