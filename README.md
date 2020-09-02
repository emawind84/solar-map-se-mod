R e a d m e
-----------

# ED's Star System Map

## Star System Map - A Space Engineers Mod


This is a modification of the mod by Skimt that you find at the link below
https://steamcommunity.com/sharedfiles/filedetails/?id=2165264228

You find a guide of how to set up the planet list in the original description of the mod, here I add the settings you can add to the Programmable Block and LCDs to change the look of the map.

### New features:

- The Star location can be set using in game gps coordinates format.
- Text and background colors can be changed from the block setting.
- The map is a 2d rappresentation of the entire world (you can have planets with negative coordinates and it should works just fine)
- The Info panel on the left can be hidden
- The grid name can be displayed near the red dot, representing the current location of the grid.
- Display debug log on any display and multi display block


### The following settings can be customized, adding the following code to the Custom Data of the block:

**Programmable Blocks**

	[SolarMap]
	DebugDisplay=0						// Surface where to display debug data
	StarRadius=100000					// Star radius (try not to exagerate here, is more like a red dwarf)
	StarPosition=GPS:Sun:0:0:-3000000:	// The Star position in game (Use game's GPS format)

	*You need to recompile the script to update the settings


**Display block or terminal with multiple screens:**

	[SolarMap]
	Display=0				// surface where to display the map
	DebugDisplay=0			// Surface where to display debug data
	DisplaySun=true			// show the sun
	DisplayInfoPanel=true	// show the info panel
	DisplayGridName=true	// show the grid name near the red dot
	DisplayOrbit=true		// show planets orbit
	StretchFactorH=1		// stretch the map horizontally, fraction allowed
	StretchFactorV=1		// stretch the map vertically, fraction allowed
	FollowGrid=1			// place the current grid at the center of the map
	CenterPosition=<GPS>	// Custom center position using GPS game format
	MapRadius=1				// Radius of the map in Km when using CenterPosition or FollowGrid option
	DisplayGPS=false		// Display GPS points (WIP)
	DisplayGrid=false		// Display a grid in the background
	PlanetScaleFactor=1		// Resize the planet by this number 
	StretchFactor=1			// [deprecated] stretch the map on x axis if too wide

**Sensors** and **cameras** can be used to detect nearby grids, by adding the tag `[SolarMap]` to the **CustomData** of these blocks.
The detected entity will be added as GPS point to the list of entities, in order to show these GPS points you need to add
`DisplaGPS=true` to the block where you want to see these points.


### Source code 
https://github.com/emawind84/solar-map-se-mod