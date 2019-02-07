using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NSubstitute.Analyzers.Shared.Extensions
{
    internal static class SyntaxNodeExtensions
    {
        public static SyntaxNode GetParentNode(this SyntaxNode syntaxNode, IEnumerable<int> parentNodeHierarchyKinds)
        {
            using (var descendantNodesEnumerator = syntaxNode.DescendantNodes().GetEnumerator())
            {
                using (var hierarchyKindEnumerator = parentNodeHierarchyKinds.GetEnumerator())
                {
                    while (hierarchyKindEnumerator.MoveNext() && descendantNodesEnumerator.MoveNext())
                    {
                        if (descendantNodesEnumerator.Current.RawKind != hierarchyKindEnumerator.Current)
                        {
                            return null;
                        }
                    }

                    if (hierarchyKindEnumerator.MoveNext() == false)
                    {
                        return descendantNodesEnumerator.Current;
                    }
                }
            }

            return null;
        }
    }
}