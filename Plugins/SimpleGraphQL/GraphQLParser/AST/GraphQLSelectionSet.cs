using System.Collections.Generic;

namespace SimpleGraphQL.GraphQLParser.AST
{
    public class GraphQLSelectionSet : ASTNode
    {
        public override ASTNodeKind Kind => ASTNodeKind.SelectionSet;

        public List<ASTNode> Selections { get; set; }
    }
}