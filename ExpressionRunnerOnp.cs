using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using PainLang.PainEngine.Classes;
using PainLang.OnpEngine.Models;
using PainLang.OnpEngine.Logic;
using PainLang.OnpEngine.Symbols;
using PainLang.Helpers;

namespace PainLang
{
    public static class ExpressionRunnerOnp
    {
        public static Boolean EvaluateOnp(
            PainContext PainContext)
        {
            Boolean result = false;
            ExpressionState expState = PainContext.CurrentExpressionState;
            ExpressionContext expContext = PainContext.CurrentExpressionContext;

            // czy zakończyć i zapisać wynik
            if (expState.TokenIndex >= expState.Expression.OnpTokens.Count)
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

            ExpressionToken token = expState.Expression.OnpTokens[expState.TokenIndex];

            // wykonanie następnej operacji
            if (token.TokenType == TokenType.VALUE)
            {
                ExpressionValue operationValue = StringHelper.
                    GetValueFromText(token.TokenChars);

                Object value = operationValue == null ? null :
                    InternalTypeConverter.ToInner(operationValue.Value);

                expState.PushValue(value);
            }
            else if (token.TokenType == TokenType.PROPERTY_NAME)
            {
                expState.PushValue(token.TokenName);
            }
            if (token.TokenType == TokenType.VARIABLE)
            {
                result = ObjectValueGetter.EvaluateValue(
                    token.TokenName,
                    PainContext);
            }
            else if (token.TokenType == TokenType.OPERATOR)
            {
                Object valueA = InternalTypeConverter.ToInner(
                    expState.ValueStack.Pop());

                Object valueB = InternalTypeConverter.ToInner(
                    expState.ValueStack.Pop());

                Object value = null;
                OperatorType operatorType = OperatorTypeHelper.
                    GetOperationType(token.TokenChars);

                try
                {
                    value = OperationHelper.Do(
                        operatorType,
                        valueB,
                        valueA);

                    expState.PushValue(value);
                }
                catch
                {
                    throw;
                }
            }

            expState.TokenIndex++;
            return result;
        }
    }

}