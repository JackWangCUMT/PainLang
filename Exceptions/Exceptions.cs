
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PainLang.Exceptions
{
#if !PCL
    [Serializable]
#endif
    public class PainMethodNotFoundException : Exception
    {
        public PainMethodNotFoundException() { }
        public PainMethodNotFoundException(string message) : base(message) { }
        public PainMethodNotFoundException(string message, Exception inner) : base(message, inner) { }
#if !PCL
        protected PainMethodNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }

#if !PCL
    [Serializable]
#endif
    public class PainUnsupportedMethodTypeException : Exception
    {
        public PainUnsupportedMethodTypeException() { }
        public PainUnsupportedMethodTypeException(string message) : base(message) { }
        public PainUnsupportedMethodTypeException(string message, Exception inner) : base(message, inner) { }
#if !PCL
        protected PainUnsupportedMethodTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }

#if !PCL
    [Serializable]
#endif
    public class PainInvalidOperationException : Exception
    {
        public PainInvalidOperationException() : this("Invalid operation type") { }
        public PainInvalidOperationException(string message) : base(message) { }
        public PainInvalidOperationException(string message, Exception inner) : base(message, inner) { }
#if !PCL
        protected PainInvalidOperationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }

#if !PCL
    [Serializable]
#endif
    public class PainInvalidExpressionException : Exception
    {
        public PainInvalidExpressionException() : this("Invalid expression") { }
        public PainInvalidExpressionException(string message) : base(message) { }
        public PainInvalidExpressionException(string message, Exception inner) : base(message, inner) { }
#if !PCL
        protected PainInvalidExpressionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }

#if !PCL
    [Serializable]
#endif
    public class PainIncorrectExpressionFormatException : Exception
    {
        public PainIncorrectExpressionFormatException() : this("Incorrect expression format") { }
        public PainIncorrectExpressionFormatException(string message) : base(message) { }
        public PainIncorrectExpressionFormatException(string message, Exception inner) : base(message, inner) { }
#if !PCL
        protected PainIncorrectExpressionFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#endif
    }
}
