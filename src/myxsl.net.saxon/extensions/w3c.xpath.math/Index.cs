// Copyright 2012 Max Toro Q.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Saxon.Api;

namespace myxsl.net.saxon.extensions.w3c.xpath.math {
   
   static class Index {
      
      public const string Namespace = "http://www.w3.org/2005/xpath-functions/math";

      public static IEnumerable<ExtensionFunctionDefinition> GetFunctions() {
         yield return new Acos();
         yield return new Asin();
         yield return new Atan();
         yield return new Cos();
         yield return new Exp();
         yield return new Exp10();
         yield return new Log();
         yield return new Log10();
         yield return new PI();
         yield return new Pow();
         yield return new Sin();
         yield return new Sqrt();
         yield return new Tan();
      }
   }

   sealed class Acos : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "acos");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {
            
            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Acos((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Asin : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "asin");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Asin((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Atan : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "atan");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Atan((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Cos : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "cos");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Cos((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Exp : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "exp");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Exp((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Exp10 : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "exp10");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Pow(10, (double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Log : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "log");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Log((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Log10 : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "log10");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Log10((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class PI : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new XdmSequenceType[0];

      readonly QName _FunctionName = new QName(Index.Namespace, "pi");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 0; }
      }

      public override int MinimumNumberOfArguments {
         get { return 0; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), ' ');
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {
            return Math.PI.ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Pow : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?'),
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), ' ')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "pow");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 2; }
      }

      public override int MinimumNumberOfArguments {
         get { return 2; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();
            XdmAtomicValue p2 = arguments[1].AsAtomicValues().Single();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Pow((double)p1.Value, (double)p2.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Sin : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "sin");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Sin((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Sqrt : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "sqrt");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Sqrt((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }

   sealed class Tan : ExtensionFunctionDefinition {

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_DOUBLE), '?')
      };

      readonly QName _FunctionName = new QName(Index.Namespace, "tan");

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return ArgumentTypes[0];
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmAtomicValue p1 = arguments[0].AsAtomicValues().SingleOrDefault();

            if (p1 == null)
               return EmptyEnumerator.INSTANCE;

            return Math.Tan((double)p1.Value).ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }
}
