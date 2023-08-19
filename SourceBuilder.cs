using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Abg.SourceGeneration
{
    public class SourceBuilder
    {
        private StringBuilder builder = new StringBuilder();
        private int intendLevel = 0;
        private string intendText = "    ";

        public long Length => builder.Length;

        public IntendHandler Intend()
        {
            return new IntendHandler(this);
        }

        public BlockHandler Block()
        {
            return new BlockHandler(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SourceBuilder NewLine()
        {
            StartNewLineWithIntend();
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SourceBuilder T(char line)
        {
            builder.Append(line);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SourceBuilder T(string line)
        {
            builder.Append(line);
            return this;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SourceBuilder When(bool when, string line)
        {
            if (when) builder.Append(line);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SourceBuilder NewLine(string line)
        {
            StartNewLineWithIntend();
            builder.Append(line);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartNewLineWithIntend()
        {
            builder.AppendLine();
            for (int i = 0; i < intendLevel; i++)
            {
                builder.Append(intendText);
            }
        }

        public void Clear()
        {
            builder.Clear();
        }

        public override string ToString()
        {
            return builder.ToString();
        }

        public struct BlockHandler : IDisposable
        {
            private readonly SourceBuilder builder;
            private IntendHandler intend;

            public BlockHandler(SourceBuilder builder)
            {
                this.builder = builder;
                builder.NewLine("{");
                intend = builder.Intend();
            }

            public void Dispose()
            {
                intend.Dispose();
                builder.NewLine("}");
            }
        }

        public struct IntendHandler : IDisposable
        {
            private readonly SourceBuilder builder;

            public IntendHandler(SourceBuilder builder)
            {
                this.builder = builder;
                builder.intendLevel += 1;
            }

            public void Dispose()
            {
                if (builder != null)
                    builder.intendLevel -= 1;
            }
        }
    }
}