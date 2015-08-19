using PainLang.PainEngine.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PainLang.PainEngine.Extenders
{
    public static class PainCodeLinesExtender
    {
        public static PainCodeLine NextLine(
            this PainCodeLines Lines,
            PainCodeLine StartLine)
        {
            Int32 depth = (StartLine == null ? 0 : StartLine.Depth);
            Int32 index = (StartLine == null ? 0 : Lines.IndexOf(StartLine));

            if (index < 0)
                return null;

            for (var i = index + 1; i < Lines.Count; i++)
            {
                PainCodeLine line = Lines[i];
                if (!line.IsLineEmpty)
                    return line;
            }

            return null;
        }

        public static PainCodeLine PrevLineWithLessDepth(
            this PainCodeLines Lines,
            PainCodeLine StartLine,
            Func<PainCodeLine, Boolean> Predicate)
        {
            Int32 depth = (StartLine == null ? 0 : StartLine.Depth);
            Int32 index = (StartLine == null ? 0 : Lines.IndexOf(StartLine));

            if (index < 0)
                return null;

            for (var i = index - 1; i >= 0; i--)
            {
                PainCodeLine line = Lines[i];
                if (!line.IsLineEmpty && line.Depth < depth)
                    if (Predicate == null || Predicate(line))
                        return line;
            }

            return null;
        }

        public static PainCodeLine ExitParentIf(
            this PainCodeLines Lines,
            PainCodeLine StartLine)
        {
            Int32 depth = (StartLine == null ? 0 : StartLine.Depth);
            Int32 index = (StartLine == null ? 0 : Lines.IndexOf(StartLine));

            if (index < 0)
                return null;

            PainCodeLine line = StartLine;
            while (true)
            {
                line = NextLine(
                    Lines,
                    line);

                if (line == null)
                    break;

                if (line.Depth < StartLine.Depth)
                    return line;

                if (line.Depth == StartLine.Depth &&
                    line.OperatorType != EOperatorType.ELIF &&
                    line.OperatorType != EOperatorType.ELSE)
                {
                    return line;
                }
            }

            return null;
        }

        public static PainCodeLine NextOnSameOrLower(
            this PainCodeLines Lines,
            PainCodeLine StartLine,
            Func<PainCodeLine, Boolean> Predicate = null)
        {
            Int32 depth = (StartLine == null ? 0 : StartLine.Depth);
            Int32 index = (StartLine == null ? 0 : Lines.IndexOf(StartLine));

            if (index < 0)
                return null;

            for (var i = index + 1; i < Lines.Count; i++)
            {
                PainCodeLine line = Lines[i];
                if (!line.IsLineEmpty && line.Depth <= depth)
                    if (Predicate == null || Predicate(line))
                        return line;
            }

            return null;
        }

        public static PainCodeLine NextOnSameOrHigher(
            this PainCodeLines Lines,
            PainCodeLine StartLine)
        {
            Int32 depth = (StartLine == null ? 0 : StartLine.Depth);
            Int32 index = (StartLine == null ? 0 : Lines.IndexOf(StartLine));

            if (index < 0)
                return null;

            for (var i = index + 1; i < Lines.Count; i++)
            {
                PainCodeLine line = Lines[i];
                if (!line.IsLineEmpty)
                    if (line.Depth > depth)
                        return line;
                    else if (line.Depth == depth)
                        return line;
            }

            return null;
        }
    }
}
