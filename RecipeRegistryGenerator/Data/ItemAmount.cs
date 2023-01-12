using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeRegistryGenerator.Data
{
    record ItemAmount(Item Item, int Amount)
    {
        public void Serialize(StringBuilder builder)
        {
            builder.Append("ItemAmount {");

            builder.Append("item: ");
            // only serializing reference
            builder.Append(Item.Name.ToSnakeCase());
            builder.Append(".clone()");
            builder.Append(", ");

            builder.Append("amount: ");
            builder.Append(Amount);
            builder.Append(" ");

            builder.Append("}");
        }
    }


}
