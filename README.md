# AmeisenBot - WoW 3.3.5a Bot 

⚠️⚠️⚠️ This Project is deprecated, this is the rewrite of it >> https://github.com/Jnnshschl/AmeisenBotX ⚠️⚠️⚠️

A bot written in (at this time) C# only for World of Warcraft WotLK (3.3.5a 12340) (the best WoW :P).
This project will be developed like "Kraut und Rüben" (Herb and beet?) as we say here in Germany, its a synonym for messy, so deal with it 😎.

⚠️ Currently this thing is only a playground for me to try out memory-hacking related stuff, but maybe it turns into something useable in the near future...

Docs: https://ameisenbot-335a.readthedocs.io

## Credits

❤️ **Blackmagic** (Memory Editing) - https://github.com/acidburn974/Blackmagic

## Features

My main focus regarding this bot is to make it a Party/Raid assistant that does specific things on its own.

* Base
    * Auto Revive/Run to the corpse
* Additional    
    * Change/Manage realmlists
    * Auto login
    * Auto launch WoW
    * Auto limit WoW's FPS
    * Auto set ulow GFX settings
* Party/Raid
    * Combat
        * DPS
        * Healing
        * *Tank (not tested)*
        * See below for currently supported specs/classes
    * Following
    * Auto accept party invite
* Navmesh based movement
    * See [AmeisenNavigation](https://github.com/Jnnshschl/AmeisenNavigation)
* Inventory/Equipment
    * Inventory/Equipment management
    * Auto equip better items
    * *Auto roll on items (W.I.P)*
* AI stuff
    * Remember units and what the bot can do at this unit
    * Auto repair equipment
        * Use the mammoth to repair if the bot got it
    * Auto sell grays from bags
    * Do random emotes when at idle
* LUA
    * Execute LUA
    * Capture WoW's events

Supported Classes:

**Class** | **Spec A** | **Spec B** | **Spec C**
------------ | ------------- | ------------- | -------------
**Druid** | ❌ Balance | ❌ Feral | ✔️ Restoration 
**Hunter** | ✔️ Marksmanship | ❌ Beast Mastery | ❌ Survival
**Mage** | ✔️ Fire | ❌ Frost | ❌ Arcane
**Paladin** | ❌ Holy | ✔️ Retribution | ❌ Protection
**Priest** | ✔️ Holy | ❌ Discipline | ✔️ Shadow
**Rogue** | ✔️ Combat | ❌ Assasination | ❌ Sublety
**Shaman** | ❌ Elemental | ❌Enhancement | ❌ Restoration
**Warlock** | ✔️ Affliction | ❌ Demonology | ❌ Destruction
**Warrior** | ❌ Arms | ✔️ Fury | ❌ Protection
**Death Knight** | ❌ Blood | ❌ Frost | ❌ Unholy

## Usage

Although i don't recommend to run this thing in this stage, **you can do it!**

🕹️ **How to use the Bot:**
Compile it, Start it, profit i guess?

🌵 **How to enable AutoLogin:**
Place the "WoW-LoginAutomator.exe" in the same folder as the bot, thats all...

🔪 **How to make a CombatClass:**
Template \*.cs file:
```c#
...
```

## Modules
**AmeisenBot.Combat**: CombatClass utilities & template

**AmeisenBot.Core**: Collection of some static object-reading/casting/lua functions

**AmeisenBot.Data**: "DataHolder" to hold things like our playerobject, our target object & active WoWObjects

**AmeisenBot.DB**: Database connection manager, from this thing the map is beeing read/saved

**AmeisenBot.FSM**: StateMachine of the Bot executing all actions

**AmeisenBot.GUI**: WPF GUI

**AmeisenBot.Logger**: Basic logging class

**AmeisenBot.Manager**: Create a new Bot instance here and manage it

**AmeisenBot.Test**: Maybe some tests will appear in this module in the near future

**AmeisenBot.Utilities**: Memory offsets, data structs and a few math related funtions

**AmeisenPathLib**: Pathfinding using A*

**WoWLoginAutomator**: Auto-Login into WoW 3.3.5a

## Screenshots

**Maybe outdated!**

### Character Selection Window

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/charselect_auto.PNG?raw=true "Character selection Autologin")

### Main Bot Screen

![alt text](https://github.com/Jnnshschl/WoW-3.3.5a-Bot/blob/master/images/mainscreen.PNG?raw=true "Mainscreen")
