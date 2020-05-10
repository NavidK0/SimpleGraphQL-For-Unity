using System.Collections.Generic;

namespace SimpleGraphQL.GraphQLParser.AST
{
    public class GraphQLSchemaDefinition : ASTNode, IHasDirectivesNode
    {
        public List<GraphQLDirective> Directives { get; set; }

        public override ASTNodeKind Kind => ASTNodeKind.SchemaDefinition;

        public List<GraphQLOperationTypeDefinition> OperationTypes { get; set; }
    }
}