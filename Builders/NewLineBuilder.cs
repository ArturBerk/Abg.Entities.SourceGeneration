namespace Abg.SourceGeneration
{
    public class NewLineBuilder : IBuilder
    {
        public void Build(SourceBuilder sb)
        {
            sb.NewLine();
        }
    }
    
    public class EmptyBuilder : IBuilder
    {
        public void Build(SourceBuilder sb)
        {
            sb.T("");
        }
    }
}