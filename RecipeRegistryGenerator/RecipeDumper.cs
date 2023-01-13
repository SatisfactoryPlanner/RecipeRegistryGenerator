using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Objects.UObject;
using RecipeRegistryGenerator.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRegistryGenerator
{
    class RecipeDumper
    {
        private Dictionary<String, Item> CachedItems = new();
        private IFileProvider m_Provider;

        public RecipeDumper(IFileProvider provider)
        {
            m_Provider = provider;
        }

        public async IAsyncEnumerable<Recipe> Dump()
        {
            foreach (var file in m_Provider.Files.Values
                .Where(e => e.Path.StartsWith("FactoryGame/Content/FactoryGame/Recipes"))
                .Where(e => e.IsUE4Package))
            {
                var recipe = await GetRecipe(file);
                if (recipe != null)
                {
                    yield return recipe;
                }
            }

            yield break;
        }

        public async Task<Item?> ResolveItem(string itemPath)
        {
            if (CachedItems.TryGetValue(itemPath, out var existingItem)) return existingItem;

            var package = await m_Provider.LoadPackageAsync(itemPath + ".uasset");

            try
            {
                var item = new Item(package);
                CachedItems.Add(itemPath, item);
                return item;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("File: " + package.Name + " " + e.Message);
                return null;
            }
        }

        private async Task<ItemAmount?> GetItem(IPackage package, FPropertyTagType property)
        {
            var scriptStruct = property.GetValue(typeof(UScriptStruct)) as UScriptStruct;
            if (scriptStruct == null) return null;

            var structType = (FStructFallback)scriptStruct.StructType;

            if (!structType.TryGetValue<FPackageIndex>(out var itemClass, "ItemClass"))
            {
                Console.WriteLine("Failed to get item class for: " + package.Name);
                return null;
            }

            if (!structType.TryGetValue<int>(out var amount, "Amount"))
            {
                Console.WriteLine("Failed to get amount for: " + package.Name);
                return null;
            }

            var itemClassObject = package.ResolvePackageIndex(itemClass)?.Package;
            if (itemClassObject == null)
            {
                Console.WriteLine("Failed to get ItemClass object for: " + package.Name);
                return null;
            }

            var item = await ResolveItem(itemClassObject.Name);
            if (item == null)
            {
                Console.WriteLine("Failed to resolve item: " + itemClassObject.Name + " for: " + package.Name);
                return null;
            }

            if (item.Type == ItemType.Liquid || item.Type == ItemType.Gas)
            {
                amount /= 1000;
            }


            var itemAmount = new ItemAmount(item, amount);

            return itemAmount;
        }

        private async IAsyncEnumerable<ItemAmount> IterateItems(IPackage package, UScriptArray array)
        {
            foreach (var property in array.Properties)
            {
                var item = await GetItem(package, property);
                if (item == null) yield break;
                yield return item;
            }
            yield break;
        }

        private static Machine MachineNameToMachine(string name)
        {
            if (!Enum.TryParse(typeof(Machine), name, out var machine))
            {
                return Machine.None;
            }
            return (Machine)machine;
        }

        private async Task<Machine> GetMachine(UScriptArray array)
        {
            foreach (var property in array.Properties)
            {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                var softObjectPath = (FSoftObjectPath)property.GetValue(typeof(FSoftObjectPath));
#pragma warning restore CS8605 // Unboxing a possibly null value.

                var machinePackage = await softObjectPath.TryLoadAsync(m_Provider);
                if (machinePackage == null) continue;

#pragma warning disable CS8604 // Possible null reference argument.
                var displayName = Utils.GetDisplayName(machinePackage.Owner);
#pragma warning restore CS8604 // Possible null reference argument.
                if (displayName == null) continue;

                var machine = MachineNameToMachine(displayName);
                if (machine == Machine.None) continue;
                return machine;
            }
            return Machine.None;
        }

        private async Task<Recipe?> GetRecipe(GameFile file)
        {
            var package = await m_Provider.LoadPackageAsync(file.Path);
            var cdo = Utils.GetCDO(package);
            if (cdo == null)
            {
                Console.WriteLine("Failed to get CDO for " + file.Path);
                return null;
            }

            var recipe = new Recipe();

            if (!cdo.TryGetValue<UScriptArray>(out var output, "mProduct"))
            {
                Console.WriteLine("No product for: " + file.Path);
                return null;
            }

            var outputItem = await GetItem(package, output.Properties[0]);
            if (outputItem == null)
            {
                Console.WriteLine("No output for: " + file.Path);
                return null;
            }
            recipe.Output = outputItem;

            if (output.Properties.Count > 1)
            {
                recipe.Byproduct = await GetItem(package, output.Properties[1]);
            }

            if (!cdo.TryGetValue<UScriptArray>(out var ingredients, "mIngredients"))
            {
                Console.WriteLine("No ingredients for: " + file.Path);
                return null;
            }

            await foreach (var item in IterateItems(package, ingredients))
            {
                recipe.Input.Add(item);
            }

            if (!cdo.TryGetValue<UScriptArray>(out var machines, "mProducedIn"))
            {
                Console.WriteLine("No machine for: " + file.Path);
            }
            else
            {
                recipe.Machine = await GetMachine(machines);
            }

            if (cdo.TryGetValue<float>(out var manufacturingDuration, "mManufactoringDuration"))
            {
                recipe.ManufacturingDuration = MathF.Max(manufacturingDuration, 1.0f);
            }

            recipe.Alternate = file.Name.Contains("Alternate");


            var displayName = Utils.GetDisplayName(package);

            var isAlternate = displayName?.Contains("Alternate: ") ?? false;
            var hasItemName = !string.IsNullOrEmpty(recipe.Output.Item.Name);


            if (!isAlternate && hasItemName)
            {
                recipe.Name = recipe.Output.Item.Name;
            }
            else
            {
                recipe.Name = displayName!;
            }

            return recipe;
        }

    }
}
