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

        public void Serialize(StringBuilder builder)
        {
            builder.Append("let ");
            builder.Append(Name.ToSnakeCase());
            builder.Append(" = Rc::new(Item { name: \"");
            builder.Append(Name);
            builder.Append("\" }); // ");
            builder.Append(PackageName);
            builder.Append("\n");

            builder.Append("item_registry.push(");
            builder.Append(Name.ToSnakeCase());
            builder.Append(".clone());\n");
        }
    }

}
