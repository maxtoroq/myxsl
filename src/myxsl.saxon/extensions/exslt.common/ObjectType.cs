// Copyright 2013 Max Toro Q.
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
using System.Text;
using Saxon.Api;

namespace myxsl.saxon.extensions.exslt.common {
   
   class ObjectType : ExtensionFunctionDefinition {

      readonly QName _FunctionName = new QName(Index.Namespace, "object-type");

      readonly XdmSequenceType[] _ArgumentTypes = new[] { 
         new XdmSequenceType(XdmAnyItemType.Instance, '*') 
      };

      readonly XdmSequenceType resultType = new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), ' ');

      public override XdmSequenceType[] ArgumentTypes { get { return _ArgumentTypes; } }
      public override QName FunctionName { get { return _FunctionName; } }
      public override int MaximumNumberOfArguments { get { return 1; } }
      public override int MinimumNumberOfArguments { get { return 1; } }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return this.resultType;
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall();
      }

      class FunctionCall : ExtensionFunctionCall {

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            XdmItem[] items = arguments[0].AsItems().ToArray();
            XdmItem item;

            if (items.Length != 1
               || (item = items[0]) is XdmNode) {

               return Result("node-set");
            }

            if (item.IsAtomic()) { 

               XdmAtomicValue atomicVal = (XdmAtomicValue)item;

               object typedVal = atomicVal.Value;

               if (typedVal is string) {
                  return Result("string");
               }

               switch (Type.GetTypeCode(typedVal.GetType())) {
                  case TypeCode.Boolean:
                     return Result("boolean");

                  case TypeCode.Byte:
                  case TypeCode.Decimal:
                  case TypeCode.Double:
                  case TypeCode.Int16:
                  case TypeCode.Int32:
                  case TypeCode.Int64:
                  case TypeCode.SByte:
                  case TypeCode.Single:
                  case TypeCode.UInt16:
                  case TypeCode.UInt32:
                  case TypeCode.UInt64:
                     return Result("number");
               }
            }

            return Result("external");
         }

         static IXdmEnumerator Result(string objectType) {
            return objectType.ToXdmAtomicValue().GetXdmEnumerator();
         }
      }
   }
}
