using System;
using System.Collections.Generic;

namespace Abg.SourceGeneration
{
    public sealed class GenericBuilder : BuilderBase
    {
        private readonly IBuilder name;
        private readonly ListBuilder args;

        public GenericBuilder(IBuilder name)
        {
            this.name = name;
            args = new ListBuilder();
        }
        
        public GenericBuilder WithArgs(Action<ListBuilder> list)
        {
            list(args);
            return this;
        }
        
        public GenericBuilder WithArgs(params IBuilder[] builders)
        {
            args.WithElements(builders);
            return this;
        }

        public GenericBuilder WithArgs(IEnumerable<IBuilder> builders)
        {
            args.WithElements(builders);
            return this;
        }

        protected override void BuildInternal(SourceBuilder sb)
        {
            sb.T(name).T("<");
            args.Build(sb);
            sb.T(">");
        }
    }
}