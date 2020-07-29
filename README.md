# Laser Power Test
A small command line application that generates laser power gradiants at various feedrates.

Run LaserPowerTest.exe and enter in the paramaters desired, or press enter for the default, then run the resulting file on your GRBL based laser machine.

# Positioning
The program that will be created starts with a G92 block, making where ever the machine is currently postioned be the new "zero".

Jog the tool to the lower left corner of where you want the test to start doing it's gradients. The program will go slightly negative to draw its borders and markings. You will need aproximately -1 unit in X from the origin, and -5 units in Y (leave some material to the left and below).
