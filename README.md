# PainLang (v0.8)
Welcome to the home page of PainLang, a dynamic scripting language written in .NET

## Main language features
 + .NET interoperability
   + possibility to use .NET objects in script context
   + objects created in script are available in .NET context
 + .NET version independent
   + .NET 2.0 or higher
   + Silverlight 2.0 or higher
   + PCL library
   + .NET CF / .NET core
 + serialization/deserialization of program state
   + possibility to save the state of the program and continue execution later or on another computer

## Performance
 22808 lines/second on laptop with Intel Core i5-3210M@2.50Ghz processor 

##Examples

 + Basic usage:
`new PainCompiler().Compile(" return 1+3 ").Eval();` ```
`# result is 4`
 + Usage of global functions:
`new PainCompiler().Compile(" substring(round(33.3333,2)+'ABC',0,6) ").Eval();`
`# result is 33.33A`
 + Function definition:
`new PainCompiler().Compile(`
`"def increment(a): \n" +`
`"  b = a + 1 \n" +`
`"  return b \n" +`
`"return increment(10)").Eval();`
`# result is 11`
 


