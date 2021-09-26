# Morph
Mesh Morpher

This is a Unity project to morph an object between two mesh procedurally. The idea is to use procedural mesh generation and to change the mesh between two different shapes.

The basic method is to find the vertices of the old mesh and correspond them to the new mesh so that the whole mesh can change to and fro between the two by roughly moving from each positions.

Updated:
Two simultaneous mesh morph is done with new and old mesh and vice versa so that the morph can be smoother. The meshes will fade in and fade out parallel to each other. At the end, the reverse morph will be destroyed.
