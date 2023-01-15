using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.UObject;
using System.Text.RegularExpressions;
using CUE4Parse.FileProvider;

namespace RecipeRegistryGenerator
{
    class Utils
    {
        public static UObject? GetCDO(IPackage package)
        {
            return package.GetExports().FirstOrDefault(e => e.Flags.HasFlag(EObjectFlags.RF_ClassDefaultObject));
        }

        public static string? GetDisplayName(IPackage package)
        {
            var cdo = GetCDO(package);
            if (cdo == null) return null;

            if (!cdo.TryGetValue<FText>(out var displayName, "mDisplayName"))
            {
                // trying to get machine name instead

                if (!cdo.TryGetValue<FPackageIndex>(out var buildableClass, "mBuildableClass")) return null;

                var buildableClassObject = package.ResolvePackageIndex(buildableClass)?.Package;
                if (buildableClassObject == null) return null;

                var buildableClassCDO = GetCDO(buildableClassObject);
                if (buildableClassCDO == null) return null;

                if (!buildableClassCDO.TryGetValue<FText>(out var buildableDisplayName, "mDisplayName")) return null;
                return buildableDisplayName.Text;
            }
            return displayName.Text;
        }

    }
}
