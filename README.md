# QBertLevelGenerator

## Case Study: Critical Path-Based Procedural Level Generation

### Objective
The "QBertLevelGenerator" algorithm aims to procedurally generate game levels that are unique, replayable, and **guarantee a valid path** from the player’s starting position to the goal (the black sphere). By prioritizing the creation of a critical path before populating the rest of the level, this algorithm ensures that every generated level is solvable, enhancing player satisfaction and gameplay experience.

### Algorithm Overview

1. **Critical Path Generation:**
   - **Starting Point Selection:** The algorithm begins by selecting a random starting block on the grid for the player to spawn.
   - **Path Construction:** Using the `walkerSteps` parameter, it generates a critical path from the starting block to the goal by moving step-by-step in a random direction (left or right).
   - **Goal Placement:** The last block in the critical path is designated as the goal location (the black sphere).

2. **Grid Population:**
   - **Filling Empty Blocks:** After establishing the critical path, the algorithm fills additional blocks based on the `fillMapNumber` parameter, adding complexity and variety to the level without obstructing the critical path.
   - **Item and Obstacle Placement:** Random items (like red gems, red spheres, and black spheres) are placed on filled blocks to enhance gameplay dynamics.

3. **Ensuring Solvability:**
   - By generating the critical path first, the algorithm avoids the common procedural generation issue where random levels may be unsolvable.
   - This method guarantees that the player can always reach the goal, providing a consistent gameplay experience.

### Key Components and Variables

- **`walkerSteps` (int):** Determines the length of the critical path. A higher number results in a longer path from the player to the goal.
- **`fillMapNumber` (int):** Specifies how many additional blocks to fill after the critical path is established, impacting grid density and obstacles.
- **`Block.cs`:** Manages individual block properties and behaviors, such as position (`row`, `col`), state (`isEmpty`), and contents (gems, spheres).
- **`BlockGridCreator.cs`:** Handles the creation and arrangement of the grid based on `gridSizeX` and `gridSizeY`, setting up spatial relationships between blocks.
- **`ProceduralGenerator.cs`:** Implements the critical path algorithm and populates the grid with additional blocks and items.

### Visual Representation

To better illustrate the algorithm's functionality, consider the following stages of level generation:

1. **Initial Grid with Empty Blocks**

   ![Insert Screenshot 1: An empty grid showcasing all blocks in their initial state before generation.]

2. **Critical Path Creation**

   ![Insert Screenshot 2: The grid highlighting the critical path from the player's starting block to the goal block (black sphere).]

3. **Populating Additional Blocks**

   ![Insert Screenshot 3: The grid after additional blocks have been filled based on `fillMapNumber`, showing increased complexity.]

4. **Final Level with Items and Obstacles**

   ![Insert Screenshot 4: The completed level with all items (gems, spheres) placed, ready for gameplay.]

### Advantages of the Critical Path Approach

- **Guaranteed Solvability:** By constructing the path first, the algorithm ensures that every level is playable without needing complex validation checks after generation.
- **Controlled Difficulty:** Adjusting `walkerSteps` and `fillMapNumber` allows for fine-tuning the level’s difficulty and complexity.
- **Enhanced Replayability:** While the critical path remains a central feature, the random placement of additional blocks and items ensures that each level feels fresh and unpredictable.

### Example Levels

Here are examples of levels generated with different input variables:

- **Short Path, Sparse Grid**

  - *Parameters:* `walkerSteps = 3`, `fillMapNumber = 2`
  - ![Insert Screenshot 5: A simple level with a short critical path and minimal additional blocks, suitable for beginners.]

- **Long Path, Dense Grid**

  - *Parameters:* `walkerSteps = 10`, `fillMapNumber = 15`
  - ![Insert Screenshot 6: A complex level with a long critical path and many additional blocks, offering a challenging experience.]

### Future Enhancements

- **Dynamic Difficulty Adjustment:** Implementing a system to adjust `walkerSteps` and `fillMapNumber` based on player performance could further enhance gameplay.
- **Alternative Pathways:** Introducing branching paths off the critical path could add exploration elements and increase replayability.
- **Visual Variety:** Incorporating different block types and visual themes as levels progress can maintain a fresh aesthetic experience.

By focusing on a critical path-first approach, the "QBertLevelGenerator" effectively addresses common challenges in procedural generation, resulting in a robust and engaging level creation system.
