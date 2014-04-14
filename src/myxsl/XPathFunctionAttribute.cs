// Copyright 2010 Max Toro Q.
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
using System.Collections.ObjectModel;

namespace myxsl {

   [AttributeUsage(AttributeTargets.Method)]
   public sealed class XPathFunctionAttribute : Attribute {

      public string Name { get; private set; }
      public string ReturnSequenceType { get; private set; }
      public ReadOnlyCollection<string> ParameterSequenceTypes { get; private set; }
      public bool HasSideEffects { get; set; }

      public XPathFunctionAttribute() 
         : this(null) { }

      public XPathFunctionAttribute(string name) 
         : this(name, null) { }

      public XPathFunctionAttribute(string name, string returnSequenceType) 
         : this(name, returnSequenceType, (string[])null) { }

      public XPathFunctionAttribute(string name, string returnSequenceType, string param1) 
         : this(name, returnSequenceType, new string[] { param1 }) { }

      public XPathFunctionAttribute(string name, string returnSequenceType, string param1, string param2)
         : this(name, returnSequenceType, new string[] { param1, param2 }) { }

      public XPathFunctionAttribute(string name, string returnSequenceType, string param1, string param2, string param3)
         : this(name, returnSequenceType, new string[] { param1, param2, param3 }) { }

      public XPathFunctionAttribute(string name, string returnSequenceType, string param1, string param2, string param3, string param4)
         : this(name, returnSequenceType, new string[] { param1, param2, param3, param4 }) { }

      public XPathFunctionAttribute(string name, string returnSequenceType, string param1, string param2, string param3, string param4, string param5)
         : this(name, returnSequenceType, new string[] { param1, param2, param3, param4, param5 }) { }

      public XPathFunctionAttribute(string name, string returnSequenceType, string param1, string param2, string param3, string param4, string param5, string param6)
         : this(name, returnSequenceType, new string[] { param1, param2, param3, param4, param5, param6 }) { }

      public XPathFunctionAttribute(string name, string returnSequenceType, params string[] parameterSequenceTypes) {
         
         this.Name = name;
         this.ReturnSequenceType = returnSequenceType;
         this.ParameterSequenceTypes = new ReadOnlyCollection<string>(parameterSequenceTypes ?? new string[0]);
      }
   }
}
