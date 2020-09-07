using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleGraphQL.GraphQLParser;
using SimpleGraphQL.GraphQLParser.AST;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace SimpleGraphQL
{
    [ScriptedImporter(1, "graphql")]
    public class GraphQLImporterV1 : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            Lexer lexer = new Lexer();
            Parser parser = new Parser(lexer);
            // Match against all lines starting with "#import "
            // and assign values coming after it to a group called path
            Regex regex = new Regex("#import \\\"(?<path>[\\w\\/.]+)\\\"");
            string contents = File.ReadAllText(ctx.assetPath);
            foreach(Match match in regex.Matches(contents))
            {
                string filePath = Path.Combine(Path.GetDirectoryName(ctx.assetPath), match.Groups["path"].Value);
                // Since the filePath contains . and .. to traverse current / parent directory
                // we need to convert the relative path to an actual path
                // and then read contents of that file
                string replacementContents = File.ReadAllText(Path.GetFullPath(filePath));

                // Replace the #import directive as a whole with the replacement contents
                contents = contents.Replace(match.Value, replacementContents);
            }

            string fileName = Path.GetFileNameWithoutExtension(ctx.assetPath);
            GraphQLFile queryFile = ScriptableObject.CreateInstance<GraphQLFile>();

            GraphQLDocument graphQLDocument = parser.Parse(new Source(contents));

            List<GraphQLOperationDefinition> operations = graphQLDocument.Definitions
                .FindAll(x => x.Kind == ASTNodeKind.OperationDefinition)
                .Select(x => (GraphQLOperationDefinition)x)
                .ToList();

            if(operations.Count > 0)
            {
                foreach(GraphQLOperationDefinition operation in operations)
                {
                    // Check for multiple anonymous queries (not allowed by graphQL)
                    // Also checks for anonymous queries inside a file with named queries
                    if(queryFile.Queries.Count > 1 && operation.Name == null)
                    {
                        throw new ArgumentException(
                            $"Multiple anonymous queries/anonymous query with named query found within: {ctx.assetPath}\nPlease ensure that there is either only one anonymous query, or all queries are named within the file!");
                    }

                    if(!Enum.TryParse(operation.Operation.ToString(), out OperationType operationType))
                    {
                        Debug.LogWarning("Unable to convert operation type in " + ctx.assetPath);
                    }

                    queryFile.Queries.Add(new Query
                    {
                        FileName = fileName,
                        OperationName = operation.Name.Value,
                        OperationType = operationType,
                        Source = contents
                    });
                }
            }
            // #typespec is a custom definition indicating that the file might not contain queries
            else if(string.IsNullOrEmpty(contents) || !contents.StartsWith("#typespec"))
            {
                throw new ArgumentException(
                    $"There were no operation definitions inside this graphql: {ctx.assetPath}\nPlease ensure that there is at least one operation defined!");
            }

            ctx.AddObjectToAsset("QueryScriptableObject", queryFile);
            ctx.SetMainObject(queryFile);
        }
    }
}
