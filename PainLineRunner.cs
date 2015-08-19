using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Reflection;
using PainLang.PainEngine.Classes;
using PainLang.OnpEngine.Models;
using PainLang.PainEngine.Extenders;
using PainLang.OnpEngine.Logic;
using PainLang.Helpers;

namespace PainLang
{
    public static class PainLineRunner
    {
        public static Boolean ExecuteNext(
            PainContext PainContext)
        {
            try
            {
                Object currentValue = null;
                PainState currentState = PainContext.
                    CurrentState;

                Boolean? checkResult = CheckIfFinishedOrEmptyLine(
                    PainContext,
                    currentState);

                if (checkResult != null)
                    return checkResult.Value;

                Boolean? executeResult = ExecuteCalculations(
                    PainContext,
                    currentState,
                    out currentValue);

                if (executeResult != null)
                    return executeResult.Value;

                Boolean gotoResult = GotoNextLine(
                    PainContext,
                    currentState,
                    currentValue);

                return gotoResult;
            }
            catch (Exception ex)
            {
                Exception error = (ex is TargetInvocationException ? ex.InnerException : ex);

                Boolean result = false;

                PainContext.Error = error;

                PainState currentState = PainContext.
                    CurrentState;

                // próba obsługi błędu 
                Boolean handled = PainContext.RaiseError(
                    currentState,
                    error);

                if (!handled)
                {
                    result = GotoCatch(
                        PainContext,
                        error);
                }
                else
                {
                    PainContext.Error = null;
                    result = true;
                }

                if (PainContext.IsFinished && PainContext.Error != null)
                    throw PainContext.Error;

                return result;
            }
        }

        private static Boolean? CheckIfFinishedOrEmptyLine(
            PainContext PainContext,
            PainState currentState)
        {
            if (currentState == null || PainContext.IsFinished)
            {
                PainContext.IsFinished = true;
                return true;
            }

            PainCodeLines lines = currentState.GetCurrentLines();
            PainCodeLine currentLine = currentState.GetCurrentLine();

            if (currentLine == null)
            {
                return ExitCurrentContext(
                    PainContext);
            }

            // jeśli linia jest pusta to przechodzimy do nastepnej
            if (currentLine.IsLineEmpty)
            {
                PainCodeLine nextLine = lines.NextLine(currentLine);
                if (nextLine == null)
                {
                    return ExitCurrentContext(
                        PainContext);
                }
                else
                {
                    currentState.CurrentLineID = nextLine.ID;
                }
                return true;
            }
            return null;
        }

        private static Boolean? ExecuteCalculations(
            PainContext PainContext,
            PainState currentState,
            out Object Result)
        {
            Result = null;
            PainCodeLines lines = currentState.GetCurrentLines();
            PainCodeLine currentLine = currentState.GetCurrentLine();

            // wykonanie kalkulacji
            if (currentLine.ContainsAnyExpressions() &&
                currentLine.OperatorType != EOperatorType.ELSE)
            {
                if (PainContext.CurrentState.ExpressionContext == null)
                    PainContext.CurrentState.ExpressionContext = new ExpressionContext(currentLine.ExpressionGroup);

                if (PainContext.CurrentState.ExpressionContext != null &&
                    PainContext.CurrentState.ExpressionContext.IsFinished)
                {
                    Result = PainContext.CurrentState.ExpressionContext.Result;
                    PainContext.CurrentState.ExpressionContext = null;
                }
                else
                {
                    try
                    {
                        Boolean result = ExpressionRunner.NextStep(
                            PainContext);

                        return result;
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return null;
        }

        //////////////////////////////////////////////

        private static Boolean GotoNextLine(
            PainContext PainContext,
            PainState currentState,
            Object currentValue)
        {
            try
            {
                PainCodeLines lines = currentState.GetCurrentLines();
                PainCodeLine currentLine = currentState.GetCurrentLine();

                // jesli return to konczymy
                if (currentLine.OperatorType == EOperatorType.RETURN)
                {
                    return ExitCurrentContext(
                        PainContext,
                        currentValue);
                }
                // throw błędu
                else if (currentLine.OperatorType == EOperatorType.THROW)
                {
                    if (currentValue is Exception)
                    {
                        throw (Exception)currentValue;
                    }
                    else
                    {
                        String message = UniConvert.ToString(currentValue ?? "");
                        throw String.IsNullOrEmpty(message) ? new Exception() : new Exception(message);
                    }

                    /*return ExitCurrentContext(
                        PainContext,
                        new Exception(message));*/
                }

                if (currentLine.OperatorType == EOperatorType.WHILE ||
                    currentLine.OperatorType == EOperatorType.IF ||
                    currentLine.OperatorType == EOperatorType.ELIF)
                {
                    Boolean conditionResult = currentValue.IfTrue();
                    if (conditionResult)
                    {
                        PainCodeLine nextLine = lines.NextOnSameOrHigher(currentLine);
                        if (nextLine != null)
                        {
                            currentState.CurrentLineID = nextLine.ID;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        PainCodeLine nextLine = lines.NextOnSameOrLower(currentLine);
                        if (nextLine == null)
                        {
                            return ExitCurrentContext(
                                PainContext);
                        }
                        else
                        {
                            if (nextLine.Depth < currentLine.Depth)
                            {
                                while (
                                    nextLine != null &
                                    (nextLine.OperatorType == EOperatorType.ELSE ||
                                    nextLine.OperatorType == EOperatorType.ELIF /*||
                                nextLine.OperatorType == EOperatorType.FINALLY*/))
                                {
                                    nextLine = lines.ExitParentIf(nextLine);

                                    if (nextLine == null)
                                        break;
                                }

                                if (nextLine == null)
                                {
                                    return ExitCurrentContext(
                                        PainContext);
                                }

                                if (nextLine.Depth < currentLine.Depth)
                                {
                                    //PainCodeLine prevIf = lines.
                                    //    PrevLineWithLessDepth(currentLine, l => l.OperatorType == EOperatorType.IF || l.OperatorType == EOperatorType.ELIF);

                                    while (true)
                                    {
                                        PainCodeLine prevConditionLine = lines.
                                            PrevLineWithLessDepth(
                                                currentLine,
                                                l => l.OperatorType == EOperatorType.IF || l.OperatorType == EOperatorType.ELIF || l.OperatorType == EOperatorType.ELSE || l.OperatorType == EOperatorType.WHILE);

                                        if (prevConditionLine != null &&
                                            prevConditionLine.Depth >= nextLine.Depth &&
                                            prevConditionLine.OperatorType == EOperatorType.WHILE)
                                        {
                                            currentState.CurrentLineID = prevConditionLine.ID;
                                            break;
                                        }
                                        else if (prevConditionLine != null)
                                        {
                                            currentLine = prevConditionLine;
                                        }
                                        else
                                        {
                                            currentState.CurrentLineID = nextLine.ID;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    currentState.CurrentLineID = nextLine.ID;
                                }
                            }
                            else
                            {
                                currentState.CurrentLineID = nextLine.ID;
                            }
                        }
                    }
                }
                else if (
                    currentLine.OperatorType == EOperatorType.TRY ||
                    currentLine.OperatorType == EOperatorType.ELSE)
                {
                    PainCodeLine nextLine = lines.NextOnSameOrHigher(currentLine);
                    if (nextLine != null)
                    {
                        currentState.CurrentLineID = nextLine.ID;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (
                    (currentLine.OperatorType == EOperatorType.FINALLY))
                {
                    throw new NotImplementedException("FINALLY");
                }
                else if (
                    (currentLine.OperatorType == EOperatorType.CATCH))
                {
                    if (PainContext.Error != null)
                    {
                        PainContext.Error = null;
                        PainCodeLine nextLine = lines.NextOnSameOrHigher(currentLine);
                        if (nextLine != null)
                        {
                            currentState.CurrentLineID = nextLine.ID;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        PainCodeLine nextLine = lines.NextOnSameOrLower(currentLine);
                        if (nextLine != null)
                        {
                            currentState.CurrentLineID = nextLine.ID;
                        }
                        else
                        {
                            return ExitCurrentContext(
                                PainContext);
                        }
                    }
                }
                else if (currentLine.OperatorType == EOperatorType.NONE)
                {
                    PainCodeLine nextLine = lines.NextLine(currentLine);
                    if (nextLine != null)
                    {
                        while (
                            nextLine != null &
                            (nextLine.OperatorType == EOperatorType.ELSE ||
                            nextLine.OperatorType == EOperatorType.ELIF /*||
                        nextLine.OperatorType == EOperatorType.FINALLY*/))
                        {
                            nextLine = lines.ExitParentIf(nextLine);

                            if (nextLine == null)
                            {
                                return ExitCurrentContext(
                                    PainContext);
                            }
                        }

                        if (nextLine == null)
                        {
                            return ExitCurrentContext(
                                PainContext);
                        }

                        if (nextLine.Depth < currentLine.Depth)
                        {
                            //PainCodeLine prevIf = lines.
                            //    PrevLineWithLessDepth(currentLine, l => l.OperatorType == EOperatorType.IF || l.OperatorType == EOperatorType.ELIF);

                            while (true)
                            {
                                PainCodeLine prevConditionLine = lines.
                                    PrevLineWithLessDepth(
                                        currentLine,
                                        l => l.OperatorType == EOperatorType.IF || l.OperatorType == EOperatorType.ELIF || l.OperatorType == EOperatorType.ELSE || l.OperatorType == EOperatorType.WHILE);

                                if (prevConditionLine != null &&
                                    prevConditionLine.Depth >= nextLine.Depth &&
                                    prevConditionLine.OperatorType == EOperatorType.WHILE)
                                {
                                    currentState.CurrentLineID = prevConditionLine.ID;
                                    break;
                                }
                                else if (prevConditionLine != null)
                                {
                                    currentLine = prevConditionLine;
                                }
                                else
                                {
                                    currentState.CurrentLineID = nextLine.ID;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            currentState.CurrentLineID = nextLine.ID;
                        }
                    }
                    // jeśli ostatnia linia i jesteśmy w while'u
                    else
                    {
                        //PainCodeLine prevIf = lines.
                        //    PrevLineWithLessDepth(currentLine, l => l.OperatorType == EOperatorType.IF || l.OperatorType == EOperatorType.ELIF);

                        while (true)
                        {
                            PainCodeLine prevConditionLine = lines.
                                PrevLineWithLessDepth(
                                    currentLine,
                                    l => l.OperatorType == EOperatorType.IF || l.OperatorType == EOperatorType.ELIF || l.OperatorType == EOperatorType.ELSE || l.OperatorType == EOperatorType.WHILE);

                            if (prevConditionLine != null &&
                                prevConditionLine.OperatorType == EOperatorType.WHILE)
                            {
                                currentState.CurrentLineID = prevConditionLine.ID;
                                break;
                            }
                            else if (prevConditionLine != null)
                            {
                                currentLine = prevConditionLine;
                            }
                            else
                            {
                                return ExitCurrentContext(
                                    PainContext);
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        private static Boolean GotoCatch(
            PainContext PainContext,
            Exception exception)
        {
            while (true)
            {
                PainState currentState = PainContext.
                    CurrentState;

                // reset dla kontekstu obliczeń, ponieważ przechodzimy do catch'a
                currentState.ExpressionContext = null;

                PainCodeLines lines = currentState.GetCurrentLines();
                PainCodeLine currentLine = currentState.GetCurrentLine();

                // poszukanie poprzedniego catch'a
                PainCodeLine prevCatch = lines.
                    PrevLineWithLessDepth(currentLine, l => l.OperatorType == EOperatorType.CATCH);

                // poszukanie poprzedniego try'a
                PainCodeLine prevTry = lines.
                    PrevLineWithLessDepth(currentLine, l => l.OperatorType == EOperatorType.TRY);

                if (prevTry == null)
                {
                    ExitCurrentContext(
                        PainContext);

                    if (PainContext.IsFinished)
                        break;
                }
                // jeśli znalazł try'a i nie jesteśmy w catch'u
                else if (prevTry.Depth < currentLine.Depth &&
                        (prevCatch == null || lines.IndexOf(prevCatch) < lines.IndexOf(prevTry)))
                {
                    PainCodeLine nextCatch = lines.NextOnSameOrLower(
                        prevTry,
                        i => i.OperatorType == EOperatorType.CATCH);

                    if (nextCatch != null)
                    {
                        ExpressionToken variableForException =
                            nextCatch.ExpressionGroup != null &&
                            nextCatch.ExpressionGroup.MainExpression != null &&
                            nextCatch.ExpressionGroup.MainExpression.Tokens != null &&
                            nextCatch.ExpressionGroup.MainExpression.Tokens.Count > 0 ?
                                nextCatch.
                                    ExpressionGroup.
                                    MainExpression.
                                    Tokens.
                                    FirstOrDefault(i => i.TokenType != TokenType.BRACKET_BEGIN) :
                                null;

                        currentState.CurrentLineID = nextCatch.ID;

                        if (variableForException != null && !String.IsNullOrEmpty(variableForException.TokenName))
                            currentState.Object[variableForException.TokenName] = exception;

                        break;
                    }
                    else
                    {
                        ExitCurrentContext(
                            PainContext);

                        if (PainContext.IsFinished)
                            break;
                    }
                }
                else
                {
                    ExitCurrentContext(
                        PainContext);

                    if (PainContext.IsFinished)
                        break;
                }
            }
            return false;
        }

        //////////////////////////////////////////////

        private static Boolean ExitCurrentContext(
            PainContext PainContext,
            Object Result)
        {
            PainState context = PainContext.CurrentState;
            context.CurrentLineID = Guid.Empty;

            // jeśli został tylko ostatni główny context
            if (PainContext.Stack.Count == 1)
            {
                PainContext.Result = Result;
                PainContext.IsFinished = true;
            }
            else
            {
                PainContext.PopContext();
                if (PainContext.CurrentExpressionState != null)
                    PainContext.CurrentExpressionState.PushValue(Result);
            }

            return true;
        }

        private static Boolean ExitCurrentContext(
            PainContext PainContext)
        {
            PainState state = PainContext.CurrentState;
            state.CurrentLineID = Guid.Empty;

            Object result = null;

            if (state != null &&
                state.ContextType == PainContextType.CLASS)
            {
                result = state.Object;
            }

            // jeśli został tylko ostatni główny context
            if (PainContext.Stack.Count == 1)
            {
                PainContext.Result = result;
                PainContext.IsFinished = true;
            }
            else
            {
                PainContext.PopContext();
                if (PainContext.CurrentExpressionState != null)
                    PainContext.CurrentExpressionState.PushValue(result);
            }

            return true;
        }

    }

}