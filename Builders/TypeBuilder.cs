using System;
using System.Collections.Generic;

namespace Abg.SourceGeneration
{
    public class TypeBuilder : BuilderBase
    {
        private AccessModifier accessModifier = AccessModifier.Public;
        private TypeKind Kind = TypeKind.Class;
        private IBuilder name;
        private bool isStatic;
        private bool isReadonly;
        private bool isUnsafe;
        private readonly ListBuilder body;
        private ListBuilder extends;

        public TypeBuilder(TypeKind kind, IBuilder name)
        {
            Kind = kind;
            this.name = name;
            body = new ListBuilder().WithoutDelimiter().EachElementOnNewLine();
        }

        public TypeBuilder WithAccessModifier(AccessModifier accessModifier)
        {
            this.accessModifier = accessModifier;
            return this;
        }

        public TypeBuilder Static()
        {
            isStatic = true;
            return this;
        }

        public TypeBuilder ReadOnly()
        {
            isReadonly = true;
            return this;
        }

        public TypeBuilder Unsafe()
        {
            isUnsafe = true;
            return this;
        }

        public TypeBuilder WithBody(Action<ListBuilder> list)
        {
            list(body);
            return this;
        }

        public TypeBuilder WithBody(params IBuilder[] builders)
        {
            body.WithElements(builders);
            return this;
        }

        public TypeBuilder WithBody(IEnumerable<IBuilder> builders)
        {
            body.WithElements(builders);
            return this;
        }
        
        public TypeBuilder When(bool predicate, Action<TypeBuilder> build)
        {
            if (!predicate) return this;
            build(this);
            return this;
        }

        public TypeBuilder WithExtends(Action<ListBuilder> list)
        {
            extends ??= new ListBuilder();
            list(extends);
            return this;
        }

        public TypeBuilder WithExtends(params IBuilder[] builders)
        {
            extends ??= new ListBuilder();
            extends.WithElements(builders);
            return this;
        }

        public TypeBuilder WithExtends(IEnumerable<IBuilder> builders)
        {
            extends ??= new ListBuilder();
            extends.WithElements(builders);
            return this;
        }

        protected override void BuildInternal(SourceBuilder sb)
        {
            using (sb.AccessModifier(accessModifier).T(" ")
                       .When(isStatic, "static ")
                       .When(isReadonly, "readonly ")
                       .When(isUnsafe, "unsafe ")
                       .TypeKind(Kind).T(" ").T(name).When(extends != null, " : ")
                       .When(extends != null, extends).Block())
            {
                sb.NewLine();
                body.Build(sb);
            }
        }
    }
}