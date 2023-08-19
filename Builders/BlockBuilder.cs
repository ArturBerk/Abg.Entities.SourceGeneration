using System;
using System.Collections.Generic;

namespace Abg.SourceGeneration
{
    public sealed class BlockBuilder : BuilderBase
    {
        private readonly IBuilder name;
        private readonly ListBuilder body;

        public BlockBuilder(IBuilder name)
        {
            this.name = name;
            body = new ListBuilder().WithoutDelimiter().EachElementOnNewLine();
        }
        
        public BlockBuilder WithBody(Action<ListBuilder> list)
        {
            list(body);
            return this;
        }
        
        public BlockBuilder WithBody(params IBuilder[] builders)
        {
            body.WithElements(builders);
            return this;
        }

        public BlockBuilder WithBody(IEnumerable<IBuilder> builders)
        {
            body.WithElements(builders);
            return this;
        }

        protected override void BuildInternal(SourceBuilder sb)
        {
            using (sb.T(name).Block())
            {
                sb.NewLine();
                body.Build(sb);
            }
        }
    }
}