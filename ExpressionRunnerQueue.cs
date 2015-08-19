using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using PainLang.Exceptions;
using PainLang.PainEngine.Classes;
using PainLang.OnpEngine.Models;
using PainLang.OnpEngine.Logic;
using PainLang.Helpers;

namespace PainLang
{
    public static class ExpressionRunnerQueue
    {
        public static Boolean EvaluateQueue(
            PainContext PainContext)
        {
            ExpressionState expState = PainContext.CurrentExpressionState;
            ExpressionContext expContext = PainContext.CurrentExpressionContext;

            // policzenie nastepnego parametru
            if (expState.AreParametersCalculating)
            {
                expState.ParameterIndex++;
                if (expState.ParameterIndex < expState.ParameterTokens.Count)
                {
                    Boolean result = false;

                    ExpressionToken parameterToken = expState.ParameterTokens[expState.ParameterIndex];
                    if (parameterToken.TokenType == TokenType.VARIABLE)
                    {
                        result = ObjectValueGetter.EvaluateValue(
                            parameterToken.TokenName,
                            PainContext);
                    }
                    else if (parameterToken.TokenType == TokenType.VALUE)
                    {
                        ExpressionValue operationValue = StringHelper.
                            GetValueFromText(parameterToken.TokenChars);

                        Object value = (
                            operationValue == null ? null : operationValue.Value);

                        expState.Parameters.Add(value);
                    }
                    else
                    {
                        throw new PainIncorrectExpressionFormatException();
                    }

                    return result;
                }
            }

            // czy zakończyć i zapisać wynik
            if (expState.TokenIndex >= expState.Expression.Tokens.Count)
            {
                Object finResult = null;
                if (expState.ValueStack.Count > 0)
                    finResult = expState.ValueStack.Pop();

                expState.ValueStack.Clear();
                expState.Finished = true;
                expState.Result = InternalTypeConverter.ToOuter(finResult);

                expContext.Stack.Pop();

                if (expContext.Current != null)
                {
                    expContext.Current.PushValue(InternalTypeConverter.ToOuter(finResult));
                    return false;
                }
                else
                {
                    expContext.Result = InternalTypeConverter.ToOuter(finResult);
                    expContext.IsFinished = true;
                    return true;
                }
            }

            Boolean isFirstToken = expState.TokenIndex == 0;
            ExpressionToken token = expState.
                Expression.
                Tokens[expState.TokenIndex];

            ExpressionTokens sequence = new ExpressionTokens(
                new TokenizerQueue().
                    GetNextTokensOnSameLevel(expState.Expression.Tokens, expState.TokenIndex));

            Int32 originalSequenceCount = sequence.Count;
            sequence.RemoveBrackets();

            // wykonanie następnej operacji
            if (token.TokenType == TokenType.BRACKET_BEGIN)
            {
                IList<ExpressionToken> prevSequenceTokens = new TokenizerQueue().
                    GetPrevTokensOnSameLevel(expState.Expression.Tokens, expState.TokenIndex - 1);

                ExpressionTokens prev_sequence = prevSequenceTokens == null ?
                    null : new ExpressionTokens(prevSequenceTokens);

                // jeśli poprzedni operator to @
                if (
                    prev_sequence != null && prev_sequence.Count == 1 &&
                    OnpOnpTokenHelper.IsFunctionOperatorToken(prev_sequence[0]))
                {
                    Boolean result = false;

                    if (sequence.Count == 0 || expState.AreParametersCalculated)
                    {
                        Object obj = expState.ValueStack.Count == 1 ?
                            new EmptyObject() :
                            expState.ValueStack.Peek(1);

                        Object methodObject = expState.
                            ValueStack.
                            Peek(0);

                        result = EvaluatorForMethods.EvaluateMethod(
                            obj,
                            methodObject,
                            expState.Parameters,
                            PainContext);

                        expState.CleanParametersState();

                        expState.TokenIndex += originalSequenceCount;
                    }
                    else
                    {
                        expState.ParameterTokens = GetParameterTokens(sequence);
                        expState.ParameterIndex = -1;
                        expState.Parameters = new List<Object>();
                        result = false;
                    }

                    return result;
                }
                // jeśli poprzedni operator to .
                else if (
                    prev_sequence != null && prev_sequence.Count == 1 &&
                    OnpOnpTokenHelper.IsPropertyOperatorToken(prev_sequence[0]))
                {
                    throw new PainIncorrectExpressionFormatException();
                }
                // jeśli brak poprzedniego operatora
                else if (
                    prev_sequence == null &&
                    sequence.Count == 1)
                {
                    Boolean result = false;

                    if (sequence[0].TokenType == TokenType.VARIABLE)
                    {
                        result = ObjectValueGetter.EvaluateValue(
                            sequence[0].TokenName,
                            PainContext);
                    }
                    else if (sequence[0].TokenType == TokenType.VALUE)
                    {
                        ExpressionValue operationValue = StringHelper.
                            GetValueFromText(sequence[0].TokenChars);

                        Object value = (
                            operationValue == null ? null : operationValue.Value);

                        expState.PushValue(value);
                    }
                    else
                    {
                        throw new PainIncorrectExpressionFormatException();
                    }

                    expState.TokenIndex += originalSequenceCount; // -1;
                    return result;
                }
                else
                {
                    throw new PainInvalidExpressionException("Incorrect expression " + expState.Expression.Tokens.JoinToString(expState.TokenIndex) + "!");
                }
            }
            else if (token.TokenType == TokenType.VALUE)
            {
                ExpressionValue operationValue = StringHelper.
                    GetValueFromText(token.TokenChars);

                Object value = (
                    operationValue == null ? null : operationValue.Value);

                if (isFirstToken)
                {
                    expState.PushValue(value);
                }
                else
                {
                    Object prevValue = expState.ValueStack.Peek();

                    Object method = ObjectValueGetter.GetValueFromObject(
                        prevValue,
                        UniConvert.ToString(value));

                    expState.PushValue(method);
                }

                expState.TokenIndex += originalSequenceCount; // -1
            }
            else if (token.TokenType == TokenType.PROPERTY_NAME)
            {
                if (isFirstToken)
                {
                    throw new PainIncorrectExpressionFormatException();
                }
                else
                {
                    Object prevValue = expState.ValueStack.Peek();

                    Object value = ObjectValueGetter.GetValueFromObject(
                        prevValue,
                        token.TokenName);

                    expState.PushValue(value);
                }

                expState.TokenIndex += originalSequenceCount; // -1
            }
            else if (token.TokenType == TokenType.VARIABLE)
            {
                Boolean result = false;

                ExpressionToken next_token = expState.TokenIndex + 1 < expState.Expression.Tokens.Count ?
                    expState.Expression.Tokens[expState.TokenIndex + 1] :
                    null;

                Object prevValue = null;
                if (isFirstToken)
                {
                    prevValue = new EmptyObject();
                    /*result = ObjectValueGetter.EvaluateValueOrMethod(
                        new EMPTY_OBJECT(),
                        sequence[0].TokenName,
                        -1,
                        PainContext);*/
                }
                else
                {
                    prevValue = (
                        expState.ValueStack.Count == 0 ?
                            null :
                            expState.ValueStack.Peek());
                }

                Int32 paramCount = -1;

                // jeśli następny operator to operator wołania funkcji @ to pobieramy z niego liczbę parametrów dla funkcji
                if (OnpOnpTokenHelper.IsFunctionOperatorToken(next_token) &&
                    next_token.TokenData != null &&
                    next_token.TokenData.FunctionParametersCount != null)
                {
                    paramCount = next_token.TokenData.FunctionParametersCount.Value;
                }
                else
                {
                    paramCount = -1;
                }

                result = ObjectValueGetter.EvaluateValueOrMethod(
                    prevValue,
                    sequence[0].TokenName,
                    paramCount,
                    PainContext);

                if (PainContext.CurrentExpressionState.ValueStack.Peek() == null)
                {
                    ExpressionToken next_next_token = expState.TokenIndex + 2 < expState.Expression.Tokens.Count ?
                        expState.Expression.Tokens[expState.TokenIndex + 2] :
                        null;

                    if (next_token.TokenType == TokenType.OPERATOR &&
                        OnpOnpTokenHelper.IsPropertyOperatorToken(next_token) &&
                        next_next_token != null)
                    {

                    }
                    else
                    {
                        next_next_token = null;
                    }

                    if (paramCount < 0)
                    {
                        if (next_next_token != null)
                        {
                            ExpressionToken next_next_next_next_next_token = expState.TokenIndex + 5 < expState.Expression.Tokens.Count ?
                                expState.Expression.Tokens[expState.TokenIndex + 5] :
                                null;

                            if (next_next_token.TokenName == "__EXT_SET" && next_next_next_next_next_token != null)
                            {
                                throw new PainMethodNotFoundException("Cannot find property " + next_next_next_next_next_token.TokenName + " in undefined object '" + sequence[0].TokenName + "'");
                            }
                            else
                            {
                                throw new PainMethodNotFoundException("Cannot call method '" + next_next_token.TokenName + "' in undefined object '" + sequence[0].TokenName + "'");
                            }
                        }
                        else
                        {
                            throw new PainMethodNotFoundException("Undefined object '" + sequence[0].TokenName + "'");
                        }
                    }
                    else
                    {
                        throw new PainMethodNotFoundException("Undefined method '" + sequence[0].TokenName + "'!");
                    }
                }

                expState.TokenIndex += originalSequenceCount; // -1
                return result;
            }
            else if (token.TokenType == TokenType.OPERATOR)
            {
                if (!OnpOnpTokenHelper.IsFunctionOperatorToken(token) &&
                    !OnpOnpTokenHelper.IsPropertyOperatorToken(token))
                {
                    throw new PainIncorrectExpressionFormatException();
                }

                expState.TokenIndex += originalSequenceCount; // -1
            }

            if (expState.ValueStack.Peek() == null)
                return false;

            return false;
        }

        private static ExpressionTokens GetParameterTokens(
            ExpressionTokens tokenParameters)
        {
            ExpressionTokens parameterTokens = new ExpressionTokens();

            foreach (ExpressionToken token in tokenParameters)
            {
                if (token.TokenType == TokenType.VARIABLE)
                    parameterTokens.Add(token);

                else if (token.TokenType == TokenType.VALUE)
                    parameterTokens.Add(token);
            }

            return parameterTokens;
        }


    }
}