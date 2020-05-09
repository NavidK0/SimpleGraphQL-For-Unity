using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace LastAbyss.SimpleGraphQL
{
    [ScriptedImporter(1, "graphql")]
    public class GraphQLImporterV1 : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("Text", asset);
            ctx.SetMainObject(asset);
        }
    }
}