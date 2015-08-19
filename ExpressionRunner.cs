using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using PainLang.PainEngine.Classes;
using PainLang.OnpEngine.Models;

namespace PainLang
{
    public static class ExpressionRunner
    {
        public static Boolean NextStep(
            PainContext PainContext)
        {
            if (PainContext == null || PainContext.CurrentState == null)
                return true;

            ExpressionContext curExpressionContext = PainContext.
                CurrentState.
                ExpressionContext;

            ExpressionGroup curExpressionGroup = curExpressionContext.
                ExpressionGroup;

            if (curExpressionContext == null ||
                curExpressionContext.IsFinished ||
                curExpressionContext.Current == null)
                return true;

            if (curExpressionContext.Current.Expression.IsOnpExecution)
            {
                return ExpressionRunnerOnp.EvaluateOnp(
                    PainContext);
            }
            else
            {
                return ExpressionRunnerQueue.EvaluateQueue(
                    PainContext);
            }
        }

    }

    public class OnpExpressionResult
    {
        public Object Result;

        public Boolean Finished;

        public PainProgram ProgramToExecute;
    }


}