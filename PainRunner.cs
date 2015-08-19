using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Collections;
using PainLang.Classes;
using PainLang.PainEngine.Classes;
using PainLang.OnpEngine.Models;
using PainLang.OnpEngine.Logic;

namespace PainLang
{
    public static class PainRunner
    {
        public static Object Eval(
            this PainProgram Program,
            IDictionary<String, Object> Parameters = null,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = CreateContext(Program, Parameters, StaticValues))
            {
                return Eval(context);
            }
        }

        public static Object EvalStep(
            this PainProgram Program,
            IDictionary<String, Object> Parameters = null,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = CreateContext(Program, Parameters, StaticValues, true))
            {
                return Eval(context);
            }
        }

        public static Object Eval(
            this PainContext PainContext)
        {
            while (true)
            {
                if (PainContext.IsFinished)
                {
                    if (PainContext.Error != null)
                        throw PainContext.Error;
                    break;
                }

                try
                {
                    Boolean result = PainLineRunner.
                        ExecuteNext(PainContext);

                    if (PainContext.BreakEveryLine && result)
                        break;
                }
                catch
                {
                    throw;
                }
            }
            return PainContext.Result;
        }

        ////////////////////////////////////////////////////////////////////

        public static PainObject Exec(
            this PainProgram Program,
            IDictionary<String, Object> Parameters = null,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = CreateContext(Program, Parameters, StaticValues, false))
            {
                return Exec(context);
            }
        }
        
        public static PainObject Exec(
            this PainContext PainContext)
        {
            while (true)
            {
                if (PainContext.IsFinished)
                    break;

                Boolean result = PainLineRunner.ExecuteNext(PainContext);

                if (PainContext.BreakEveryLine && result)
                    break;
            }
            return PainContext.GlobalObject;
        }

        //////////////////////////////////////////////

        public static Object InvokeObjectMethod(
            this PainObject Object,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> Values = null,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = InvokeObjectMethodGetContext(
                Object,
                Method,
                MethodParameters,
                Values,
                StaticValues))
            {
                return context.Eval();
            }
        }

        public static Object InvokeMethod(
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> Values = null,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = InvokeMethodGetContext(
                Method,
                MethodParameters,
                Values,
                StaticValues))
            {
                return context.Eval();
            }
        }

        public static Object InvokeObjectMethod(
            this PainObject Object,
            PainContext PainContext,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = InvokeObjectMethodGetContext(
                Object,
                PainContext,
                Method,
                MethodParameters,
                StaticValues))
            {
                return context.Eval();
            }
        }
        
        public static Object InvokeObjectMethod(
            this PainContext PainContext,
            PainObject Object,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = InvokeObjectMethodGetContext(
                Object,
                Method,
                MethodParameters,
                PainContext != null && PainContext.GlobalObject != null ? PainContext.GlobalObject.DynamicValues : null,
                StaticValues))
            {
                return context.Eval();
            }
        }

        /*public static Object InvokeMethod(
            this PainContext PainContext,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = InvokeMethodGetContext(
                PainContext,
                Method,
                MethodParameters,
                StaticValues))
            {
                return context.Eval();
            }
        }*/

        public static Object InvokeMethod(
            this PainContext PainContext,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> StaticValues = null)
        {
            using (PainContext context = InvokeMethodGetContext(
                Method,
                MethodParameters,
                PainContext != null && PainContext.GlobalObject != null ? PainContext.GlobalObject.DynamicValues : null,
                StaticValues))
            {
                return context.Eval();
            }
        }

        //////////////////////////////////////////////

        public static PainContext InvokeObjectMethodGetContext(
            this PainObject Object,
            PainContext PainContext,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> StaticValues = null)
        {
            return InvokeObjectMethodGetContext(
                Object,
                Method,
                MethodParameters,
                PainContext != null && PainContext.GlobalObject != null ? PainContext.GlobalObject.DynamicValues : null,
                StaticValues);
        }

        public static PainContext InvokeObjectMethodGetContext(
            this PainContext PainContext,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> StaticValues = null)
        {
            return InvokeObjectMethodGetContext(
                PainContext.GlobalObject,
                Method,
                MethodParameters,
                PainContext != null && PainContext.GlobalObject != null ? PainContext.GlobalObject.DynamicValues : null,
                StaticValues);
        }

        public static PainContext InvokeMethodGetContext(
            this PainContext PainContext,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> StaticValues = null)
        {
            return InvokeMethodGetContext(
                Method,
                MethodParameters,
                PainContext != null && PainContext.GlobalObject != null ? PainContext.GlobalObject.DynamicValues : null,
                StaticValues);
        }

        ////////////////////////////////////////////////////////

        private static PainContext InvokeObjectMethodGetContext(
            PainObject Object,
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> Values,
            IDictionary<String, Object> StaticValues)
        {
            PainContext newContext = CreateContext(
                new PainCodeLines(),
                Values,
                StaticValues,
                false, true);
            PainObject globalObject = newContext.GlobalObject;

            String objectName = IdGenerator.Generate();
            globalObject.DynamicValues[objectName] = Object; // much faster
            //newContext[objectName] = Object;

            ExpressionGroup expressionGroup = new ExpressionGroup(false);
            expressionGroup.MainExpression = new Expression();
            expressionGroup.MainExpression.IsOnpExecution = false;
            expressionGroup.MainExpression.Tokens = new ExpressionTokens();

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken(objectName, TokenType.VARIABLE));

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken('.', TokenType.OPERATOR));

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken(Method, TokenType.PROPERTY_NAME));

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken('@', TokenType.OPERATOR));

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken('(', TokenType.BRACKET_BEGIN));

            if (MethodParameters != null)
            {
                Int32 i = -1;
                foreach (Object parameter in MethodParameters)
                {
                    i++;
                    if (i > 0)
                    {
                        expressionGroup.MainExpression.Tokens.Add(
                            new ExpressionToken(',', TokenType.SEPARATOR));
                    }

                    String parameterName = IdGenerator.Generate();
                    globalObject.DynamicValues[parameterName] = parameter; // much faster
                    //newContext[parameterName] = parameter;

                    expressionGroup.MainExpression.Tokens.Add(
                        new ExpressionToken(parameterName, TokenType.VARIABLE));
                }
            }

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken(')', TokenType.BRACKET_END));

            PainCodeLine lineOfCode = new PainCodeLine()
            {
                ExpressionGroup = expressionGroup,
                Depth = 0,
                IsLineEmpty = false,
                OperatorType = EOperatorType.RETURN
            };
            newContext.GlobalState.Program.Lines.Add(lineOfCode);

            return newContext;
        }

        private static PainContext InvokeMethodGetContext(
            String Method,
            IList<Object> MethodParameters,
            IDictionary<String, Object> Values,
            IDictionary<String, Object> StaticValues)
        {
            PainContext newContext = CreateContext(
                new PainCodeLines(),
                Values,
                StaticValues,
                false, true);
            PainObject globalObject = newContext.GlobalObject;
            
            ExpressionGroup expressionGroup = new ExpressionGroup(false);
            expressionGroup.MainExpression = new Expression();
            expressionGroup.MainExpression.IsOnpExecution = false;
            expressionGroup.MainExpression.Tokens = new ExpressionTokens();

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken(Method, TokenType.VARIABLE));

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken('@', TokenType.OPERATOR));

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken('(', TokenType.BRACKET_BEGIN));

            if (MethodParameters != null)
            {
                Int32 i = -1;
                foreach (Object parameter in MethodParameters)
                {
                    i++;
                    if (i > 0)
                    {
                        expressionGroup.MainExpression.Tokens.Add(
                            new ExpressionToken(',', TokenType.SEPARATOR));
                    }

                    String parameterName = IdGenerator.Generate();
                    globalObject.DynamicValues[parameterName] = parameter; // much faster
                    //newContext[parameterName] = parameter;

                    expressionGroup.MainExpression.Tokens.Add(
                        new ExpressionToken(parameterName, TokenType.VARIABLE));
                }
            }

            expressionGroup.MainExpression.Tokens.Add(
                new ExpressionToken(')', TokenType.BRACKET_END));

            PainCodeLine lineOfCode = new PainCodeLine()
            {
                ExpressionGroup = expressionGroup,
                Depth = 0,
                IsLineEmpty = false,
                OperatorType = EOperatorType.RETURN
            };
            newContext.GlobalState.Program.Lines.Add(lineOfCode);

            return newContext;
        }

        //////////////////////////////////////////////

        public static PainContext CreateContext(
            this PainProgram Program,
            IDictionary<String, Object> Values = null,
            IDictionary<String, Object> StaticValues = null,
            Boolean BreakEveryLine = false,
            Boolean CopyParameters = false)
        {
            if (Values == null)
                Values = new Dictionary<String, Object>();

            foreach (PainMethod painMethod in Program.Methods)
                Values[painMethod.Name] = painMethod;

            foreach (PainClass painClass in Program.Classes)
                Values[painClass.Name] = painClass;

            return CreateContext(
                Program.Lines,
                Values,
                StaticValues,
                BreakEveryLine,
                CopyParameters);
        }

        private static PainContext CreateContext(
            PainCodeLines Lines,
            IDictionary<String, Object> Values,
            IDictionary<String, Object> StaticValues,
            Boolean BreakEveryLine = false,
            Boolean CopyParameters = false)
        {
            PainContext runContext = new PainContext(
                new PainProgram()
                {
                    ID = Guid.Empty,
                    Lines = Lines
                });
            runContext.BreakEveryLine = BreakEveryLine;

            if (runContext.CurrentState.Program.Lines.Count > 0)
                runContext.CurrentState.CurrentLineID = runContext.CurrentState.Program.Lines[0].ID;

            if (Values != null)
            {
                if (CopyParameters)
                {
                    if (Values is ICloneShallow)
                    {
                        runContext.CurrentState.Object.DynamicValues = (IDictionary<String, Object>)((ICloneShallow)Values).CloneShallow();
                    }
                    else
                    {
                        runContext.CurrentState.Object.DynamicValues = new MyDictionary<String, Object>(Values);
                    }
                }
                else
                {
                    runContext.CurrentState.Object.DynamicValues = Values;
                }
            }
            runContext.CurrentState.Object.StaticValues = StaticValues;

            return runContext;
        }


    }


}