﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;
using PainLang.OnpEngine.Logic;
#if PCL
using System.Linq2;
#endif

namespace PainLang.OnpEngine.Models
{
    [DataContract(IsReference = true)]
    public class ExpressionToken
    {
        [IgnoreDataMember]
        private IList<Char> tokenChars;

        ////////////////////////////////////////////////////

        [DataMember(EmitDefaultValue = false)]
        public TokenType TokenType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public String TokenName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Int32 Priority { get; set; }

        [IgnoreDataMember]
        public IList<Char> TokenChars
        {
            get
            {
                if (tokenChars == null && TokenName != null)
                    tokenChars = TokenName.ToList();
                return tokenChars;
            }
            set { tokenChars = value; }
        }

        ////////////////////////////////////////////////////

        [DataMember(EmitDefaultValue = false)]
        public Int32? TokenLength { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public OnpTokenData TokenData { get; set; }

        ////////////////////////////////////////////////////

        public ExpressionToken(String Token, TokenType TokenType)
        {
            this.Set(Token);
            this.TokenType = TokenType;
        }

        public ExpressionToken(Char TokenChar, TokenType TokenType)
        {
            this.Set(new[] { TokenChar });
            this.TokenType = TokenType;
        }

        public ExpressionToken(IList<Char> TokenChars, TokenType TokenType)
        {
            this.Set(TokenChars);
            this.TokenType = TokenType;
        }

        ////////////////////////////////////////////////////

        public Int32 GetFinalTokenLength()
        {
            return TokenLength == null ? (TokenChars != null ? TokenChars.Count : 0) : TokenLength.Value;
        }

        public void Set(String TokenName, Boolean CorrectPriority = true)
        {
            this.TokenChars = TokenName.ToArray();
            this.TokenName = TokenName;

            if (CorrectPriority)
                this.Priority = OnpOnpTokenHelper.GetPriority(this);
        }

        public void Set(IList<Char> TokenName, Boolean CorrectPriority = true)
        {
            this.TokenChars = TokenName;
            this.TokenName = new String(this.TokenChars.ToArray());

            if (CorrectPriority)
                this.Priority = OnpOnpTokenHelper.GetPriority(this);
        }

        ////////////////////////////////////////////////////

        public override string ToString()
        {
            return String.Format(
                "{0} {1}",
                TokenType,
                TokenName);
        }

        public ExpressionToken Clone()
        {
            ExpressionToken item = (ExpressionToken)this.MemberwiseClone();
            item.TokenChars = this.TokenChars.ToArray();
            if (item.TokenData != null)
                item.TokenData = item.TokenData.Clone();
            return item;
        }
    }

    [DataContract(IsReference = true)]
    public class OnpTokenData
    {
        [DataMember]
        public Int32? FunctionParametersCount { get; set; }

        public OnpTokenData Clone()
        {
            return (OnpTokenData)this.MemberwiseClone();
        }
    }
}
