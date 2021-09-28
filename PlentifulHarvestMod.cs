using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using ObjectBased.Garden.GrowingSpot;
using ObjectBased.InteractiveItem;
using System.Reflection;
using BepInEx;

namespace PlentifulHarvest
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class PlentifulHarvestMod : BaseUnityPlugin
    {
        // BepinEx
        public const string pluginGuid = "vip.TommySoucy.PlentifulHarvest";
        public const string pluginName = "Plentiful Harvest";
        public const string pluginVersion = "1.1.0";

        // Config settings
        public static int harvestIngredientMultiplier = 1;
        public static int harvestExperienceMutliplier = 1;
        public static int nigredoCountMultiplier = 1;
        public static int albedoCountMultiplier = 1;
        public static int citrinitasCountMultiplier = 1;
        public static int rubedoCountMultiplier = 1;
        public static int philosopherStoneCountMultiplier = 1;
        public static int saltCountMultiplier = 1;
        public static float growthDelay = 3;
        public static int growthIncrement = 2;
        public static int harvestIncrement = 1;

        // Live data
        public class GrowthTimer
        {
            public PotionItem potion;
            public int level;
            public float time;
            public GrowthTimer(PotionItem potion, int level, float time)
            {
                this.potion = potion;
                this.level = level;
                this.time = time;
            }
        }
        public static List<GrowthTimer> currentGrowthTimers;
        private static List<GrowthTimer> growthTimers;
        public static List<PotionItem> harvestPotions;
        public static int harvestCount = 0;

        public void Start()
        {
            Init();

            DoPatching();
        }

        private void Init()
        {
            growthTimers = new List<GrowthTimer>();
            currentGrowthTimers = new List<GrowthTimer>();
            harvestPotions = new List<PotionItem>();

            LoadConfig();
        }

        public void Update()
        {
            // Process growth timers
            for(int i=0; i < growthTimers.Count; ++i)
            {
                growthTimers[i].time -= Time.deltaTime;
                if(growthTimers[i].time <= 0)
                {
                    currentGrowthTimers.Add(growthTimers[i]);
                    for(int j=0; j < harvestPotions.Count; ++j)
                    {
                        if (harvestPotions[j].Equals(growthTimers[i].potion))
                        {
                            harvestPotions.RemoveAt(j);
                            break;
                        }
                    }
                    GameObject.Destroy(growthTimers[i].potion);
                    growthTimers.RemoveAt(i--);
                }
            }
            if (currentGrowthTimers.Count > 0)
            {
                GrowingSpotObject.GrowPlants();
            }
        }

        private void LoadConfig()
        {
            try
            {
                string[] lines = File.ReadAllLines("BepinEx/Plugins/PlentifulHarvestConfig.txt");

                foreach (string line in lines)
                {
                    if (line.Length == 0 || line[0] == '#')
                    {
                        continue;
                    }

                    string trimmedLine = line.Trim();
                    string[] tokens = trimmedLine.Split('=');

                    if (tokens.Length == 0)
                    {
                        continue;
                    }

                    if (tokens[0].IndexOf("harvestIngredientMultiplier") == 0)
                    {
                        harvestIngredientMultiplier = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("harvestExperienceMutliplier") == 0)
                    {
                        harvestExperienceMutliplier = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("nigredoCountMultiplier") == 0)
                    {
                        nigredoCountMultiplier = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("albedoCountMultiplier") == 0)
                    {
                        albedoCountMultiplier = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("citrinitasCountMultiplier") == 0)
                    {
                        citrinitasCountMultiplier = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("rubedoCountMultiplier") == 0)
                    {
                        rubedoCountMultiplier = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("philosopherStoneCountMultiplier") == 0)
                    {
                        philosopherStoneCountMultiplier = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("saltCountMultiplier") == 0)
                    {
                        saltCountMultiplier = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("growthDelay") == 0)
                    {
                        growthDelay = float.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("growthIncrement") == 0)
                    {
                        growthIncrement = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("harvestIncrement ") == 0)
                    {
                        harvestIncrement = int.Parse(tokens[1].Trim());
                    }
                }

                Logger.LogInfo("Configs loaded");
            }
            catch (FileNotFoundException ex) { Logger.LogInfo("Couldn't find PlentifulHarvestConfig.txt, using default settings instead. Error: " + ex.Message); }
            catch (Exception ex) { Logger.LogInfo("Couldn't read PlentifulHarvestConfig.txt, using default settings instead. Error: " + ex.Message); }
        }

        private void DoPatching()
        {
            var harmony = new HarmonyLib.Harmony("VIP.TommySoucy.PlentifulHarvest");

            // HarvestCountPatch
            var harvestCountPatchOriginal = typeof(SpotPlant).GetMethod("GatherIngredient", BindingFlags.NonPublic | BindingFlags.Instance);
            var harvestCountPatchPrefix = typeof(HarvestCountPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(harvestCountPatchOriginal, new HarmonyMethod(harvestCountPatchPrefix));

            // SpotPlantIngredientAmountPatch
            var spotPlantIngredientAmountPatchOriginal = typeof(GrowingSpotObject).GetMethod("InstantiatePlant", BindingFlags.NonPublic | BindingFlags.Instance);
            var spotPlantIngredientAmountPatchPostfix = typeof(SpotPlantIngredientAmountPatch).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(spotPlantIngredientAmountPatchOriginal, null, new HarmonyMethod(spotPlantIngredientAmountPatchPostfix));

            // AlchemyMachineProductCountPatch
            var alchemyMachineProductCountPatchOriginal = typeof(AlchemyMachineProductItem).GetMethod("PutInInventory");
            var alchemyMachineProductCountPatchPrefix = typeof(AlchemyMachineProductCountPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(alchemyMachineProductCountPatchOriginal, new HarmonyMethod(alchemyMachineProductCountPatchPrefix));

            // GardenDropPotionPatch
            var gardenDropPotionPatchOriginal = typeof(ItemFromInventory).GetMethod("ReleaseToPlayZone", BindingFlags.NonPublic | BindingFlags.Instance);
            var gardenDropPotionPatchPostfix = typeof(GardenDropPotionPatch).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(gardenDropPotionPatchOriginal, null, new HarmonyMethod(gardenDropPotionPatchPostfix));

            // GardenGrabPotionPatch
            var gardenGrabPotionPatchOriginal = typeof(ItemFromInventory).GetMethod("OnGrabPrimary");
            var gardenGrabPotionPatchPrefix = typeof(GardenGrabPotionPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(gardenGrabPotionPatchOriginal, new HarmonyMethod(gardenGrabPotionPatchPrefix));

            // GrowPlantsPatch
            var growPlantsPatchOriginal = typeof(GrowingSpotObject).GetMethod("GrowPlants");
            var growPlantsPatchPrefix = typeof(GrowPlantsPatch).GetMethod("Prefix", BindingFlags.NonPublic | BindingFlags.Static);
            var growPlantsPatchPostfix = typeof(GrowPlantsPatch).GetMethod("Postfix", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(growPlantsPatchOriginal, new HarmonyMethod(growPlantsPatchPrefix), new HarmonyMethod(growPlantsPatchPostfix));
        }

        public static void AddGrowth(PotionItem potion, int level)
        {
            growthTimers.Add(new GrowthTimer(potion, level, growthDelay));
        }

        public static void RemoveGrowth(PotionItem potion)
        {
            for(int i=0; i < growthTimers.Count; ++i)
            {
                if (growthTimers[i].potion.Equals(potion))
                {
                    growthTimers.RemoveAt(i);
                    break;
                }
            }
        }
    }

    class HarvestCountPatch
    {
        // This prefix modifies instance values before they are used by the original method
        static void Prefix(ref int ___experienceOnHarvest, ref Vector2Int ___ingredientAmount)
        {
            ___experienceOnHarvest *= PlentifulHarvestMod.harvestExperienceMutliplier;
            ___ingredientAmount *= PlentifulHarvestMod.harvestIngredientMultiplier;
        }
    }

    class SpotPlantIngredientAmountPatch
    {
        // This postfix modifies the ingredientAmount of a plant directly when it gets instantiated using harvestCount
        static void Postfix(ref GameObject ___plantGameObject)
        {
            SpotPlant plantInstance = ___plantGameObject.GetComponent<SpotPlant>();

            // Set private field ingredientAmount through reflection
            var ingredientAmount = plantInstance.GetType().GetField("ingredientAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            Vector2Int ingredientAmountVector = (Vector2Int)ingredientAmount.GetValue(plantInstance);
            int increment = PlentifulHarvestMod.harvestCount * PlentifulHarvestMod.harvestIncrement;
            ingredientAmountVector.x += increment;
            ingredientAmountVector.y += increment;
            ingredientAmount.SetValue(plantInstance, ingredientAmountVector);
        }
    }

    class AlchemyMachineProductCountPatch
    {
        // This prefix replaces the original function that gets called when you finish an alchemy machine product entirely
        static bool Prefix(AlchemyMachineProductItem __instance, ref Inventory ___inventory, ref ItemsPanel ___itemsPanel)
        {
            int multiplier = 1;
            LegendarySaltPile legendarySaltPile = __instance.inventoryItem as LegendarySaltPile;
            if (__instance.inventoryItem.name.Equals("Nigredo"))
            {
                multiplier = PlentifulHarvestMod.nigredoCountMultiplier;
            }
            else if (__instance.inventoryItem.name.Equals("Albedo"))
            {
                multiplier = PlentifulHarvestMod.albedoCountMultiplier;
            }
            else if (__instance.inventoryItem.name.Equals("Citrinitas"))
            {
                multiplier = PlentifulHarvestMod.citrinitasCountMultiplier;
            }
            else if (__instance.inventoryItem.name.Equals("Rubedo"))
            {
                multiplier = PlentifulHarvestMod.rubedoCountMultiplier;
            }
            else if (__instance.inventoryItem.name.Equals("Philosopher's Stone"))
            {
                multiplier = PlentifulHarvestMod.philosopherStoneCountMultiplier;
            }
            else if (legendarySaltPile != null)
            {
                multiplier = PlentifulHarvestMod.saltCountMultiplier;
            }
            else
            {
                // Item is not legendary substance or salt, we want to continue with original method instead
                return true;
            }

            // Get inventory
            if (___inventory == null)
            {
                MethodInfo getInventory = typeof(ItemFromInventory).GetMethod("GetInventory", BindingFlags.NonPublic | BindingFlags.Instance);
                ___inventory = (Inventory)getInventory.Invoke(__instance, null);
            }

            if (legendarySaltPile != null)
            {
                ___inventory.AddItem(legendarySaltPile.convertToSaltOnPickup, legendarySaltPile.amountOfSalt * multiplier, true, true);
                __instance.DestroyItem();
            }
            else
            {
                // Call to base's PutInInventory
                ___inventory.AddItem(__instance.inventoryItem, multiplier, true, true); // Default value is 1, so we use the multiplier directly instead
                __instance.DestroyItem(); // The original checks if can destroy but unecessary here because a legendary substance can always be destroyed
            }
            MethodInfo tryDetachFromAlchemicalMachine = typeof(AlchemyMachineProductItem).GetMethod("TryDetachFromAlchemicalMachine", BindingFlags.NonPublic | BindingFlags.Instance);
            tryDetachFromAlchemicalMachine.Invoke(__instance, null);

            // Skip original method
            return false;
        }
    }

    class GardenDropPotionPatch
    {
        // This postfix will check the dropped item to possibly apply effects to the garden
        static void Postfix(ref ItemFromInventory __instance)
        {
            PotionItem potionPhysicalItem = null;
            int growthCount = 0;
            int harvestCount = 0;

            // If in garden
            if (Managers.Room.IsCurrentOrTargetRoom(RoomManager.RoomIndex.Garden))
            {
                // If is a potion
                PotionItem potionItem = __instance as PotionItem;
                if (potionItem != null)
                {
                    potionPhysicalItem = potionItem;
                    // This should always be true but never know
                    Potion potion = potionItem.inventoryItem as Potion;
                    if (potion != null)
                    {
                        for (int i = 0; i < potion.effects.Length; ++i)
                        {
                            if (potion.effects[i] == null)
                            {
                                continue;
                            }
                            if (potion.effects[i].name.Equals("Growth", StringComparison.OrdinalIgnoreCase))
                            {
                                growthCount++;
                            }
                            else if (potion.effects[i].name.Equals("Crop", StringComparison.OrdinalIgnoreCase))
                            {
                                harvestCount++;
                            }
                        }
                    }
                }
            }

            if (growthCount > 0)
            {
                PlentifulHarvestMod.AddGrowth(potionPhysicalItem, growthCount);
            }

            if (harvestCount > 0)
            {
                PlentifulHarvestMod.harvestCount += harvestCount;
                PlentifulHarvestMod.harvestPotions.Add(potionPhysicalItem);
            }
        }
    }

    class GardenGrabPotionPatch
    {
        // This prefix will check the grabbed item to process if we need to modify mod values
        static void Prefix(ref ItemFromInventory __instance)
        {
            PotionItem potionPhysicalItem = null;
            bool hasGrowth = false;
            bool hasHarvest = false;
            int harvestCount = 0;

            // If in garden
            if (Managers.Room.IsCurrentOrTargetRoom(RoomManager.RoomIndex.Garden))
            {
                // If is a potion
                PotionItem potionItem = __instance as PotionItem;
                if (potionItem != null)
                {
                    potionPhysicalItem = potionItem;
                    // This should always be true but never know
                    Potion potion = potionItem.inventoryItem as Potion;
                    if (potion != null)
                    {
                        for (int i = 0; i < potion.effects.Length; ++i)
                        {
                            if(potion.effects[i] == null)
                            {
                                continue;
                            }
                            if (potion.effects[i].name.Equals("Growth", StringComparison.OrdinalIgnoreCase))
                            {
                                hasGrowth = true;
                            }
                            else if (potion.effects[i].name.Equals("Crop", StringComparison.OrdinalIgnoreCase))
                            {
                                hasHarvest = true;
                                harvestCount++;
                            }
                        }
                    }
                }
            }

            if (hasGrowth)
            {
                PlentifulHarvestMod.RemoveGrowth(potionPhysicalItem);
            }

            if (hasHarvest)
            {
                // Decrement harvestCount only if the potion was found in the list
                if (PlentifulHarvestMod.harvestPotions.Remove(potionPhysicalItem))
                {
                    PlentifulHarvestMod.harvestCount -= harvestCount;
                }
            }
        }
    }

    class GrowPlantsPatch
    {
        static void Prefix()
        {
            int minimalSpotPlants = 6; // 6 is default
            if (PlentifulHarvestMod.currentGrowthTimers.Count > 0) // If we have currentGrowthTimers it means growth was triggered by a potion, so we want to overwrite the minimum
            {
                int levelSum = 0;
                foreach (PlentifulHarvestMod.GrowthTimer growthTimer in PlentifulHarvestMod.currentGrowthTimers)
                {
                    levelSum += growthTimer.level;
                }
                PlentifulHarvestMod.currentGrowthTimers.Clear();
                minimalSpotPlants = levelSum * PlentifulHarvestMod.growthIncrement;
            }
            Managers.Ingredient.spotPlantSettings.minimalSpotPlants = minimalSpotPlants;
        }

        // This postfix will reset harvestCount and destroy all harvest potions in garden. The values have already been used by SpotPlantIngredientAmountPatch by now
        static void Postfix()
        {
            PlentifulHarvestMod.harvestCount = 0;
            foreach (PotionItem potion in PlentifulHarvestMod.harvestPotions)
            {
                GameObject.Destroy(potion);
            }
            PlentifulHarvestMod.harvestPotions.Clear();
        }
    }
}
