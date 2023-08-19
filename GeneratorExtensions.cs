using System.Runtime.CompilerServices;
using Abg.SourceGeneration;
using Microsoft.CodeAnalysis;

namespace Abg.Entities
{

    internal static class GeneratorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SourceBuilder TypeFullname(this SourceBuilder sb, ITypeSymbol type)
        {
            sb.T(type.ToDisplayString());
            return sb;
        }

        public static SourceBuilder GenericType(this SourceBuilder sb, string genericType, ITypeSymbol type1)
        {
            sb.T(genericType).T("<").TypeFullname(type1).T(">");
            return sb;
        }

        public static SourceBuilder GenericType(this SourceBuilder sb, string genericType, ITypeSymbol type1,
            ITypeSymbol type2)
        {
            sb.T(genericType).T("<").TypeFullname(type1).T(",").TypeFullname(type2).T(">");
            return sb;
        }

        public static bool IsAccessibleOutsideOfAssembly(this ISymbol symbol) =>
            symbol.DeclaredAccessibility switch
            {
                Accessibility.Private => false,
                Accessibility.Internal => false,
                Accessibility.ProtectedAndInternal => false,
                Accessibility.Protected => false,
                Accessibility.ProtectedOrInternal => false,
                Accessibility.Public => true,
                _ => true, //Here should be some reasonable default
            };
    }
}