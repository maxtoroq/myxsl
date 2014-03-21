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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace myxsl.net.common {
   
   public sealed class XPathFunctionInfo {

      readonly MethodInfo _Method;
      readonly XPathModuleInfo _Module;
      readonly XPathFunctionAttribute functionAttr;

      string _Name;
      bool? _HasSideEffects;
      XPathSequenceType _ReturnType;
      ReadOnlyCollection<XPathVariableInfo> _Parameters;

      public MethodInfo Method { get { return _Method; } }
      public XPathModuleInfo Module { get { return _Module; } }

      public string Name {
         get {
            if (_Name == null) {
               _Name = XmlConvert.VerifyNCName(
                  (functionAttr != null && functionAttr.Name.HasValue()) ?
                     functionAttr.Name
                     : Method.Name
               );
            };
            return _Name;
         }
      }

      public bool HasSideEffects {
         get {
            if (_HasSideEffects == null) {
               _HasSideEffects = (functionAttr != null) ?
                  functionAttr.HasSideEffects
                  : Method.ReturnType == typeof(void);
            }
            return _HasSideEffects.Value;
         }
      }

      public XPathSequenceType ReturnType {
         get {
            if (_ReturnType == null) {
               _ReturnType = new XPathSequenceType(
                  Method.ReturnType,
                  (functionAttr != null && functionAttr.ReturnSequenceType.HasValue()) ? 
                     functionAttr.ReturnSequenceType : null,
                  Module.NamespaceBindings
               );
            }
            return _ReturnType;
         }
      }

      public ReadOnlyCollection<XPathVariableInfo> Parameters {
         get {
            if (_Parameters == null) {

               var list = new List<XPathVariableInfo>();

               ParameterInfo[] parameters = Method.GetParameters();
               
               IList<string> sequenceTypes = functionAttr != null
                  && functionAttr.ParameterSequenceTypes.Count == parameters.Length ?
                  functionAttr.ParameterSequenceTypes
                  : null;

               for (int i = 0; i < parameters.Length; i++) {
                  
                  ParameterInfo param = parameters[i];

                  list.Add(new XPathVariableInfo(
                     param.Name, 
                     new XPathSequenceType(
                        param.ParameterType,
                        (sequenceTypes != null) ?
                           sequenceTypes[i]
                           : null,
                        Module.NamespaceBindings
                     )
                  ));
               }

               _Parameters = new ReadOnlyCollection<XPathVariableInfo>(list);
            }
            return _Parameters;
         }
      }

      internal XPathFunctionInfo(MethodInfo method, XPathModuleInfo module) {

         if (method == null) throw new ArgumentNullException("method");
         if (method.ContainsGenericParameters) throw new ArgumentException("Methods that accept type parameters cannot be used as module functions.", "method");
         if (method.GetParameters().Any(p => p.IsOut)) throw new ArgumentException("Methods with out parmeters cannot be used as module functions.", "method");
         if (module == null) throw new ArgumentNullException("module");

         this._Method = method;
         this._Module = module;

         if (this.Module.HasModuleAttribute) {
            this.functionAttr = Attribute.GetCustomAttribute(this.Method, typeof(XPathFunctionAttribute))
               as XPathFunctionAttribute;
         }
      }
   }
}
