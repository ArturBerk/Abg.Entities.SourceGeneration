using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Abg.SourceGeneration
{
    public static class Builders
    {
        public static readonly ConstantBuilder Void = new ConstantBuilder("void");
        public static readonly NewLineBuilder NewLine = new NewLineBuilder();
        public static readonly EmptyBuilder Empty = new EmptyBuilder();
        
        public static ConstantBuilder Constant(string value)
        {
            return new ConstantBuilder(value);
        }
        
        public static GenericBuilder GenericType(string name, params string[] args)
        {
            return new GenericBuilder(Constant(name)).WithArgs(args.Select(Constant));
        }

        public static BlockBuilder Namespace(string name)
        {
            return new BlockBuilder(Constant("namespace " + name));
        }
        
        public static BlockBuilder Block(IBuilder name)
        {
            return new BlockBuilder(name);
        }
        
        public static BlockBuilder Namespace(IBuilder name)
        {
            return new BlockBuilder(Concat(Constant("namespace "), name));
        }

        public static ListBuilder Concat(params IBuilder[] builders)
        {
            return new ListBuilder().WithoutDelimiter().WithElements(builders);
        }
        
        public static ListBuilder Lines(params IBuilder[] builders)
        {
            return new ListBuilder().WithoutDelimiter().EachElementOnNewLine().WithElements(builders);
        }

        public static ConstantBuilder Using(string @using)
        {
            return new ConstantBuilder($"using {@using};");
        }
        
        public static ListBuilder List(params IBuilder[] builders)
        {
            return new ListBuilder().WithElements(builders);
        }

        public static ConstantBuilder Argument(string type, string name)
        {
            return new ConstantBuilder($"{type} {name}");
        }

        public static ConstantBuilder Assign(string type, string name)
        {
            return new ConstantBuilder($"{type} = {name};");
        }

        public static DelegateBuilder SB(Action<SourceBuilder> sb)
        {
            return new DelegateBuilder(sb);
        }

        public static ListBuilder Argument(IBuilder type, IBuilder name)
        {
            return new ListBuilder().WithDelimiter(" ").WithElements(type, name);
        }

        public static TypeBuilder Class(string name)
        {
            return new TypeBuilder(SourceGeneration.TypeKind.Class, Constant(name));
        }

        public static TypeBuilder Struct(string name)
        {
            return new TypeBuilder(SourceGeneration.TypeKind.Struct, Constant(name));
        }

        public static TypeBuilder Class(IBuilder name)
        {
            return new TypeBuilder(SourceGeneration.TypeKind.Class, name);
        }

        public static TypeBuilder Struct(IBuilder name)
        {
            return new TypeBuilder(SourceGeneration.TypeKind.Struct, name);
        }

        public static FieldBuilder Field(string type, string name)
        {
            return new FieldBuilder(Constant(type), Constant(name));
        }

        public static FieldBuilder Field(IBuilder type, IBuilder name)
        {
            return new FieldBuilder(type, name);
        }

        public static PropertyBuilder Property(string type, string name)
        {
            return new PropertyBuilder(Constant(type), Constant(name));
        }

        public static PropertyBuilder Property(IBuilder type, IBuilder name)
        {
            return new PropertyBuilder(type, name);
        }
        
        public static MethodBuilder Constructor(string value)
        {
            return new MethodBuilder(MethodKind.Constructor, Constant(value));
        }
        
        public static MethodBuilder Constructor(IBuilder value)
        {
            return new MethodBuilder(MethodKind.Constructor, value);
        }
        
        public static MethodBuilder Method(string name, string returnType = null)
        {
            var methodBuilder = new MethodBuilder(MethodKind.General, Constant(name));
            if (returnType != null) methodBuilder.WithReturnType(Constant(returnType));
            return methodBuilder;
        }
        
        public static MethodBuilder Method(IBuilder value)
        {
            return new MethodBuilder(MethodKind.General, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SourceBuilder T(this SourceBuilder sb, IBuilder builder)
        {
            builder.Build(sb);
            return sb;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SourceBuilder When(this SourceBuilder sb, bool when, IBuilder builder)
        {
            if (when) builder.Build(sb);
            return sb;
        }
        
        public static SourceBuilder AccessModifier(this SourceBuilder sb, AccessModifier type)
        {
            return sb.T(type switch
            {
                SourceGeneration.AccessModifier.Internal => "internal",
                SourceGeneration.AccessModifier.Public => "public",
                SourceGeneration.AccessModifier.Private => "private",
                SourceGeneration.AccessModifier.Protected => "protected",
                SourceGeneration.AccessModifier.ProtectedInternal => "protected internal",
                _ => "unknownAccessModifier"
            });
        }

        public static SourceBuilder TypeKind(this SourceBuilder sb, TypeKind type)
        {
            return sb.T(type switch
            {
                SourceGeneration.TypeKind.Class => "class",
                SourceGeneration.TypeKind.Struct => "struct",
                _ => "unknownTypeKind"
            });
        }
    }
}