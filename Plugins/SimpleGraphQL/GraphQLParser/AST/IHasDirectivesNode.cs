using System.Collections.Generic;

namespace SimpleGraphQL.GraphQLParser.AST
{
    public interface IHasDirectivesNode
    {
        List<GraphQLDirective> Directives { get; set; }
    }
}
