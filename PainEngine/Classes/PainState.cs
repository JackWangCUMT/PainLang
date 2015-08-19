using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using PainLang.OnpEngine.Models;

namespace PainLang.PainEngine.Classes
{
    [DataContract(IsReference = true)]
    public class PainState
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid ID { get; set; }

        //////////////////////////////////////////////

        [DataMember(EmitDefaultValue = false)]
        public PainProgram Program { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public String DisplayName { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public Guid ObjectID { get; set; }

        //////////////////////////////////////////////

        [DataMember(EmitDefaultValue = false)]
        public PainContextType ContextType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PainObject Object { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public PainObject ThisObject { get; set; }

        //[DataMember(EmitDefaultValue = false)]
        //public PainCodeLines Lines { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Int32 CurrentLineIndex { get; set; }

        //////////////////////////////////////////////

        [DataMember(EmitDefaultValue = false)]
        public ExpressionContext ExpressionContext { get; set; }

        //////////////////////////////////////////////

        [IgnoreDataMember]
        public Guid CurrentLineID
        {
            get
            {
                PainCodeLine currentLine = GetCurrentLine();
                return currentLine == null ? Guid.Empty : currentLine.ID;
            }
            set
            {
                if (Program == null)
                    return;

                PainCodeLine newCurrentLine = Program.Lines.Get_by_ID(value);
                this.CurrentLineIndex = newCurrentLine == null ? -1 : Program.Lines.IndexOf(newCurrentLine);
                this.ExpressionContext = null;

                if (CurrentLineChanged != null)
                    CurrentLineChanged.Invoke(this, new EventArgs());
            }
        }

        //////////////////////////////////////////////

        public event EventHandler CurrentLineChanged;

        //////////////////////////////////////////////

        /*public PainState(String DisplayName, Guid ObjectID, PainContextType ContextType)
        {
            this.DisplayName = DisplayName ?? "";
            this.ObjectID = ObjectID;
            this.ID = Guid.NewGuid();
            this.ContextType = ContextType;
            this.Object = new PainObject();
            this.Lines = new PainCodeLines();
            this.CurrentLineIndex = 0;
        }*/

        public PainState(PainProgram Program, PainContextType ContextType)
        {
            this.Program = Program;
            //this.DisplayName = DisplayName ?? "";
            //this.ObjectID = ObjectID;
            this.ID = Guid.NewGuid();
            this.ContextType = ContextType;
            this.Object = new PainObject();
            //this.Lines = new PainCodeLines();
            this.CurrentLineIndex = 0;
        }

        //////////////////////////////////////////////

        public PainCodeLine GetCurrentLine()
        {
            if (Program == null)
                return null;

            return
                CurrentLineIndex >= 0 && CurrentLineIndex < Program.Lines.Count ?
                Program.Lines[CurrentLineIndex] :
                null;
        }

        public PainCodeLines GetCurrentLines()
        {
            if (Program == null)
                return null;

            return Program.Lines;
        }

        //////////////////////////////////////////////

        public PainState Clone()
        {
            return (PainState)this.MemberwiseClone();
        }

        public void Clean()
        {
            Object = null;
            ThisObject = null;
            Program = null;
            if (ExpressionContext != null)
                ExpressionContext.Clean();
            ExpressionContext = null;
        }
    }

    public enum PainContextType
    {
        GLOBAL,
        METHOD,
        CLASS
    }
}