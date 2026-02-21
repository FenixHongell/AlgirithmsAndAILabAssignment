# Implementation Document
This document outlines the project structure.

## Project Structure

All user relevant project files are located in the `/assets/` folder.

### `Assets/Src/`
The `src` directory contains all the code for the dungeon generation algorithm. 
- `Config`
  - Contains the class used to define the config **Scriptable Object**.
- `Globals`
  - Contains the Globals class, which defines shared constants used across the project.
- `scripts`
  - Contains the core implementation and algorithms for dungeon generation:
    - **Delaunay.cs**
      - Implements the Delaunay triangulation algorithm, used to establish connections between generated rooms. 
    - **DungeonGenerator.cs**
      - Contains the core logic responsible for generating the dungeon.
    - **Graph.cs**
      - Defines graph-related data structures and utilities used during generation. 
    - **Grid.cs**
      - Manages the grid system on which rooms are positioned and validated.
    - **Pathfinder.cs**
      - Implements the A* pathfinding algorithm for corridor generation between rooms. 
    - **PrimsAlgorithm.cs**
      - Implements Prims algorithm to compute the Minimum Spanning Tree from the generated graph.
### `Assets/Test/`
The `Tests` directory contains all the tests for the project.

## LLM Usage
LLM Usage can be find in `LLMUsage.md`

## Sources
- https://vazgriz.com/119/procedurally-generated-dungeons/