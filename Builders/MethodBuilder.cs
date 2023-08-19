using System;
using System.Collections.Generic;

namespace Abg.SourceGeneration
{
    public class MethodBuilder : BuilderBase
    {
        private AccessModifier accessModifier = AccessModifier.Public;
        private bool isStatic;
        private IBuilder returnType = Builders.Void;
        private ListBuilder arguments = new ListBuilder();
        private ListBuilder baseArguments;
        private ListBuilder thisArguments;
        private readonly MethodKind kind;
        private readonly IBuilder name;
        private ListBuilder body;

        public MethodBuilder(MethodKind kind, IBuilder name)
        {
            this.kind = kind;
            this.name = name;
            body = new ListBuilder().WithoutDelimiter().EachElementOnNewLine();
        }

        public MethodBuilder WithReturnType(IBuilder builder)
        {
            returnType = builder;
            return this;
        }

        public MethodBuilder WithAccessModifier(AccessModifier accessModifier)
        {
            this.accessModifier = accessModifier;
            return this;
        }

        public MethodBuilder Static()
        {
            isStatic = true;
            return this;
        }
        
        public MethodBuilder WithArguments(Action<ListBuilder> list)
        {
            list(arguments);
            return this;
        }
        
        public MethodBuilder WithArguments(params IBuilder[] builders)
        {
            arguments.WithElements(builders);
            return this;
        }

        public MethodBuilder WithArguments(IEnumerable<IBuilder> builders)
        {
            arguments.WithElements(builders);
            return this;
        }
        
        public MethodBuilder WithBaseArguments(Action<ListBuilder> list)
        {
            baseArguments ??= new ListBuilder();
            list(baseArguments);
            return this;
        }
        
        public MethodBuilder WithBaseArguments(params IBuilder[] builders)
        {
            baseArguments ??= new ListBuilder();
            baseArguments.WithElements(builders);
            return this;
        }

        public MethodBuilder WithBaseArguments(IEnumerable<IBuilder> builders)
        {
            baseArguments ??= new ListBuilder();
            baseArguments.WithElements(builders);
            return this;
        }
        
        public MethodBuilder WithThisArguments(Action<ListBuilder> list)
        {
            thisArguments ??= new ListBuilder();
            list(thisArguments);
            return this;
        }
        
        public MethodBuilder WithThisArguments(params IBuilder[] builders)
        {
            thisArguments ??= new ListBuilder();
            thisArguments.WithElements(builders);
            return this;
        }

        public MethodBuilder WithThisArguments(IEnumerable<IBuilder> builders)
        {
            thisArguments ??= new ListBuilder();
            thisArguments.WithElements(builders);
            return this;
        }
        
        public MethodBuilder WithBody(Action<ListBuilder> list)
        {
            list(body);
            return this;
        }
        
        public MethodBuilder WithBody(params IBuilder[] builders)
        {
            body.WithElements(builders);
            return this;
        }

        public MethodBuilder WithBody(IEnumerable<IBuilder> builders)
        {
            body.WithElements(builders);
            return this;
        }
        
        public MethodBuilder When(bool predicate, Action<MethodBuilder> build)
        {
            if (!predicate) return this;
            build(this);
            return this;
        }

        protected override void BuildInternal(SourceBuilder sb)
        {
            switch (kind)
            {
                case MethodKind.General:
                    using (sb.AccessModifier(accessModifier).T(" ").When(isStatic, "static ").T(" ").T(returnType)
                               .T(" ").T(name).T("(").T(arguments).T(")").Block())
                    {
                        sb.NewLine();
                        body.Build(sb);
                    }
                    break;
                case MethodKind.Constructor:
                    if (isStatic)
                    {
                        sb = sb.T("static ").T(" ").T(name).T("()");
                        using (sb.Block())
                        {
                            sb.NewLine();
                            body.Build(sb);
                        }
                    }
                    else
                    {
                        sb = sb.AccessModifier(accessModifier).T(" ").T(name).T("(").T(arguments).T(")");
                        if (thisArguments != null)
                        {
                            sb.T(" : this(");
                            thisArguments.Build(sb);
                            sb.T(")");
                        }
                        if (baseArguments != null)
                        {
                            sb.T(" : base(");
                            baseArguments.Build(sb);
                            sb.T(")");
                        }
                        using (sb.Block())
                        {
                            sb.NewLine();
                            body.Build(sb);
                        }
                    }
                    break;
                case MethodKind.Destructor:
                    using (sb.T("~").T(name).T("()").Block())
                    {
                        sb.NewLine();
                        body.Build(sb);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}