using System;
using System.Collections;
using System.Collections.Generic;

namespace StrongFilePath
{
    public struct PathSegmentEnumerator : IEnumerator<string>
    {
        private string? _filePath;
        private int _pos;
        private int _len;

        internal PathSegmentEnumerator(string filePath)
        {
            _filePath = filePath;
            _pos = 0;
            _len = 0;
        }

        public bool MoveNext()
        {
            if (_filePath is null || _pos + _len == _filePath.Length)
            {
                ResetToEnd();
                return false;
            }
            ReadOnlySpan<char> remaining = _filePath.AsSpan(_pos + _len);
            int consecutiveDirectorySeparators = 0;
            int pos = 0;
            while (pos < remaining.Length)
            {
                if (FilePath.DirectorySeparators.Contains(remaining[pos]))
                {
                    consecutiveDirectorySeparators += 1;
                }
                else if (consecutiveDirectorySeparators != 0)
                {
                    _pos += _len;
                    _len = pos;

                    if (_len == consecutiveDirectorySeparators)
                    {
                        return MoveNext();
                    }
                    return true;
                }

                pos += 1;
            }

            _pos += _len;
            _len = _filePath.Length - _pos;
            return true;
        }

        public bool MovePrevious()
        {
            if (_filePath is null || _pos == 0)
            {
                Reset();
                return false;
            }
            ReadOnlySpan<char> remaining = _filePath.AsSpan(0, _pos);
            int index = remaining.Length - 1;
            while (index >= 0)
            {
                char ch = remaining[index];
                index -= 1;
                if (!FilePath.DirectorySeparators.Contains(ch))
                {
                    break;
                }
            }

            while (index >= 0)
            {
                char ch = remaining[index];
                if (FilePath.DirectorySeparators.Contains(ch))
                {
                    break;
                }
                index -= 1;
            }
            _pos = index + 1;
            _len = remaining.Length - _pos;
            return true;
        }

        public void Reset()
        {
            _pos = 0;
            _len = 0;
        }

        public void ResetToEnd()
        {
            _pos = _filePath?.Length ?? 0;
            _len = 0;
        }

        public ReadOnlySpan<char> Current => _filePath.AsSpan(_pos, _len);

        string IEnumerator<string>.Current => Current.IsEmpty ? String.Empty : Current.ToString();
        
        object IEnumerator.Current => Current.IsEmpty ? String.Empty : Current.ToString();

        public void Dispose()
        {
            if (_filePath is null)
            {
                return;
            }

            Reset();
            _filePath = null;
        }
    }
}
