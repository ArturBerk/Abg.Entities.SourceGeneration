using System;
using System.Collections.Generic;

namespace Abg.SourceGeneration
{
    public class PropertyBuilder : BuilderBase
    {
        private bool isStatic;
        private AccessModifier accessModifier = AccessModifier.Public;
        private bool isByReference;
        private GetterOrSetterBuilder getter;
        private GetterOrSetterBuilder setter;

        private readonly IBuilder type;
        private readonly IBuilder name;
        
        public PropertyBuilder WithAccessModifier(AccessModifier accessModifier)
        {
            this.accessModifier = accessModifier;
            return this;
        }
        
        public PropertyBuilder Static()
        {
            isStatic = true;
            return this;
        }

        public PropertyBuilder ByReference()
        {
            isByReference = true;
            return this;
        }
        
        public PropertyBuilder WithByReferenceState(bool state)
        {
            isByReference = state;
            return this;
        }

        public PropertyBuilder WithSetter(Action<GetterOrSetterBuilder> setter)
        {
            this.setter = new GetterOrSetterBuilder("set");
            setter(this.setter);
            return this;
        }

        public PropertyBuilder WithGetter(Action<GetterOrSetterBuilder> getter)
        {
            this.getter = new GetterOrSetterBuilder("get");
            getter(this.getter);
            return this;
        }

        public PropertyBuilder(IBuilder type, IBuilder name)
        {
            this.type = type;
            this.name = name;
        }

        protected override void BuildInternal(SourceBuilder sb)
        {
            using (sb.AccessModifier(accessModifier).T(" ").When(isStatic, "static ")
                       .When(isByReference, "ref ").T(type).T(" ").T(name).Block())
            {
                if (getter != null)
                {
                    sb.NewLine();
                    getter.Build(sb);
                }
                if (setter != null)
                {
                    sb.NewLine();
                    setter.Build(sb);
                }
            }
        }
    }

    public class GetterOrSetterBuilder : BuilderBase
    {
        private readonly string type;
        private readonly ListBuilder body;

        public GetterOrSetterBuilder(string type)
        {
            this.type = type;
            body = new ListBuilder().WithoutDelimiter().EachElementOnNewLine();
        }
        
        public GetterOrSetterBuilder WithBody(Action<ListBuilder> list)
        {
            list(body);
            return this;
        }
        
        public GetterOrSetterBuilder WithBody(params IBuilder[] builders)
        {
            body.WithElements(builders);
            return this;
        }

        public GetterOrSetterBuilder WithBody(IEnumerable<IBuilder> builders)
        {
            body.WithElements(builders);
            return this;
        }

        protected override void BuildInternal(SourceBuilder sb)
        {
            using (sb.T(type).Block())
            {
                sb.NewLine();
                body.Build(sb);
            }
        }
    }
}