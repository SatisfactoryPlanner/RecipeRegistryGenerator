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

        var itemRecipes = new Dictionary<String, (ItemAmount, List<Recipe>)>();

        var graph = new AdjacencyGraph<GraphNode, GraphEdge>();

        await foreach (var recipe in dumper.Dump())
        {
            Console.WriteLine(recipe.Name + " output: " + recipe.Output.Item.Name + " machine: " + recipe.Machine.ToString());
            if (!itemRecipes.TryGetValue(recipe.Output.Item.PackageName, out var recipes))
            {
                recipes = (recipe.Output, new List<Recipe>());
                itemRecipes.Add(recipe.Output.Item.PackageName, recipes);
            }

            recipes.Item2.Add(recipe);
        }

        var itemGraphNodes = new Dictionary<string, GraphNode>();

        foreach (var (_, (item, recipes)) in itemRecipes)
        {
            if (!itemGraphNodes.TryGetValue(item.Item.PackageName, out var itemVertex))
            {
                itemVertex = new GraphNode
                {
                    Item = item,
                    Recipe = null
                };
                graph.AddVertex(itemVertex);
                itemGraphNodes.Add(item.Item.PackageName, itemVertex);
            }

            Console.WriteLine(graph.ContainsVertex(itemVertex));
            graph.AddVertex(itemVertex);

            foreach (var recipe in recipes)
            {
                var recipeVertex = new GraphNode
                {
                    Item = null,
                    Recipe = recipe
                };
                graph.AddVertex(recipeVertex);
                graph.AddEdge(new GraphEdge(itemVertex, recipeVertex));

                foreach (var inputItem in recipe.Input)
                {
                    if (!itemGraphNodes.TryGetValue(inputItem.Item.PackageName, out var inputVertex))
                    {
                        inputVertex = new GraphNode
                        {
                            Item = item,
                            Recipe = null
                        };
                        graph.AddVertex(inputVertex);
                        itemGraphNodes.Add(inputItem.Item.PackageName, inputVertex);
                    }

                    graph.AddEdge(new GraphEdge(recipeVertex, inputVertex));
                }
            }
        }


        var graphViz = new GraphvizAlgorithm<GraphNode, GraphEdge>(graph);
        graphViz.FormatVertex += (sender, args) =>
        {
            if (args.Vertex.Item != null)
            {
                args.VertexFormat.Label = args.Vertex.Item.Item.Name;
                args.VertexFormat.Comment = args.Vertex.Item.Item.Name;
            }
            else
            {
                args.VertexFormat.Label = args.Vertex.Recipe.Name + " Recipe";
                args.VertexFormat.Comment = args.Vertex.Recipe.Name + " Recipe";
            }
        };

        graphViz.Generate(new FileDotEngine(), "graph.graphviz");
    }

    public static void Main(string[] args) => AsyncMain(args).GetAwaiter().GetResult();

}