using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Veneer;

public class NameSpaceRewriter(string oldName, string newName) : CSharpSyntaxRewriter
{
    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        if (node.Name.ToString() == oldName)
        {
            NameSyntax newNameSyntax = SyntaxFactory.ParseName(newName);
            node = node.WithName(newNameSyntax);
        }
        return base.VisitNamespaceDeclaration(node);
    }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        if (node.Name.ToString() == oldName)
        {
            NameSyntax newNameSyntax = SyntaxFactory.ParseName(newName);
            node = node.WithName(newNameSyntax);
        }
        return base.VisitFileScopedNamespaceDeclaration(node);
    }
}