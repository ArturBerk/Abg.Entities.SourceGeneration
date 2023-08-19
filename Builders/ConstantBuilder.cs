using System;

namespace Abg.SourceGeneration
{
    public class ConstantBuilder : IBuilder
    {
        private readonly string value;

        public ConstantBuilder(string value)
        {
            this.value = value;
        }
        
        public void Build(SourceBuilder sb)
        {
            sb.T(value);
        }

        public static explicit operator ConstantBuilder(string value)
        {
            return new ConstantBuilder(value);
        }
    }

    public class DelegateBuilder : IBuilder
    {
        private readonly Action<SourceBuilder> builder;

        public DelegateBuilder(Action<SourceBuilder> builder)
        {
            this.builder = builder;
        }

        public void Build(SourceBuilder sb)
        {
            builder(sb);
        }
    }
}