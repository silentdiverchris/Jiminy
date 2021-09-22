using Jiminy.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jiminy.Classes
{
    /// <summary>
    /// Represents the content of a file
    /// </summary>
    internal class FileContent
    {
        private string? _fullFileName = null;
        private string[]? _lines = null;
        private int _currentLine = -1;
        private int _lineCount = 0;

        internal FileContent(string fullFileName)
        {
            _fullFileName = fullFileName;

            if (fullFileName.IsExistingFileName())
            {
                _lines = File.ReadAllLines(fullFileName);
                _lineCount = _lines.Length;
            }
        }

        internal int LineCount => _lineCount;
        internal int CurrentLineNumber => _currentLine;
        internal string? FullFileName => _fullFileName;

        internal FileLine? GetNextLine(bool skipEmptyLines)
        {
            if (_lineCount > 0 && _lines is not null && _currentLine < _lineCount)
            {
                _currentLine++;

                while (skipEmptyLines && _currentLine < _lineCount && string.IsNullOrEmpty(_lines[_currentLine]))
                {
                    _currentLine++;
                }

                if (_currentLine < _lineCount)
                {
                    return new FileLine(_lines[_currentLine], _currentLine);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        internal FileLine? GetLine(int lineNumber)
        {
            if (lineNumber > 0 && lineNumber < _lineCount)
            {
                return new FileLine(_lines![lineNumber], lineNumber);
            }
            else
            {
                return null;
            }
        }

        internal string ConcatenateSubsequentLines(string untilMarker, bool resumeAfter = true, int maxLineCount = 20, int? fromLine = null, string? prefix = null, string? linePrefix = null, string? lineSuffix = null, string? suffix = null)
        {
            int lineNo = fromLine ?? _currentLine + 1;

            StringBuilder sb = new(200);

            FileLine? line = GetLine(lineNo);

            while (lineNo < _lineCount && lineNo < _currentLine + maxLineCount)
            {
                if (line is null)
                {
                    throw new Exception($"PeekSubsequentLines found null line at #{lineNo}, that shouldn't be possible");
                }

                if (line!.Text.StartsWith(untilMarker))
                {
                    break;
                }
                else
                {
                    sb.Append($"{linePrefix}{line.Text}{lineSuffix}");
                }

                line = GetLine(++lineNo);
            }

            if (resumeAfter)
            {
                _currentLine = lineNo++;
            }

            return $"{prefix}{sb}{suffix}";
        }
    }

    internal class FileLine
    {
        internal FileLine(string text, int lineNumber)
        {
            Text = text;
            LineNumber = lineNumber;
        }

        internal string Text { get; private set; }
        internal int LineNumber { get; private set; }
        internal bool NotEmpty => Text.NotEmpty();
    }
}
