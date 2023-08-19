namespace Abg.SourceGeneration
{
    public abstract class BuilderBase : IBuilder
    {
        private IBuilder prefix;
        private IBuilder suffix;

        public BuilderBase WithPrefix(IBuilder builder)
        {
            prefix = builder;
            return this;
        }

        public BuilderBase WithSuffix(IBuilder builder)
        {
            suffix = builder;
            return this;
        }

        protected abstract void BuildInternal(SourceBuilder sb);
        
        public void Build(SourceBuilder sb)
        {
            prefix?.Build(sb);
            BuildInternal(sb);
            suffix?.Build(sb);
        }
    }
}