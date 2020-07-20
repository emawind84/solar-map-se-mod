# solar-map-se-mod
SolarMap Space Engineers Mod

R e a d m e
-----------

This is a modification of the mod made by Skimt that you find here
https://steamcommunity.com/sharedfiles/filedetails/?id=2165264228

You find a guide of how to set up the planet list in the original description of the mod, here I add the settings you can add to the Programmable Block and LCDs to change the look of the map.

Changes compared to the original mod:
- You can set the location of the star using in game gps coordinates.
- You should be able to change the color of text and background.
- You can add more planets to the list covering the entire world and they should be correctly placed on the map
- You can toggle the info panel
- You can show the grid name near the current position red dot or arrow
...


The following settings can be added to the Custom Data of the block:

Programmable Blocks:

	[SolarMap]
	DebugDisplay=0 // surface where to display debug data
	StarRadius=100000
	StarPosition=GPS:Sun:0:0:-3000000: // star position in game (GPS format from the game)

* You need to recompile the script to update the settings

LCD block or terminal with multi LCD:

	[SolarMap]
	Display=0 // surface where to display the map
	DisplaySun=true // show the sun
	DisplayInfoPanel=true // show the info panel
	DisplayGridName=true // show the grid name near the red dot
	DisplayOrbit=true // show planets orbit
	StretchFactor=1 // stretch the map on x axis if too wide