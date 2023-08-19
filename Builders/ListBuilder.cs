using System;
using System.Collections.Generic;

namespace Abg.SourceGeneration
{
    public sealed class ListBuilder : BuilderBase
    {
        private List<IBuilder> builders = new List<IBuilder>();
        private string delimiter = ", ";
        private bool eachElementOnNewLine = false;
        private bool intend;
        private bool newLineBefore;
        private bool newLineAfter;

        public ListBuilder WithoutDelimiter()
        {
            delimiter = null;
            return this;
        }
        
        public ListBuilder WithDelimiter(string d)
        {
            delimiter = d;
            return this;
        }

        public ListBuilder NewLineBefore()
        {
            newLineBefore = true;
            return this;
        }

        public ListBuilder NewLineAfter()
        {
            newLineAfter = true;
            return this;
        }

        public ListBuilder EachElementOnNewLine()
        {
            eachElementOnNewLine = true;
            return this;
        }

        public ListBuilder WithIntend()
        {
            intend = true;
            return this;
        }

        public ListBuilder When(bool predicate, Action<ListBuilder> build)
        {
            if (!predicate) return this;
            build(this);
            return this;
        }

        public ListBuilder WithElements(params IBuilder[] builders)
        {
            this.builders.AddRange(builders);
            return this;
        }

        public ListBuilder WithElements(IEnumerable<IBuilder> builders)
        {
            this.builders.AddRange(builders);
            return this;
        }

        protected override void BuildInternal(SourceBuilder sb)
        {
            SourceBuilder.IntendHandler intendHandler = default;
            if (intend)
                intendHandler = sb.Intend();
            if (newLineBefore) sb.NewLine();
            for (int i = 0; i < builders.Count; i++)
            {
                if (i > 0)
                {
                    if (delimiter != null)
                        sb.T(delimiter);
                    if (eachElementOnNewLine)
                        sb.NewLine();
                }

                builders[i].Build(sb);
            }
            intendHandler.Dispose();
            if (newLineAfter) sb.NewLine();
        }
    }
}