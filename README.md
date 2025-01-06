# High-performance N-body simulation

[ApplyVelocitySystem.cs](https://github.com/naosea/gravity-sim/blob/main/ApplyVelocitySystem.cs) is where the most interesting code is. This project contains just the C# scripts for an optimised N-body gravity simulation in Unity. The other files were just generic unity project files and not interesting to look at.

While there are O(n log n) [approximation algorithms](https://en.wikipedia.org/wiki/N-body_simulation#Calculation_optimizations), this project was focused on CPU code optimisation rather than making the most efficient sim, so I went with the simple way of calculating the total gravitational force and velocity applied to each particle by each particle which is O(n^2).

https://github.com/user-attachments/assets/21e82a5d-ad03-4101-830b-f3614f52d831
## Optimisations made:
1. Use of data-oriented design. Programming in accordance to Unity's Data-Oriented Technology Stack (DOTS) to layout the data for each particle in contiguous memory for efficent CPU cache use.
2. Parallelism. Unity DOTS can also handle multithreading and calling of jobs with specific batch sizes.
3. Reading and writing to separate arrays and then swapping pointers to avoid false sharing.
4. Vectorization for SIMD execution of square roots.
5. A multiplication mask of 0/1 for the skipped indexes.
6. More that didn't make the final cut.

Sadly I didn't find a way to vectorize the inner batch loop. The branching prevents auto vectorization and I couldn't find a way to remove the branch preventing the force calculations between a particle and itself, if I had then the other branch could have been removed by unrolling the final iteration.

## Results
I regret not recording the performance with each change I made, including the ones that seems to make it slower, but as I'm not going to redo it all now, I've whipped up a quick comparison in Python that I ran in Google Colab. I feel this is a fair comparison because a Python notebook is what I'd typically use if I were curious about something, and Unity has a bunch of other stuff going on as well, e.g. rendering.

The fps of the high performance sim with 5000 particles was similar to that of the Python sim with 31 with about 9ms per frame.

That is a 2,500,000% speed up!!!
