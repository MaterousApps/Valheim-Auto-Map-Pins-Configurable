# AMPED - Auto Map Pins Enhanced
Author: JordanRichmeier
Nexus: [AMPED - Auto Map Pins Enhanced](https://www.nexusmods.com/valheim/mods/2199)
Source: [Github](https://github.com/raziell74/Valheim-Auto-Map-Pins-Configurable/)

Automatic pinned resources, pickables, locations, spawners, and creatures to your map and minimap.

This is an enhancement on Materous' AutoMapPins with Configuration and is very heavily based off of his work. So far all I've done is overhaul the configuration of his mod the rest of the code for pinning is still from his source code, save for a few speed optimizations. 

Pin Types now have their own json file that details what is pinned, the label and icon to use, and the icon size. Icons are also in their own folder and can be changed around, added to, or completely removed. Everything was designed to be modified with out the need to update the DLL every time Iron Gate adds something new. 

## Credits
Contibutions from the following modders was invaluable and appreciated: 
  * [Materous](https://www.nexusmods.com/valheim/users/6021662) - for the original mod and providing access/permission to the source code
  * [LuxZg](https://www.nexusmods.com/users/1014505) - For doing all the leg work in adding a ton of new pins for various structures as well as new mistland objects

# Change Log

### Version v1.3.5
  * 

### Version v1.3.4
  * New config for hiding pin types by label has been added. Users can now entered a comma seperated list of pin labels that they don't want to be autopinned
  * Multiple pin type configuration json files can now be loaded. Opening up the possibility of other authors to release their own pin and icon packs
  * Pin Config file has been made dynamic. AMPED will now load any json file with the prefix 'amp_' and attempt to use it as a pin type config file
  * Overhauled sprite loading to instead search the plugins folder and use the first file that matches the file name
  * Refactored some internal code to be more maintainable

### Version v1.3.3
  * Added new pins
  * New Pin: Infested Tree (Guck Trees)
  * New Pin: Road Post
  * New Pin: Dvergr Tower
  * New Pin: Statues
  * New Pin: Ruins
  * New Pin: Stone Tower
  * New Pin: Runestone
  * New Pin: Giant Remains (petrified remains and soft tissue)
  * New Pin: Stone Tower
  * New Pin: Log Cabin
  * New Pin: Infested Mine
  * New Pin: Dvergr Exavation
  * New Pin: Swamp Hut
  * New Pin: Swamp Tower
  * New Pin: Giant Armor (Giant sword/helmets)
  * New Pin: Wood House
  * New Pin: Stone Circle
  * New Pin: Well
  * New Pin: Dolmen
  * New Pin: Fuling Tower
  * New Pin: Harbour
  * New Pin: Viaduct
  * New Pin: Stone Tower
  * New Pin: Shipwreck
  * New Pin: Farm House

### Version v1.3.1/1.3.2
  * Fixed an error with r2modmanager not finding assets

### Version v1.3.0
  * Overhauled configuration of map pin types 
  * Speed optimizations for loading in pins
  * Ability to toggle logging. Users can now enable the logging to print out object id's that come within range of the player so that players can add custom map pins to the new amp_pin_types.json file and pin what ever they would like, even provide their own custom icon for it. 
  * New Pins added: Mountain Caves, Tar Pits, Fuling Camps, Jotun Puffs, Mage Caps, and Black Cores
  
## TODO

  * New UI interface on the world map that will give users the ability to easily hide, toggle labels, set detection range, and save pins
  * Ability to pin Prefabs (user constructed objects): Carts, Boats, Beds, Portals, etc...
  * Option for pins to be added "globally" with no range limit. This will only apply to custom constructed objects since in game spawned objects won't be in existence yet at world load.
  * Include Auga UI compatibility
  * Separate pin sizing options for the world map and the minimap
  * Label font sizing per pin type
  * Add a separate json file for custom user pins, this is so people can put out their own icon and pin packs without having to worry about my updates overriding their changes to amp_pin_types.json 
  * Live tracking of creature pins (could be FPS intensive)
  * Automatic unpinning: when pickable has been picked, after ore has been completely mined, after a location has been destroyed.
  * Allow users to pick from any loaded pin-icon image to make their own manual pins. 
