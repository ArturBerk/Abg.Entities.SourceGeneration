namespace Abg.SourceGeneration
{
    public class FieldBuilder : BuilderBase
    {
        private readonly IBuilder type;
        private readonly IBuilder name;
        private AccessModifier accessModifier = AccessModifier.Public;
        private bool isStatic;
        private bool isReadonly;

        public FieldBuilder WithAccessModifier(AccessModifier accessModifier)
        {
            this.accessModifier = accessModifier;
            return this;
        }
        
        public FieldBuilder Static()
        {
            isStatic = true;
            return this;
        }
        
        public FieldBuilder ReadOnly()
        {
            isReadonly = true;
            return this;
        }
        
        public FieldBuilder(IBuilder type, IBuilder name)
        {
            this.type = type;
            this.name = name;
        }

        protected override void BuildInternal(SourceBuilder sb)
        {
            sb.AccessModifier(accessModifier).T(" ").When(isStatic, "static ")
                .When(isReadonly, "readonly ").T(type).T(" ").T(name).T(";");
        }
    }
}