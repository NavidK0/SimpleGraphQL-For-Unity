using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using SimpleGraphQL.GraphQLParser;
using SimpleGraphQL.GraphQLParser.AST;

// ifdef for different unity versions
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;

#elif UNITY_2017_1_OR_NEWER
using UnityEditor.Experimental.AssetImporters;
#endif

namespace SimpleGraphQL
{
    [ScriptedImporter(1, "graphql")]
    public class GraphQLImporterV1 : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            string contents = File.ReadAllText(ctx.assetPath);
            string fileName = Path.GetFileNameWithoutExtension(ctx.assetPath);
            var queryFile = ScriptableObject.CreateInstance<GraphQLFile>();

            GraphQLDocument graphQLDocument = parser.Parse(new Source(contents));

            List<GraphQLOperationDefinition> operations = graphQLDocument.Definitions
                .FindAll(x => x.Kind == ASTNodeKind.OperationDefinition)
                .Select(x => (GraphQLOperationDefinition) x)
                .ToList();

            if (operations.Count > 0)
            {
                foreach (GraphQLOperationDefinition operation in operations)
                {
                    // Check for multiple anonymous queries (not allowed by graphQL)
                    // Also checks for anonymous queries inside a file with named queries
                    if (queryFile.Queries.Count > 1 && operation.Name == null)
                    {
                        throw new ArgumentException(
                            $"Multiple anonymous queries/anonymous query with named query found within: {ctx.assetPath}\nPlease ensure that there is either only one anonymous query, or all queries are named within the file!");
                    }

                    if (!Enum.TryParse(operation.Operation.ToString(), out OperationType operationType))
                    {
                        Debug.LogWarning("Unable to convert operation type in " + ctx.assetPath);
                    }

                    queryFile.Queries.Add(new Query
                    {
                        FileName = fileName,
                        OperationName = operation.Name?.Value,
                        OperationType = operationType,
                        Source = contents
                    });
                }
            }
            else
            {
                throw new ArgumentException(
                    $"There were no operation definitions inside this graphql: {ctx.assetPath}\nPlease ensure that there is at least one operation defined!");
            }

            ctx.AddObjectToAsset("QueryScriptableObject", queryFile);
            ctx.SetMainObject(queryFile);
        }
    }
}