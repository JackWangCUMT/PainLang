# PainLang (v0.8)
Welcome to the home page of PainLang, a dynamic scripting language written in .NET. 

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
 + written entirely in plain C# (without DLR or Reflection.Emit)

## Performance
 22808 lines/second on single core (Intel Core i5-3210M@2.50Ghz)

##Examples

 + Basic usage:
```
new PainCompiler().Compile(" return 1+3 ").Eval();
# result is 4
```
 + Usage of global functions:
```
new PainCompiler().Compile(" return substring( round(33.3333, 2) + 'ABC', 0, 6) ").Eval();
# result is 33.33A
```
 + Conditional statements:
```
new PainCompiler().Compile(@"

if 100 < 10:
  return 'Not true..'
elif 100 < 90:
  return 'Also not true..'
elif 100 < 101:
  return 'This is true..'
else:
  return '..' 
  
").Eval();
# result is 'This is true..'
```
 + While statement:
```
new PainCompiler().Compile(@"

i = 0
while i < 100:
  i = i + 1
return i

").Eval();
# result is 100
```
 + Function definition:
```
new PainCompiler().Compile(@"

def increment(a): 
  b = a + 1 
  return b 
return increment(10)

").Eval();
# result is 11
```
 + Try / catch:
```
new PainCompiler().Compile(@"

error = null
try:
  throw 'Error!'
catch (ex):
  error = ex
return error.Message

").Eval();
# result is 'Error!'
```
 + Classes:
```
new PainCompiler().Compile(@"

class TestClass():
  a = 1
  b = 2
  def Sum(c):
    return this.a + this.b + c
obj = TestClass()
return obj.Sum(3)+obj.a

").Eval();
# result is 7
```
 + .NET interoperability example (using c# variable from PainLang):
```
Dictionary<String,Object> variables = new Dictionary<String,Object>();
variables["A"] = "Test string";

new PainCompiler().Compile(@"

return A.Length + ' ' + A.Substring(5,6) 

").Eval(variables);
# result is '11 string'
```
 + .NET interoperability example (using PainLang method in C#):
```
PainContext context = new PainCompiler().Compile(@"

def method1(a,b,c):
  if a:
    return b
  else:
    return c

").CreateContext();
Object result = context.InvokeMethod("method1", new object [] { 0, "Result1", "Result2" });
# result is 'Result2'
```
