using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using PainLang.PainEngine.Classes;
using PainLang.OnpEngine.Logic;
using PainLang.OnpEngine.Symbols;
using PainLang.OnpEngine.Models;
using PainLang.Helpers;

namespace PainLang.PainEngine.Compiler
{
    public class PainCompiler
    {
        private static readonly char[] str_method = "def".ToCharArray();

        private static readonly char[] str_class = "class".ToCharArray();

        private static readonly char[] str_if = "if".ToCharArray();

        private static readonly char[] str_while = "while".ToCharArray();

        private static readonly char[] str_else = "else".ToCharArray();

        private static readonly char[] str_elif = "elif".ToCharArray();

        private static readonly char[] str_for = "for".ToCharArray();

        private static readonly char[] str_return = "return".ToCharArray();

        private static readonly char[] str_try = "try".ToCharArray();

        private static readonly char[] str_catch = "catch".ToCharArray();

        private static readonly char[] str_finally = "finally".ToCharArray();

        private static readonly char[] str_throw = "throw".ToCharArray();

        ////////////////////////////////////////////

        public PainCompiler()
        {

        }

        ////////////////////////////////////////////

        public PainProgram Compile(String Code)
        {
            return Compile(Code.ToCharArray());
        }

        public PainProgram Compile(IList<Char> Code)
        {
            CodeLines lines = GetLines(Code);

            PainProgram mainProgram = new PainProgram();
            List<PainProgram> methodStack = new List<PainProgram>();
            methodStack.Push(mainProgram);

            foreach (CodeLine line in lines)
            {
                Int32 currentDepth = -1;

                PainMethod method = null;
                method = GetMethodDefinition(line);
                if (method != null)
                    currentDepth = method.Depth;

                PainClass classDefinition = null;
                if (method == null)
                {
                    classDefinition = GetClassDefinition(line);
                    if (classDefinition != null)
                        currentDepth = classDefinition.Depth;
                }

                PainCodeLine codeLine = null;
                if (method == null && classDefinition == null)
                {
                    codeLine = GetCodeLine(line);
                    if (codeLine != null)
                        currentDepth = codeLine.Depth;
                }
                
                PainProgram currentMethod = methodStack.Peek();
                if (codeLine == null || !codeLine.IsLineEmpty)
                {
                    while (currentDepth < currentMethod.Depth ||
                        (currentDepth == currentMethod.Depth && classDefinition != null && classDefinition != currentMethod) ||
                        (currentDepth == currentMethod.Depth && method != null && method != currentMethod))
                    {
                        methodStack.Pop();
                        currentMethod = methodStack.Peek();
                    }
                }

                if (method != null)
                {
                    currentMethod.Methods.Remove_by_Name(method.Name);
                    currentMethod.Methods.Add(method);
                    methodStack.Push(method);
                    continue;
                }

                if (classDefinition != null)
                {
                    currentMethod.Classes.Remove_by_Name(classDefinition.Name);
                    currentMethod.Classes.Add(classDefinition);
                    methodStack.Push(classDefinition);
                    continue;
                }

                if (codeLine != null)
                {
                    if (codeLine.IsLineEmpty == false)
                        currentMethod.Lines.Add(codeLine);
                }
            }

            return mainProgram;
        }

        public PainMethod GetMethodDefinition(CodeLine line)
        {
            CodeLine trimmedLine = new CodeLine(line.TrimStart());

            // DEF: wykrycie definicji metody
            if (trimmedLine.Count > str_method.Length &&
                StringHelper.StrEquals(trimmedLine, str_method, true) &&
                Char.IsWhiteSpace(trimmedLine[str_method.Length]))
            {
                PainMethod method = new PainMethod();
                method.Depth = GetDepth(line) + 1; // zwiekszamy poziom metody

                IList<OnpMethodPart> methodParameters = MethodParser.
                    ExtractNames(trimmedLine.ToString().Substring(str_method.Length + 1), true);

                foreach (OnpMethodPart methodParameter in methodParameters)
                {
                    if (methodParameter.Part == EOnpMethodPart.METHOD_NAME)
                    {
                        method.Name = methodParameter.Code.ToUpper();
                    }
                    else if (methodParameter.Part == EOnpMethodPart.PARAMETER)
                    {
                        method.Parameters.Add(methodParameter.Code);
                    }
                }

                return method;
            }
            return null;
        }

        public PainClass GetClassDefinition(CodeLine line)
        {
            CodeLine trimmedLine = new CodeLine(line.TrimStart());

            // DEF: wykrycie definicji metody
            if (trimmedLine.Count > str_class.Length &&
                StringHelper.StrEquals(trimmedLine, str_class, true) &&
                Char.IsWhiteSpace(trimmedLine[str_class.Length]))
            {
                PainClass classDefinition = new PainClass();
                classDefinition.Depth = GetDepth(line) + 1; // zwiekszamy poziom klasy

                IList<OnpMethodPart> methodParameters = MethodParser.
                    ExtractNames(trimmedLine.ToString().Substring(str_class.Length + 1), true);

                foreach (OnpMethodPart methodParameter in methodParameters)
                {
                    if (methodParameter.Part == EOnpMethodPart.METHOD_NAME)
                    {
                        classDefinition.Name = methodParameter.Code.ToUpper();
                    }
                    else if (methodParameter.Part == EOnpMethodPart.PARAMETER)
                    {
                        classDefinition.Parameters.Add(methodParameter.Code);
                    }
                }

                return classDefinition;
            }
            return null;
        }

        public PainCodeLine GetCodeLine(CodeLine line)
        {
            return SetCodeLine(null, line);
        }

        public PainCodeLine SetCodeLine(PainCodeLine compiledLine, String line)
        {
            return SetCodeLine(compiledLine, new CodeLine(line));
        }

        public PainCodeLine SetCodeLine(PainCodeLine compiledLine, CodeLine line)
        {
            IList<Char> lineTrimmed = line.
                TrimStart().
                ToArray();

            IList<Char> lineBody = line;
            EOperatorType operatorType = EOperatorType.NONE;

            // IF: wykrycie definicji IF'a
            if (lineTrimmed.Count > str_if.Length &&
                   StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_if, true) &&
                   Char.IsWhiteSpace(lineTrimmed[str_if.Length]))
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.IF;

                lineBody = lineTrimmed.
                    Substring2(str_if.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }
            // WHILE: wykrycie definicji WHILE'a
            else if (lineTrimmed.Count > str_while.Length &&
                   StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_while, true) &&
                   Char.IsWhiteSpace(lineTrimmed[str_while.Length]))
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.WHILE;

                lineBody = lineTrimmed.
                    Substring2(str_while.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }
            // ELSE: wykrycie definicji ELSE'a
            else if (lineTrimmed.Count >= str_else.Length &&
                   StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_else, true))
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.ELSE;

                lineBody = lineTrimmed.
                    Substring2(str_else.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }
            // ELIF: wykrycie definicji ELIF'a
            else if (lineTrimmed.Count > str_elif.Length &&
                       StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_elif, true) &&
                       Char.IsWhiteSpace(lineTrimmed[str_elif.Length]))
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.ELIF;

                lineBody = lineTrimmed.
                    Substring2(str_elif.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }
            // RETURN: wykrycie definicji RETURN'a
            else if (lineTrimmed.Count > str_return.Length &&
                       StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_return, true) &&
                       Char.IsWhiteSpace(lineTrimmed[str_return.Length]))
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.RETURN;

                lineBody = lineTrimmed.
                    Substring2(str_return.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }
            // TRY: wykrycie definicji TRY'a
            else if (lineTrimmed.Count > str_try.Length &&
                       StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_try, true) /*&&
                       Char.IsWhiteSpace(lineTrimmed[str_try.Length])*/)
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.TRY;

                lineBody = lineTrimmed.
                    Substring2(str_try.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }
            // CATCH: wykrycie definicji CATCH'a
            else if (lineTrimmed.Count > str_catch.Length &&
                       StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_catch, true) /*&&
                       Char.IsWhiteSpace(lineTrimmed[str_catch.Length])*/)
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.CATCH;

                lineBody = lineTrimmed.
                    Substring2(str_catch.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }
            // FINALLY: wykrycie definicji FINALLY'a
            else if (lineTrimmed.Count > str_finally.Length &&
                       StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_finally, true) /*&&
                       Char.IsWhiteSpace(lineTrimmed[str_finally.Length])*/)
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.FINALLY;

                lineBody = lineTrimmed.
                    Substring2(str_finally.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }
            // THROW: wykrycie definicji THROW'a
            else if (lineTrimmed.Count > str_throw.Length &&
                       StringHelper.StrEquals(lineTrimmed.TrimStart().ToArray(), str_throw, true) /*&&
                       Char.IsWhiteSpace(lineTrimmed[str_THROW.Length])*/)
            {
                Int32 depth = GetDepth(line);
                operatorType = EOperatorType.THROW;

                lineBody = lineTrimmed.
                    Substring2(str_throw.Length).
                    TrimStart().
                    ToList();

                for (int i = 0; i < depth; i++)
                    lineBody.Insert(0, ' ');
                lineBody.TrimEnd(':');
            }

            ExpressionGroup expressionGroup = TokenizerInvariant.I.
                Compile(lineBody);

            if (compiledLine == null)
                compiledLine = new PainCodeLine();

            compiledLine.Code = line.ToString2();
            compiledLine.ExpressionGroup = expressionGroup;
            compiledLine.OperatorType = operatorType;
            compiledLine.IsLineEmpty = lineTrimmed.Count == 0;
            compiledLine.Depth += GetDepth(lineBody);

            return compiledLine;
        }

        ////////////////////////////////////////////

        private CodeLines GetLines(IList<Char> Chars)
        {
            CodeLines outChars = new CodeLines();

            CodeLine currentLine = new CodeLine();
            outChars.Add(currentLine);
            Boolean wasCommentStart = false;

            for (Int32 i = 0; i <= Chars.Count; i++)
            {
                OnpOnpStringFindResult firstNext = StringHelper.FirstNextIndex(
                    Chars, i,
                    new[] { PainLanguageSymbols.NewLineChars },
                    false);

                if (firstNext == null || firstNext.Index < 0)
                {
                    for (var j = i; j < Chars.Count; j++)
                    {
                        var ch = Chars[j];

                        if (ch == PainLanguageSymbols.CommentStartSymbol[0][0])
                            wasCommentStart = true;

                        if (!wasCommentStart)
                            currentLine.Add(ch);
                    }
                    break;
                }
                else
                {
                    if (PainLanguageSymbols.NewLineChars.Contains(firstNext.Chars))
                    {
                        for (var j = i; j < firstNext.Index; j++)
                        {
                            var ch = Chars[j];

                            if (ch == PainLanguageSymbols.CommentStartSymbol[0][0])
                                wasCommentStart = true;

                            if (!wasCommentStart)
                                currentLine.Add(ch);
                        }

                        i = firstNext.Index + firstNext.Chars.Length - 1;

                        currentLine = new CodeLine();
                        outChars.Add(currentLine);
                        wasCommentStart = false;
                    }
                }
            }

            return outChars;
        }

        private Int32 GetDepth(IList<Char> line)
        {
            int Depth = 0;
            for (var i = 0; i < line.Count; i++)
                if (Char.IsWhiteSpace(line[i]))
                {
                    Depth++;
                }
                else
                {
                    break;
                }
            return Depth;
        }
    }

}
