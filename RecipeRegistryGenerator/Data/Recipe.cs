using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRegistryGenerator.Data
{

    class Recipe
    {
        public string Name { get; set; } = "";
        public List<ItemAmount> Input { get; set; } = new();
        public ItemAmount Output { get; set; } = new(Item.NONE, 0);
        public ItemAmount? Byproduct { get; set; } = null;
        public float ManufacturingDuration { get; set; } = 0.0f;
        public bool Alternate { get; set; } = false;
        public Machine Machine { get; set; } = Machine.None;

        public void Serialize(StringBuilder builder)
        {
            builder.Append("let ");
            builder.Append(Name.ToSnakeCase());
            builder.Append("_recipe = Recipe { ");

            builder.Append("name: \"");
            builder.Append(Name);
            builder.Append("\", ");

            builder.Append("machine: Machine::");
            builder.Append(Machine.ToString());
            builder.Append(", ");

            builder.Append("input: vec![");
            foreach (var inputItem in Input.Where(e => !string.IsNullOrEmpty(e.Item.Name)))
            {
                inputItem.Serialize(builder);
                builder.Append(", ");
            }
            builder.Append("], ");

            builder.Append("output: ");
            Output.Serialize(builder);
            builder.Append(", ");

            builder.Append("byproduct: ");
            if (Byproduct != null)
            {
                builder.Append("Some(");
                Byproduct.Serialize(builder);
                builder.Append(")");
            }
            else
            {
                builder.Append("None");
            }
            builder.Append(", ");

            builder.Append("manufacturing_duration: ");
            builder.Append(ManufacturingDuration);
            builder.Append("f32, ");

            builder.Append("alternate: ");
            if (Alternate)
            {
                builder.Append("true");
            }
            else
            {
                builder.Append("false");
            }
            builder.Append(" ");

            builder.Append("};\n");

            builder.Append("recipe_registry.entry(\"");
            builder.Append(Output.Item.Name);
            builder.Append("\").or_insert_with(Vec::new)");

            builder.Append(".push(");
            builder.Append(Name.ToSnakeCase());
            builder.Append("_recipe);\n");
        }
    }

}
