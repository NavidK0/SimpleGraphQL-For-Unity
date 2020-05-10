using System;
using System.Collections.Generic;
using SimpleGraphQL.GraphQLParser.AST;

namespace SimpleGraphQL.GraphQLParser
{
    public class GraphQLAstVisitor
    {
        protected IDictionary<string, GraphQLFragmentDefinition> Fragments { get; private set; }

        public GraphQLAstVisitor()
        {
            Fragments = new Dictionary<string, GraphQLFragmentDefinition>();
        }

        public virtual GraphQLName BeginVisitAlias(GraphQLName alias) => alias;

        public virtual GraphQLArgument BeginVisitArgument(GraphQLArgument argument)
        {
            if (argument.Name != null)
                BeginVisitNode(argument.Name);

            if (argument.Value != null)
                BeginVisitNode(argument.Value);

            return EndVisitArgument(argument);
        }

        public virtual IEnumerable<GraphQLArgument> BeginVisitArguments(IEnumerable<GraphQLArgument> arguments)
        {
            foreach (var argument in arguments)
                BeginVisitNode(argument);

            return arguments;
        }

        public virtual GraphQLScalarValue BeginVisitBooleanValue(GraphQLScalarValue value) => value;

        public virtual GraphQLDirective BeginVisitDirective(GraphQLDirective directive)
        {
            if (directive.Name != null)
                BeginVisitNode(directive.Name);

            if (directive.Arguments != null)
                BeginVisitArguments(directive.Arguments);

            return directive;
        }

        public virtual IEnumerable<GraphQLDirective> BeginVisitDirectives(IEnumerable<GraphQLDirective> directives)
        {
            foreach (var directive in directives)
                BeginVisitNode(directive);

            return directives;
        }

        public virtual GraphQLScalarValue BeginVisitEnumValue(GraphQLScalarValue value) => value;

        public virtual GraphQLFieldSelection BeginVisitFieldSelection(GraphQLFieldSelection selection)
        {
            BeginVisitNode(selection.Name);

            if (selection.Alias != null)
                BeginVisitAlias((GraphQLName)BeginVisitNode(selection.Alias));

            if (selection.Arguments != null)
                BeginVisitArguments(selection.Arguments);

            if (selection.SelectionSet != null)
                BeginVisitNode(selection.SelectionSet);

            if (selection.Directives != null)
                BeginVisitDirectives(selection.Directives);

            return EndVisitFieldSelection(selection);
        }

        public virtual GraphQLScalarValue BeginVisitFloatValue(GraphQLScalarValue value) => value;

        public virtual GraphQLFragmentDefinition BeginVisitFragmentDefinition(GraphQLFragmentDefinition node)
        {
            BeginVisitNode(node.TypeCondition);
            BeginVisitNode(node.Name);

            if (node.SelectionSet != null)
                BeginVisitNode(node.SelectionSet);

            return node;
        }

        public virtual GraphQLFragmentSpread BeginVisitFragmentSpread(GraphQLFragmentSpread fragmentSpread)
        {
            BeginVisitNode(fragmentSpread.Name);
            return fragmentSpread;
        }

        public virtual GraphQLInlineFragment BeginVisitInlineFragment(GraphQLInlineFragment inlineFragment)
        {
            if (inlineFragment.TypeCondition != null)
                BeginVisitNode(inlineFragment.TypeCondition);

            if (inlineFragment.Directives != null)
                BeginVisitDirectives(inlineFragment.Directives);

            if (inlineFragment.SelectionSet != null)
                BeginVisitSelectionSet(inlineFragment.SelectionSet);

            return inlineFragment;
        }

        public virtual GraphQLScalarValue BeginVisitIntValue(GraphQLScalarValue value) => value;

        public virtual GraphQLName BeginVisitName(GraphQLName name) => name;

        public virtual GraphQLNamedType BeginVisitNamedType(GraphQLNamedType typeCondition) => typeCondition;

        public virtual ASTNode BeginVisitNode(ASTNode node)
        {
            switch (node?.Kind)
            {
                case ASTNodeKind.OperationDefinition:
                    return BeginVisitOperationDefinition((GraphQLOperationDefinition) node);
                case ASTNodeKind.SelectionSet:
                    return BeginVisitSelectionSet((GraphQLSelectionSet) node);
                case ASTNodeKind.Field:
                    return BeginVisitNonIntrospectionFieldSelection((GraphQLFieldSelection) node);
                case ASTNodeKind.Name:
                    return BeginVisitName((GraphQLName) node);
                case ASTNodeKind.Argument:
                    return BeginVisitArgument((GraphQLArgument) node);
                case ASTNodeKind.FragmentSpread:
                    return BeginVisitFragmentSpread((GraphQLFragmentSpread) node);
                case ASTNodeKind.FragmentDefinition:
                    return BeginVisitFragmentDefinition((GraphQLFragmentDefinition) node);
                case ASTNodeKind.InlineFragment:
                    return BeginVisitInlineFragment((GraphQLInlineFragment) node);
                case ASTNodeKind.NamedType:
                    return BeginVisitNamedType((GraphQLNamedType) node);
                case ASTNodeKind.Directive:
                    return BeginVisitDirective((GraphQLDirective) node);
                case ASTNodeKind.Variable:
                    return BeginVisitVariable((GraphQLVariable) node);
                case ASTNodeKind.IntValue:
                    return BeginVisitIntValue((GraphQLScalarValue) node);
                case ASTNodeKind.FloatValue:
                    return BeginVisitFloatValue((GraphQLScalarValue) node);
                case ASTNodeKind.StringValue:
                    return BeginVisitStringValue((GraphQLScalarValue) node);
                case ASTNodeKind.BooleanValue:
                    return BeginVisitBooleanValue((GraphQLScalarValue) node);
                case ASTNodeKind.EnumValue:
                    return BeginVisitEnumValue((GraphQLScalarValue) node);
                case ASTNodeKind.ListValue:
                    return BeginVisitListValue((GraphQLListValue) node);
                case ASTNodeKind.ObjectValue:
                    return BeginVisitObjectValue((GraphQLObjectValue) node);
                case ASTNodeKind.ObjectField:
                    return BeginVisitObjectField((GraphQLObjectField) node);
                case ASTNodeKind.VariableDefinition:
                    return BeginVisitVariableDefinition((GraphQLVariableDefinition) node);
                default:
                    return null;
            }
        }

        public virtual GraphQLOperationDefinition BeginVisitOperationDefinition(GraphQLOperationDefinition definition)
        {
            if (definition.Name != null)
                BeginVisitNode(definition.Name);

            if (definition.VariableDefinitions != null)
                BeginVisitVariableDefinitions(definition.VariableDefinitions);

            BeginVisitNode(definition.SelectionSet);

            return EndVisitOperationDefinition(definition);
        }

        public virtual GraphQLOperationDefinition EndVisitOperationDefinition(GraphQLOperationDefinition definition) => definition;

        public virtual GraphQLSelectionSet BeginVisitSelectionSet(GraphQLSelectionSet selectionSet)
        {
            if (selectionSet.Selections != null)
            {
                foreach (var selection in selectionSet.Selections)
                    BeginVisitNode(selection);
            }

            return selectionSet;
        }

        public virtual GraphQLScalarValue BeginVisitStringValue(GraphQLScalarValue value) => value;

        public virtual GraphQLVariable BeginVisitVariable(GraphQLVariable variable)
        {
            if (variable.Name != null)
                BeginVisitNode(variable.Name);

            return EndVisitVariable(variable);
        }

        public virtual GraphQLVariableDefinition BeginVisitVariableDefinition(GraphQLVariableDefinition node)
        {
            BeginVisitNode(node.Type);

            return node;
        }

        public virtual IEnumerable<GraphQLVariableDefinition> BeginVisitVariableDefinitions(IEnumerable<GraphQLVariableDefinition> variableDefinitions)
        {
            foreach (var definition in variableDefinitions)
                BeginVisitNode(definition);

            return variableDefinitions;
        }

        public virtual GraphQLArgument EndVisitArgument(GraphQLArgument argument) => argument;

        public virtual GraphQLFieldSelection EndVisitFieldSelection(GraphQLFieldSelection selection) => selection;

        public virtual GraphQLVariable EndVisitVariable(GraphQLVariable variable) => variable;

        public virtual void Visit(GraphQLDocument ast)
        {
            if (ast.Definitions != null)
            {
                foreach (var definition in ast.Definitions)
                {
                    if (definition.Kind == ASTNodeKind.FragmentDefinition)
                    {
                        var fragment = (GraphQLFragmentDefinition)definition;
                        string name = fragment.Name?.Value;
                        if (name == null)
                            throw new InvalidOperationException("Fragment name cannot be null");

                        Fragments.Add(name, fragment);
                    }
                }

                foreach (var definition in ast.Definitions)
                {
                    BeginVisitNode(definition);
                }
            }
        }

        public virtual GraphQLObjectField BeginVisitObjectField(GraphQLObjectField node)
        {
            BeginVisitNode(node.Name);

            BeginVisitNode(node.Value);

            return node;
        }

        public virtual GraphQLObjectValue BeginVisitObjectValue(GraphQLObjectValue node)
        {
            if (node.Fields != null)
            {
                foreach (var field in node.Fields)
                    BeginVisitNode(field);
            }

            return EndVisitObjectValue(node);
        }

        public virtual GraphQLObjectValue EndVisitObjectValue(GraphQLObjectValue node) => node;

        public virtual GraphQLListValue EndVisitListValue(GraphQLListValue node) => node;

        private ASTNode BeginVisitListValue(GraphQLListValue node)
        {
            if (node.Values != null)
            {
                foreach (var value in node.Values)
                    BeginVisitNode(value);
            }

            return EndVisitListValue(node);
        }

        private ASTNode BeginVisitNonIntrospectionFieldSelection(GraphQLFieldSelection selection)
        {
            return BeginVisitFieldSelection(selection);
        }
    }
}
