using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.UE4.Versions;
using QuikGraph;
using QuikGraph.Graphviz;
using RecipeRegistryGenerator;
using RecipeRegistryGenerator.Data;
using System.Text;

class GraphNode
{
    public ItemAmount? Item { get; set; }
    public Recipe? Recipe { get; set; }
}

class GraphEdge : IEdge<GraphNode>
{
    private GraphNode m_SourceNode;
    private GraphNode m_TargetNode;

    public GraphEdge(GraphNode sourceNode, GraphNode targetNode)
    {
        m_SourceNode = sourceNode;
        m_TargetNode = targetNode;
    }

    public GraphNode Source => m_SourceNode;

    public GraphNode Target => m_TargetNode;
}



class Program
{
    private static async Task AsyncMain(string[] args)
    {
        // todo: get from args
        var provider = new DefaultFileProvider(
            "C:\\Program Files\\Epic Games\\SatisfactoryExperimental\\FactoryGame\\Content\\Paks\\",
            SearchOption.TopDirectoryOnly,
            versions: new VersionContainer(EGame.GAME_UE4_26));

        provider.Initialize();
        provider.SubmitKey(new FGuid(0, 0, 0, 0), new FAesKey("0000000000000000000000000000000000000000000000000000000000000000"));

        provider.LoadVirtualPaths();


        var dumper = new RecipeDumper(provider);

        var itemRecipes = new Dictionary<String, (Item, List<Recipe>)>();
        var items = new List<Item>();
        var recipes = new List<Recipe>();

        var graph = new AdjacencyGraph<GraphNode, GraphEdge>();

        await foreach (var recipe in dumper.Dump())
        {
            if (!items.Contains(recipe.Output.Item))
            {
                items.Add(recipe.Output.Item);
            }

            foreach (var inputItem in recipe.Input)
            {
                if (!items.Contains(inputItem.Item))
                {
                    items.Add(inputItem.Item);
                }
            }

            recipes.Add(recipe);
        }

        var builder = new StringBuilder();
        foreach (var item in items.Where(item => !string.IsNullOrEmpty(item.Name)))
        {
            item.Serialize(builder);
        }

        var recipeBuilder = new StringBuilder();
        foreach (var recipe in recipes
            .Where(recipe => !string.IsNullOrEmpty(recipe.Output.Item.Name))
            .Where(recipe => recipe.Machine != Machine.None))
        {
            recipe.Serialize(recipeBuilder);
        }

        Console.WriteLine("use std::{collections::HashMap, rc::Rc};");
        Console.WriteLine("use crate::{\n    buildings::building::Machine,\n    items::{item::Item, recipe::Recipe, ItemAmount},\n};");
        Console.WriteLine("#[allow(non_snake_case)]");
        Console.WriteLine("#[allow(clippy::redundant_clone)]");

        Console.WriteLine("pub(crate) fn get_registry() -> (Vec<Rc<Item>>, HashMap<&'static str, Vec<Recipe>>) {");
        Console.WriteLine("let mut item_registry: Vec<Rc<Item>> = Vec::new();");
        Console.WriteLine("let mut recipe_registry: HashMap<&'static str, Vec<Recipe>> = HashMap::new();");

        Console.WriteLine("{");
        Console.WriteLine(builder.ToString());
        Console.WriteLine(recipeBuilder.ToString());
        Console.WriteLine("}");

        Console.WriteLine("(item_registry, recipe_registry)");
        Console.WriteLine("}");
    }

    public static void Main(string[] args) => AsyncMain(args).GetAwaiter().GetResult();

}