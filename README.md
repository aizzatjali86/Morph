# Morph
Mesh Morpher

This is a Unity project to morph an object between two mesh procedurally. The idea is to use procedural mesh generation and to change the mesh between two different shapes.

The basic method is to find the vertices of the old mesh and correspond them to the new mesh so that the whole mesh can change to and fro between the two by roughly moving from each positions.

Additionally, some issue arises when the different between the number of vertices for the target mesh is too low compared to the original. One solution is to reverse the correlation of vertices so that the target will always have the larger number of vertices. Then, replace the final mesh with the target mesh at the end of the morph.
