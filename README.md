roboPianist, Midi Piano Player Character with Autofingering
=============

When I watched Westworld in 2017, I decided to develop a character in unity who plays the piano according to midi notes with correct fingering. 
I started with great enthusiasm, but I did not have time to continue. 
Now I am sharing the code as open source. I hope someone completes it. 

Unity Version:  2020.3.14

detailed informatin in my artstation page 
https://www.artstation.com/blogs/emrecancubukcu/jobw/making-of-robotpianist-midi-piano-player-character-with-autofingering


How To Use
=============

- copy your midi file to resources/midi and rename and add txt ( ex: westword.midi  => westword.midi.txt ) 
- in midi file, left and right hand track should be seperate.
- Midi programs of left/right track  ( enstrumant no )  have to be 1 for left hand, 0 for right hand


TODO & ISSUES
=============
- some systems midi notes cannot mute
- Develop a simple fingering algorithm
- Philip Abbet's auto-finger code to my c# port bugfix ( working correct, only first midi file in projects :( )
- Wrist rotation calculation
- IK reaching problems
- Smart head and shoulder movement
- Different playing styles of the characters ( laziness, passion, fatigue, etc.)
- solve note pressing duration & glitch bug
- try this, https://github.com/marcomusy/pianoplayer
- try with learning AI systems
- documentation, and tips on Unity3d editor

CREDITS
-----------------------------------


Chad Carter's Unity port  https://github.com/kewlniss/CSharpSynthForUnity

Philip Abbet's python implementation, https://github.com/Kanma/piano_fingering

Nora's Greatest full-body IK, https://github.com/Stereoarts/SAFullBodyIK

Zombie model, www.mixamo.com

Other models created in Adobe Fuse

Midi files, https://www.midiworld.com/

