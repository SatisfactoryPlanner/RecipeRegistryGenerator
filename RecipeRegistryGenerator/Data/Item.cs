using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Objects.UObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRegistryGenerator.Data
{

    enum ItemType
    {
        Solid,
        Liquid,
        Gas
    }

    class Item
    {
        public string Name { get; set; } = "None";
        public string PackageName { get; set; } = "None";
        public bool Raw { get; set; } = false;
        public ItemType Type { get; set; } = ItemType.Solid;

        private Item() { }

        public Item(IPackage package)
        {
            var cdo = Utils.GetCDO(package);
            if (cdo?.TryGetValue<FName>(out var form, "mForm") ?? false)
            {
                if (form.Text.Contains("LIQUID"))
                {
                    Type = ItemType.Liquid;
                }
                else if (form.Text.Contains("GAS"))
                {
                    Type = ItemType.Gas;
                }
            }

            var displayName = Utils.GetDisplayName(package);
            Name = displayName ?? "";
            PackageName = package.Name;

            if (package.Name.Contains("RawResources"))
            {
                Raw = true;
            }
        }

        public static Item NONE = new Item { Name = "None", PackageName = "None" };

        public void Serialize(StringBuilder builder)
        {
            builder.Append("let ");
            builder.Append(Name.ToSnakeCase());
            builder.Append(" = Rc::new(Item { ");

            builder.Append("name: \"");
            builder.Append(Name);
            builder.Append("\", ");

            builder.Append("raw: ");

            if (Raw)
            {
                builder.Append("true");
            }
            else
            {
                builder.Append("false");
            }

            builder.Append("}); // ");
            builder.Append(PackageName);
            builder.Append("\n");

            builder.Append("item_registry.push(");
            builder.Append(Name.ToSnakeCase());
            builder.Append(".clone());\n");
        }
    }

}
