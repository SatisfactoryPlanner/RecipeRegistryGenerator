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
        public ItemAmount Output { get; set; } = new(Item.NONE, 0, 0.0f);
        public ItemAmount? Byproduct { get; set; } = new(Item.NONE, 0, 0.0f);
        public Machine Machine { get; set; } = Machine.None;
    }

}
