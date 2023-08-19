using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Abg.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using static Abg.SourceGeneration.Builders;

namespace Abg.Entities
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var entityTypes = GatherEntityTypes(context.Compilation.Assembly);

            var sb = new SourceBuilder();
            foreach (INamedTypeSymbol entityType in entityTypes)
            {
                sb.Clear();
                var (entityName, entitiesName) = ToEntityName(entityType);

                var properties = entityType.GetMembers()
                    .OfType<IPropertySymbol>().ToList();

                var entityFieldName = default(string);
                var componentTypes = new List<Component>();
                var index = 0;
                foreach (IPropertySymbol property in properties)
                {
                    if (property.Type.ToDisplayString() == "Abg.Entities.Entity")
                    {
                        entityFieldName = property.Name;
                    }
                    else
                    {
                        componentTypes.Add(new Component(property, index++));
                    }
                }

                var excludedTypes = entityType.GetAttributes()
                    .Where(a => a.AttributeClass.ToDisplayString() != "Abg.Entities.ExcludeComponentAttribute")
                    .Select(a => (ITypeSymbol)a.ConstructorArguments[0].Value)
                    .ToList();

                var entityConstructor = Constructor(entityName).WithArguments(arguments =>
                    arguments.EachElementOnNewLine().WithIntend().NewLineBefore()
                        .When(entityFieldName != null, builder => builder.WithElements(Constant("Abg.Entities.Entity entity")))
                        .WithElements(componentTypes.Select(c =>
                                Argument(c.Type.IsValueType ? $"Abg.Entities.ComponentRef<{c.TypeName}>" : c.TypeName, c.ComponentFieldName)
                            )
                        )
                )
                    .When(entityFieldName != null, builder => builder.WithBody(Constant($"this.{entityFieldName} = entity;")))
                    .WithBody(
                    componentTypes.Select(c =>
                        Assign($"this.{c.ComponentFieldName}", c.ComponentFieldName)
                    )
                );

                var entityStruct = Struct(entityName).ReadOnly()
                    .When(entityFieldName != null, builder => builder.WithBody(Constant($"public readonly Abg.Entities.Entity {entityFieldName};")))
                    .WithBody(
                        componentTypes.Select(c =>
                            Field(c.Type.IsValueType ? $"Abg.Entities.ComponentRef<{c.TypeName}>" : c.TypeName, c.ComponentFieldName)
                                .ReadOnly().WithAccessModifier(AccessModifier.Private)
                        )
                    )
                    .WithBody(
                        Empty,
                        entityConstructor,
                        Empty
                    )
                    .WithBody(
                        componentTypes.Select(c => Property(c.TypeName, c.PropertyName)
                            .WithByReferenceState(c.Type.IsValueType)
                            .WithGetter(getter =>
                                getter.WithBody(Constant(c.Type.IsValueType
                                        ? $"return ref {c.ComponentFieldName}.Value;"
                                        : $"return {c.ComponentFieldName};"))
                                    .WithPrefix(SB(sb =>
                                            sb.T("[MethodImpl(MethodImplOptions.AggressiveInlining)]").NewLine()
                                        )
                                    )
                            )
                        )
                    );

                var entitiesConstructor = Constructor(entitiesName)
                    .WithArguments(Argument("EntityWorld", "world"))
                    .WithBaseArguments(
                        Constant("world"),
                        List().EachElementOnNewLine().WithIntend().NewLineAfter().NewLineBefore()
                            .WithElements(componentTypes.Select(c => Constant($"typeof({c.TypeName})")))
                            .WithPrefix(Constant("new Type[] {"))
                            .WithSuffix(Constant("}")),
                        List().EachElementOnNewLine().WithIntend().NewLineAfter().NewLineBefore()
                            .WithElements(excludedTypes.Select(c => Constant($"typeof({c.ToDisplayString()})")))
                            .WithPrefix(Constant("new Type[] {"))
                            .WithSuffix(Constant("}"))
                    );

                var enumeratorConstructor = Constructor("Enumerator")
                    .WithArguments(Argument(entitiesName, "entities"))
                    .WithThisArguments()
                    .WithBody(
                        Constant("collections = entities.PrepareCollections();"),
                        Constant("collectionIndex = 0;"),
                        Constant("indexInCollection = -1;")
                    );

                var enumeratorMoveNextMethod = Method("MoveNext").WithReturnType(Constant("bool"))
                    .WithBody(
                        Constant("if (collectionIndex >= collections.Count) return false;"),
                        Constant(
                            "IEntityCollection activeCollection = collections[collectionIndex];"),
                        Constant("if (++indexInCollection < activeCollection.Count)")
                    )
                    .WithBody(
                        Lines().WithIntend().NewLineBefore().NewLineAfter()
                            .WithElements(
                                componentTypes.Select(c =>
                                    Constant(
                                        $"{c.ComponentFieldName} = activeCollection.GetComponents<{c.TypeName}>();"))
                            )
                            .WithElements(Constant("return true;"))
                            .WithPrefix(Constant("{"))
                            .WithSuffix(Constant("}"))
                    )
                    .WithBody(
                        Constant("indexInCollection = -1;"),
                        Constant("++collectionIndex;"),
                        Constant("return MoveNext();")
                    );

                var enumeratorCurrentProperty = Property(entityName, "Current").WithGetter(getter =>
                {
                    getter.WithBody(List().WithIntend().NewLineAfter().NewLineBefore()
                        .EachElementOnNewLine()
                        .When(entityFieldName != null, builder => builder.WithElements(Constant($"collections[collectionIndex].GetEntity(indexInCollection)")))
                        .WithElements(componentTypes.Select(c =>
                            Constant(c.Type.IsValueType
                                ? $"new Abg.Entities.ComponentRef<{c.TypeName}>({c.ComponentFieldName}, indexInCollection)"
                                : $"{c.ComponentFieldName}[indexInCollection]")))
                        .WithPrefix(Constant($"return new {entityName}("))
                        .WithSuffix(Constant(");")));
                });

                var enumeratorStruct = Struct("Enumerator").WithExtends(GenericType($"IEnumerator", entityName))
                    .WithBody(
                        componentTypes.Select(c => Field(GenericType("ComponentAccessor", c.TypeName),
                            Constant(c.ComponentFieldName)))
                    )
                    .WithBody(
                        Field(GenericType("IReadOnlyList", "IEntityCollection"), Constant("collections"))
                            .WithAccessModifier(AccessModifier.Private),
                        Empty,
                        Field("int", "collectionIndex"),
                        Field("int", "indexInCollection"),
                        Empty,
                        enumeratorConstructor,
                        Empty,
                        enumeratorCurrentProperty,
                        Empty,
                        Constant("object IEnumerator.Current => Current;"),
                        Empty,
                        enumeratorMoveNextMethod,
                        Empty,
                        Method("Reset")
                            .WithBody(
                                Constant("collectionIndex = -1;"),
                                Constant("indexInCollection = -1;")
                            ),
                        Empty,
                        Method("Dispose")
                            .WithBody(
                                Constant("Reset();")
                            )
                    );

                var entitiesClass = Class(entitiesName).WithExtends(Constant("Abg.Entities.Entities")).WithBody(
                    Method("FromWorld", entitiesName).Static()
                        .WithArguments(Argument("Abg.Entities.EntityWorld", "world"))
                        .WithBody(Constant($"return new {entitiesName}(world);")),
                    Empty,
                    entitiesConstructor,
                    Empty,
                    Method("GetEnumerator", "Enumerator").WithBody(Constant("return new Enumerator(this);")),
                    Empty,
                    enumeratorStruct
                );

                Lines(
                    Using("System"),
                    Using("System.Collections"),
                    Using("System.Collections.Generic"),
                    Using("System.Runtime.CompilerServices"),
                    Using("Abg.Entities"),
                    Empty,
                    Namespace(entityType.ContainingNamespace.ToDisplayString()).WithBody(
                        entityStruct,
                        Empty,
                        entitiesClass
                    )
                ).Build(sb);

//                 sb.NewLine(@"using System;
// using System.Collections;
// using System.Collections.Generic;");
//
//                 using (sb.NewLine("namespace ").T(entityType.ContainingNamespace.ToDisplayString())
//                            .Block())
//                 {
//                     using (sb.NewLine("public readonly struct ").T(entityName).T(" : ").TypeFullname(entityType)
//                                .Block())
//                     {
//                         foreach (var component in componentTypes)
//                         {
//                             sb.NewLine("private readonly ").T(component.ComponentFieldTypeName).T(" ")
//                                 .T(component.ComponentFieldName).T(";");
//                         }
//
//                         sb.NewLine("public ").T(entityName).T("(");
//                         var isFirst = true;
//                         foreach (var component in componentTypes)
//                         {
//                             if (!isFirst)
//                             {
//                                 sb.T(", ");
//                             }
//
//                             sb.T(component.ComponentFieldTypeName).T(" ").T(component.ComponentFieldName);
//
//                             isFirst = false;
//                         }
//
//                         using (sb.T(")").Block())
//                         {
//                             foreach (var component in componentTypes)
//                             {
//                                 sb.NewLine("this.").T(component.ComponentFieldName).T(" = ")
//                                     .T(component.ComponentFieldName).T(";");
//                             }
//                         }
//
//                         foreach (var component in componentTypes)
//                         {
//                             using (sb.NewLine("public ").When(component.IsByReference, "ref ").T(component.TypeName)
//                                        .T(" ")
//                                        .T(component.PropertyName).Block())
//                             {
//                                 sb.NewLine(
//                                     "[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
//                                 sb.NewLine("get => ").When(component.IsByReference, "ref ")
//                                     .T(component.ComponentFieldName).T(".Value;");
//                             }
//                         }
//                     }
//
//                     var entitiesName = entityName + "Entities";
//                     using (sb.NewLine("public class ").T(entitiesName).T(" : Abg.Entities.Entities").Block())
//                     {
//                         sb.NewLine("public ").T(entitiesName)
//                             .T("(Abg.Entities.EntityWorld world) : base(world, new System.Type[]");
//
//                         using (sb.Block())
//                         {
//                             var isFirst = true;
//                             foreach (Component component in componentTypes)
//                             {
//                                 if (!isFirst) sb.T(", ");
//                                 sb.NewLine("typeof(").T(component.TypeName).T(")");
//                                 isFirst = false;
//                             }
//                         }
//
//                         sb.T(", new System.Type[]");
//                         using (sb.Block())
//                         {
//                             var isFirst = true;
//                             foreach (ITypeSymbol component in excludedTypes)
//                             {
//                                 if (!isFirst) sb.T(", ");
//                                 sb.NewLine("typeof(").T(component.ToDisplayString()).T(")");
//                                 isFirst = false;
//                             }
//                         }
//
//                         sb.T(") {}");
//
//                         sb.NewLine("public Enumerator GetEnumerator() { return new Enumerator(this); }");
//
//                         sb.NewLine(@"
//         public struct Enumerator : IEnumerator<");
//                         sb.T(entityName);
//                         sb.T(@">
//         {
//             private IReadOnlyList<Abg.Entities.IEntityCollection> collections;
//             private int collectionIndex;
//             private int indexInCollection;
//
//             public Enumerator(");
//                         sb.T(entitiesName);
//                         sb.T(@" entities) : this()
//             {
//                 collections = entities.PrepareCollections();
//                 collectionIndex = 0;
//                 indexInCollection = -1;
//             }
//
//             public void Reset()
//             {
//                 collectionIndex = -1;
//                 indexInCollection = -1;
//             }
//
//             object IEnumerator.Current => Current;
//
//             public void Dispose()
//             {
//                 collectionIndex = -1;
//                 indexInCollection = -1;
//             }
// ");
//                         using (sb.NewLine("public bool MoveNext()").Block())
//                         {
//                             sb.T(@"
//                 if (collectionIndex >= collections.Count) return false;
//                 IEntityCollection activeCollection = collections[collectionIndex];
//                 if (++indexInCollection < activeCollection.Count)
//                 {");
//                             foreach (var component in componentTypes)
//                             {
//                                 sb.NewLine(component.ComponentFieldName).T(" = activeCollection.GetComponents<")
//                                     .T(component.TypeName).T(">();");
//                             }
//
//                             sb.T(@"
//                     return true;
//                 }
//
//                 indexInCollection = -1;
//                 ++collectionIndex;
//                 return MoveNext();");
//                         }
//
//                         foreach (var component in componentTypes)
//                         {
//                             sb.NewLine("private Abg.Entities.IComponents<").T(component.TypeName).T("> ")
//                                 .T(component.ComponentFieldName).T(";");
//                         }
//
//                         sb.NewLine(@"
//             public ");
//                         sb.T(entityName);
//                         sb.T(@" Current =>
//                 new(");
//                         {
//                             var isFirst = true;
//                             foreach (Component component in componentTypes)
//                             {
//                                 if (!isFirst) sb.T(", ");
//                                 sb.NewLine(component.ComponentFieldName).T(".GetComponent(indexInCollection)");
//                                 isFirst = false;
//                             }
//                         }
//                         sb.NewLine(@"
//                 );
// ");
//                         sb.NewLine("}");
//                     }
//                 }

                context.AddSource($"Generated_{entityType.ContainingNamespace.ToDisplayString()}_{entityName}.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8));

                sb.Clear();
            }
        }

        private class Component
        {
            public ITypeSymbol Type;
            public string TypeName;
            public string PropertyName;
            public string ComponentFieldName;
            public string ComponentFieldTypeName;
            public bool IsByReference;

            public Component(IPropertySymbol propertySymbol, int index)
            {
                Type = propertySymbol.Type;
                IsByReference = propertySymbol.ReturnsByRef;
                TypeName = Type.ToDisplayString();
                PropertyName = propertySymbol.Name;
                ComponentFieldName = "component_" + index; //TypeName.Replace(".", "_");
                ComponentFieldTypeName = $"ComponentRef<{TypeName}>";
            }
        }

        private (string, string) ToEntityName(INamedTypeSymbol entityType)
        {
            var name = entityType.Name;
            if (name.StartsWith("I")) name = name.Substring(1);
            if (name.EndsWith("Entity")) name = name.Substring(0, name.Length - 6);
            var entityName = name + "Entity";
            var entitiesName = name + "Entities";
            return (entityName, entitiesName);
        }

        private ISet<INamedTypeSymbol> GatherEntityTypes(IAssemblySymbol assemblySymbol)
        {
            const string EntityInterface = "Abg.Entities.IEntity";
            var typesToGenerate = new HashSet<INamedTypeSymbol>();

            foreach (INamedTypeSymbol typeSymbol in GetAllTypes(assemblySymbol))
            {
                if (!typeSymbol.Interfaces.Any(i => i.ToDisplayString() == EntityInterface)) continue;
                typesToGenerate.Add(typeSymbol);
            }

            return typesToGenerate;

            static IEnumerable<INamedTypeSymbol> GetAllTypes(IAssemblySymbol assemblySymbol)
            {
                var collector = new ExportedTypesCollector(CancellationToken.None);
                assemblySymbol.Accept(collector);

                foreach (INamedTypeSymbol typeSymbol in collector.GetPublicTypes())
                {
                    yield return typeSymbol;
                }
            }
        }
    }
}