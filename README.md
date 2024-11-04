# QBertLevelGenerator

## Overview

The "QBertLevelGenerator" is a procedural generation algorithm designed to create Q*Bert-inspired levels. These levels follow a pyramid-like grid structure where each block can contain items, obstacles, or spawn points for the player and goal. The unique design philosophy of this algorithm focuses on ensuring a **solvable path** from the player's start to the goal (a black sphere) by laying out the critical path first. This approach guarantees level solvability, which is essential in procedurally generated games.

## Q*Bert Level Layout

In a Q*Bert-inspired game, levels are organized in a staggered, triangular grid, where each block connects diagonally with neighboring blocks. This distinct layout sets up a playable area with strategic pathways that guide player movement.

![Insert Screenshot 1: An example of the empty Q*Bert grid layout to introduce the structure.]*

## Key Components of the Algorithm

The generation process is managed by three main scripts: **BlockGridCreator.cs**, **Block.cs**, and **ProceduralGenerator.cs**. Each plays a specific role in constructing and populating the level.

### 1. Building the Grid Layout with `BlockGridCreator.cs`

The `BlockGridCreator.cs` script is responsible for creating the level’s grid structure. Using adjustable grid dimensions (`gridSizeX` and `gridSizeY`) and block spacing (`blockWidth` and `blockHypotenuse`), it arranges blocks in a staggered layout that mimics a pyramid. This layout forms the basis of the Q*Bert-style gameplay space.

- **Parameters:**
  - `gridSizeX` and `gridSizeY`: Define the grid’s width and height.
  - `blockWidth` and `blockHypotenuse`: Control the spacing between blocks, creating the staggered layout.

**Process:**
- Each block is placed in rows with staggered columns to form the pyramid layout.
- Jump vectors (`LeftJumpVector` and `RightJumpVector`) are calculated, guiding player movement directions.

![Insert Screenshot 2: The grid created by `BlockGridCreator.cs`, showing how blocks are positioned in a pyramid structure.]*

### 2. Defining Block Properties with `Block.cs`

Each block within the grid is defined by `Block.cs`, which assigns properties such as state (empty or filled) and potential contents (gems, spheres). Blocks know their own position (`row` and `col`), any items they hold, and connections to neighbors, allowing for flexible gameplay interactions.

- **Key Properties:**
  - **row and col:** Position in the grid.
  - **isEmpty:** Indicates whether the block is empty or filled.
  - **hasBlackGem, hasRedGem, etc.:** Tracks which items are present on the block.

**Process:**
- Each block’s state and contents are dynamically managed.
- Visual effects and animations are applied as necessary, enabling elements like gem animations.

![Insert Screenshot 3: Example of blocks with various properties set, showing items like gems or spheres on different blocks.]*

### 3. Laying Out the Solvable Path with `ProceduralGenerator.cs`

The `ProceduralGenerator.cs` script is the core of the level’s procedural generation. Its design philosophy emphasizes **ensuring solvability** by laying out a critical path before filling the remaining blocks with randomized elements. This reverse approach—where the critical path is established first—was chosen to avoid the complexity of validating randomly generated layouts for solvability, a step that would have been outside the project’s scope.

#### Critical Path-First Approach

1. **Starting with the Goal in Mind:** The algorithm begins by selecting a starting block and then creates a path towards the black sphere (the goal). This ensures that a solvable route is always in place.
2. **Path Construction:** Using the `walkerSteps` parameter, a path is generated step-by-step from the starting block to the goal, ensuring continuous movement through adjacent blocks.
3. **Populating the Remaining Grid:** After laying out the critical path, additional blocks are filled based on the `fillMapNumber` parameter, adding complexity without interfering with the critical path.

- **Key Variables:**
  - **walkerSteps**: Defines the length of the critical path.
  - **fillMapNumber**: Controls the number of additional filled blocks after the path is set.
  - **Item Prefabs** (e.g., `blackGemPrefab`, `redGem`, `redSpherePrefab`): Prefabs for items that populate the grid, adding variation to each level.

**Process:**
1. **Critical Path Creation:** The algorithm lays out the critical path first, ensuring the player can always reach the goal.
2. **Random Block Filling:** Additional blocks are filled randomly, adding challenge and interest without obstructing the main path.
3. **Item Placement:** Items are added to various blocks, creating gameplay elements and enhancing player interaction.

![Insert Screenshot 4: The grid after generating the critical path, highlighting the direct path from the start to the goal.]*

![Insert Screenshot 5: The fully populated grid with additional blocks and items, demonstrating the final level structure.]*

### Example Levels and Parameters

Below are examples of levels generated with different values for `walkerSteps` and `fillMapNumber`, showing the flexibility and scalability of the algorithm.

1. **Short Path, Sparse Grid**

   - *Parameters:* `walkerSteps = 3`, `fillMapNumber = 2`
   - ![Insert Screenshot 6: A simple level with a short critical path and minimal additional blocks, suitable for beginners.]*
  
2. **Long Path, Dense Grid**

   - *Parameters:* `walkerSteps = 10`, `fillMapNumber = 15`
   - ![Insert Screenshot 7: A complex level with a long critical path and many additional blocks, offering a challenging experience.]*
  
### Advantages of the Critical Path Approach

- **Guaranteed Solvability:** By laying out the critical path first, the algorithm ensures that each level is playable without requiring additional validation checks.
- **Controlled Difficulty:** Parameters like `walkerSteps` and `fillMapNumber` allow for fine-tuning the level’s complexity and challenge.
- **Enhanced Replayability:** Randomized elements, such as block filling and item placement, ensure each level remains fresh and engaging.

### Future Enhancements

To further improve the generation process, the following features could be considered:

- **Dynamic Difficulty Adjustment:** Adjusting `walkerSteps` and `fillMapNumber` based on player performance.
- **Branching Paths:** Adding optional paths for exploration beyond the critical path.
- **Themed Blocks:** Introducing different block types and visual themes as levels progress.

By focusing on a critical path-first approach, the "QBertLevelGenerator" achieves a balanced solution between uniqueness and playability, exemplifying a robust design for procedural level generation.
