using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Runtime.Serialization;
using PainLang.PainEventArgs;
using PainLang.OnpEngine.Models;
using PainLang.OnpEngine.Internal;
using PainLang.Helpers;

namespace PainLang.PainEngine.Classes
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(PainMethod))]
    [KnownType(typeof(PainObject))]
    [KnownType(typeof(PainProgram))]
    [KnownType(typeof(PainClass))]
    [KnownType(typeof(ExpressionExtenderInfo))]
    [KnownType(typeof(ExpressionMethodInfo))]
    [KnownType(typeof(Undefined))]
    [KnownType(typeof(EmptyObject))]
    public class PainContext : IDisposable
    {
        [DataMember]
        public Guid ID { get; set; }

        //////////////////////////////////////////////

        [DataMember]
        public PainStates Stack { get; set; }

        [DataMember]
        public Boolean BreakEveryLine { get; set; }

        //////////////////////////////////////////////

        [DataMember]
        public Exception Error { get; set; }

        [DataMember]
        public Object Result { get; set; }

        [DataMember]
        public Boolean IsFinished { get; set; }

        //////////////////////////////////////////////

        [DataMember]
        public PainState CurrentState { get; set; }

        [DataMember]
        public PainState GlobalState { get; set; }

        //////////////////////////////////////////////

        [IgnoreDataMember]
        public PainObject GlobalObject
        {
            get { return GlobalState != null ? GlobalState.Object : null; }
        }

        public object this[string PropertyName]
        {
            get { return GlobalObject[PropertyName]; }
            set { GlobalObject[PropertyName] = value; }
        }

        //////////////////////////////////////////////

        [IgnoreDataMember]
        public ExpressionContext CurrentExpressionContext
        {
            get
            {
                return CurrentState != null ?
                    CurrentState.ExpressionContext :
                    null;
            }
        }

        [IgnoreDataMember]
        public ExpressionGroup CurrentExpressionGroup
        {
            get
            {
                return CurrentExpressionContext != null ?
                    CurrentExpressionContext.ExpressionGroup :
                    null;
            }
        }

        [IgnoreDataMember]
        public ExpressionState CurrentExpressionState
        {
            get
            {
                return CurrentExpressionContext != null ?
                    CurrentExpressionContext.Current :
                    null;
            }
        }

        //////////////////////////////////////////////

        //[IgnoreDataMember]
        public event EventHandler<PainProgramChangedEventArgs> OnProgramStart;

        //[IgnoreDataMember]
        public event EventHandler<PainProgramChangedEventArgs> OnProgramEnd;

        //[IgnoreDataMember]
        public event EventHandler<PainProgramErrorEventArgs> OnProgramError;

        //[IgnoreDataMember]
        public event EventHandler<PainErrorEventArgs> OnError;

        //////////////////////////////////////////////

        public PainContext(PainProgram MainProgram)
        {
            this.GlobalState = new PainState(
                MainProgram,
                PainContextType.GLOBAL);
            
            this.Stack = new PainStates();
            this.Stack.Push(this.GlobalState);
            this.CurrentState = this.GlobalState;

            this.ID = Guid.NewGuid();
        }

        //////////////////////////////////////////////

        //public PainState PushContext(String DisplayName, Guid ObjectID, PainContextType ContextType)
        public PainState PushContext(
            PainProgram Program,
            PainContextType ContextType,
            IList<Object> Parameters)
        {
            PainState state = new PainState(Program, ContextType); // DisplayName, ObjectID, ContextType);
            PainObject obj = state.Object;
            this.Stack.Push(state);
            this.CurrentState = state;

            List<PainMethodParam> finalParameters = new List<PainMethodParam>();
            Int32 index = -1;

            if (Program is PainMethod)
            {
                PainMethod method = (PainMethod)Program;
                foreach (String parameter in method.Parameters)
                {
                    index++;

                    Object parameterValue = Parameters == null ? null :
                        index < Parameters.Count ?
                            Parameters[index] :
                            new Undefined();

                    if (obj != null)
                    {
                        obj[parameter] = parameterValue;
                        finalParameters.Add(new PainMethodParam()
                        {
                            Name = parameter,
                            Value = parameterValue
                        });
                    }
                }
            }

            if (obj != null)
            {
                foreach (PainMethod painMethod in Program.Methods)
                {
                    if (state.Program is PainClass)
                    {
                        PainMethod newMethod = (PainMethod)painMethod.Clone();
                        newMethod.ParentObject = obj;
                        obj[newMethod.Name] = newMethod;
                    }
                    else
                    {
                        obj[painMethod.Name] = painMethod;
                    }
                }

                foreach (PainClass painClass in Program.Classes)
                {
                    obj[painClass.Name] = painClass;
                }
            }

            RaiseProgramStart(
                state,
                finalParameters);

            return state;
        }

        public PainState PopContext()
        {
            PainState currentState = this.Stack.Peek();

            if (currentState != null &&
                currentState.ContextType != PainContextType.GLOBAL)
            {
                RaiseProgramEnd(currentState);

                currentState.Clean();

                PainState popedState = this.Stack.Pop();
                this.CurrentState = this.Stack.Peek();

                return currentState;
            }
            return null;
        }

        //////////////////////////////////////////////

        public void RaiseProgramStart(PainState state, IList<PainMethodParam> finalParameters)
        {
            PainState currentState = this.Stack.Peek();

            if (this.OnProgramStart != null)
            {
                PainProgramChangedEventArgs args = new PainProgramChangedEventArgs()
                {
                    Context = this,
                    Program = state.Program,
                    State = state,
                    Parameters = finalParameters
                };
                this.OnProgramStart(this, args);
                args.Clean();
            }
        }

        public void RaiseProgramEnd(PainState currentState)
        {
            if (this.OnProgramEnd != null)
            {
                PainProgramChangedEventArgs args = new PainProgramChangedEventArgs()
                {
                    Context = this,
                    Program = currentState.Program,
                    State = currentState
                };
                this.OnProgramEnd(this, args);
                args.Clean();
            }
        }

        public void RaiseProgramError(PainState currentState)
        {
            if (this.OnProgramError != null)
            {
                PainProgramErrorEventArgs args = new PainProgramErrorEventArgs()
                {
                    Context = this,
                    Program = currentState.Program,
                    State = currentState
                };
                this.OnProgramError(this, args);
                args.Clean();
            }
        }

        public Boolean RaiseError(PainState currentState, Exception Error)
        {
            if (this.OnError != null)
            {
                PainErrorEventArgs args = new PainErrorEventArgs()
                {
                    Context = this,
                    Program = currentState.Program,
                    State = currentState,
                    Error = Error,
                    Handled = false
                };
                this.OnError(this, args);
                args.Clean();
                return args.Handled;
            }
            return false;
        }

        //////////////////////////////////////////////

        public virtual ExpressionValue GetValue(
            PainContext EvalContext,
            String Name,
            Boolean SeekForExtenders,
            Boolean SeekForMethods,
            Boolean SeekInContexts)
        {
            Name = Name.ToUpper();

            PainContext painContext = EvalContext as PainContext;
            if (painContext == null)
                return null;

            // szukanie extender'a
            if (SeekForExtenders)
            {
                ExpressionExtenderInfo extender = BuildinExtenders.FindByName(Name);
                if (extender != null)
                    return new ExpressionValue(extender);
            }

            // szukanie metody
            if (SeekForMethods)
            {
                ExpressionMethodInfo method = BuildinMethods.FindByName(Name);
                if (method != null)
                    return new ExpressionValue(method);
            }

            if (SeekInContexts)
            {
                // szukanie po innych zmiennych
                if (painContext.CurrentState != painContext.GlobalState)
                {
                    if (painContext.CurrentState.Object != null &&
                        painContext.CurrentState.Object.Contains(Name))
                    {
                        return new ExpressionValue(painContext.CurrentState.Object[Name]);
                    }
                }

                if (painContext.GlobalState.Object != null &&
                    painContext.GlobalState.Object.Contains(Name))
                {
                    return new ExpressionValue(painContext.GlobalState.Object[Name]);
                }
            }

            /*for (var i = painContext.Stack.IndexOf(painContext.Current); i >= 0; i--)
            {
                PainState context = painContext.Stack[i];

                if ((context == painContext.Current || context.ContextType == PainContextType.GLOBAL) &&
                    context.Object != null &&
                    context.Object.Contains(Name))
                {
                    return new ExpressionValue(context.Object[Name]);
                }
            }*/

            // szukanie po globalnych zmiennych
            /*if (painContext.GlobalContext.ContextType == PainContextType.GLOBAL)
            {
                if (painContext.GlobalContext.Object.Contains(Name))
                    return new ExpressionValue(painContext.GlobalContext.Object[Name]);
            }*/

            return null;
        }

        public virtual Boolean SetValue(PainContext EvalContext, String Name, Object Value)
        {
            Name = Name.ToUpper();

            PainContext painContext = EvalContext as PainContext;
            if (painContext == null)
                return false;

            painContext.CurrentState.Object[Name] = Value;

            return true;
        }

        public void Dispose()
        {
            if (Stack != null)
            {
                foreach (PainState state in Stack)
                {
                    if (state == null || state == GlobalState)
                        continue;

                    state.Clean();
                }
                Stack.Clear();
                Stack = null;
            }

            /*if (GlobalState != null)
            {
                GlobalState.Clean();
                GlobalState = null;
            }*/

            /*if (CurrentState != null && CurrentState != GlobalState)
            {
                CurrentState.Clean();
                CurrentState = null;
            }*/

            CurrentState = null;
            Result = null;
            Error = null;

            /*GlobalState;
            CurrentState;
            Result;
            Error;
            Stack;*/
        }
    }

    public class PainContextes : List<PainContext>
    {

    }
}