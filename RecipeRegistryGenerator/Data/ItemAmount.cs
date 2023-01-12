using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRegistryGenerator.Data
{
    record ItemAmount(Item Item, int Amount, float manufacturingDuration);
}
