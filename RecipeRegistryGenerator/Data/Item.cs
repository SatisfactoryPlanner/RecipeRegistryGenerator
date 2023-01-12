using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRegistryGenerator.Data
{

    class Item
    {
        public string Name { get; set; } = "None";
        public string PackageName { get; set; } = "None";

        private Item() { }

        public Item(IPackage package)
        {
            var displayName = Utils.GetDisplayName(package);
            Name = displayName ?? "";
            PackageName = package.Name;
        }

        public static Item NONE = new Item { Name = "None", PackageName = "None" };
    }

}
