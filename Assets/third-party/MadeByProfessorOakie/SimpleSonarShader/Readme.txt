=====================
How to Use
=====================

You must choose between the two systems for setting it up:
- Object
	- every object with the sonar effect has a script on it
	- objects all share the same sonar waves
- Parent-Child
	- every object with the sonar effect must be a child of a sonar parent
	- all children share the same sonar waves


=============
Object Setup
=============
1. Add the SimpleSonarShader_Object.cs script to all objects you want to have the effect.
2. Set the SimpleSonarShader.shader as the shader for the material on all objects you want to have the effect.
3. By default, these objects will spawn rings OnCollisionEnter.


===================
Parent-Child Setup
===================
1. Make all objects you want to have the effect be children of a single game object.
2. Add these scripts to that single parent game object: SimpleSonarShader_Parent.cs, SimpleSonarShader_ExampleConfigureChildren.cs 
3. By default, these objects will spawn rings OnCollisionEnter.
