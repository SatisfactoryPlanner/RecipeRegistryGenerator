using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RecipeRegistryGenerator
{
    static class StringExtensions
    {
        private static readonly Regex SnakeCaseRegex = new Regex("(?<=[a-z0-9])[A-Z]", RegexOptions.Compiled);

        public static string ToSnakeCase(this string pascalCase)
        {
            return SnakeCaseRegex
                .Replace(
                    pascalCase.Replace(" ", ""), "_$0"
                )
                .ToLowerInvariant()
                .Replace(".", "_")
                .Replace(":", "_")
                .Replace("-", "_")
                .Replace("(", "_")
                .Replace(")", "_");
        }
    }
}
