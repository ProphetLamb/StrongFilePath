using System;
using HeaplessUtility;

namespace StrongFilePath
{
    public readonly partial struct FilePath
    {
        public ref struct Builder
        {
            private ValueStringBuilder _builder;

            public Builder(Span<char> initialBuffer)
            {
                _builder = new ValueStringBuilder(initialBuffer);
            }
            
            public Builder(int length)
            {
                _builder = new ValueStringBuilder(length);
            }

            public char this[int index] => _builder[index];

            public int Length => _builder.Length;

            public bool IsEmpty => 0 >= (uint)_builder.Length;

            public void Append(ReadOnlySpan<char> value)
            {
                _builder.Append(value);
            }

            public void Append(char ch)
            {
                _builder.Append(ch);
            }

            public void AppendExtension(ReadOnlySpan<char> extension)
            {
                if (extension.IsEmpty)
                {
                    return;
                }
                if (extension[0] != ExtensionSeparator)
                {
                    _builder.Append('.');
                }
                Append(extension);
            }

            public void Combine(ReadOnlySpan<char> pathSegment)
            {
                AppendPath(ref this, pathSegment);
            }

            public FilePath ToFilePath()
            {
                return _builder.ToString().ToFilePath();
            }

            internal static void AppendPath(ref Builder builder, ReadOnlySpan<char> pathSegment)
            {
                if (pathSegment.IsEmpty)
                {
                    return;
                }
                if (builder.Length == 0)
                {
                    builder.Append(pathSegment);
                    return;
                }

                bool hasTailingSeparator = DirectorySeparators.Contains(builder[builder.Length - 1]);
                bool hasLeadingSeparator = DirectorySeparators.Contains(pathSegment[0]);
                if (!hasTailingSeparator && !hasLeadingSeparator)
                {
                    builder.Append('\\');
                }

                builder.Append(pathSegment);
            }
        }
    }
}
