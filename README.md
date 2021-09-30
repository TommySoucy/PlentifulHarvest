# Plentiful Harvest

A mod for Potion Craft that gives the ability to use Growth and Harvest potions on your own garden. Also lets you change values for how many ingredients you get when harvesting plants and how many products you get from the alchemy machine.

Using a growth or harvest potion is as simple as dropping it in the garden. After a few seconds, a growth potion will make plants grow. The amount is dependent on the strength of the potion. Whenever plants are grown, that it be through sleeping or using a growth potion, if a harvest potion is also present in the garden, it will be used to increase the amount of ingredients you get from the plants.
An unlimited amount of potions can be used at once.

## Installation

1. Download latest from [releases](https://github.com/TommySoucy/PlentifulHarvest/releases)
2. Download the latest BepinEx package corresponding to your operating system from [here](https://github.com/BepInEx/BepInEx/releases) and extract all files from the zip into your Potion Craft installation
3. Run the game once for BepinEx to generate its file system
4. Put all files from the .zip of this mod into BepinEx/Plugins folder

## Config

There are some settings available in the provided config file described below:

- **_harvestIngredientMultiplier_**: Default is 1, which has no effect. This setting will be used as a general multiplier for the amount of ingredients you get when harvesting a plant. It will be applied on top of the bonus you get from using a harvest potion.
      
- **_harvestExperienceMutliplier_**: Default is 1, which has no effect. This setting will be used as a multiplier for the amount of experience you get when harvesting a plant.
      
- **_All the other count multipliers_**: Default is 1, which has no effect. These settings will decide the amount of the corresponding substance you get from the alchemy machine.

- **_growthDelay_**: This setting will decide how long, in seconds, after dropping a fast growth potion in the garden will it trigger growth.

- **_growthIncrement_**: This settings will decide how many plants should grow minimum per level when using a fast growth potion in the garden. For example, if set to 2, when using a weak growth potion, you'll get a minimum of 2 plants. For a normal potion you'll get minimum 4 and for a strong potion you'll get minimum 6.

- **_harvestIncrement_**: This settings will decide how many ingredients more you will get from each level of harvest potions. So if set to 1 while using a weak hearvest potion, you will get +1 to amount of ingredients harvested from each plant. If you use a normal potion you will get +2, and if you use a strong potion, you will get +3.

## Building

1. Clone repo
2. Open solution
3. Ensure all references are there
4. Build
5. DLL is now ready for install as explained in **Installation** section

## Used libraries

- Harmony
- BepinEx
