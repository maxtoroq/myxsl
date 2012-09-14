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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace myxsl.net.common {
   
   [DebuggerDisplay("{Namespace}")]
   public sealed class XPathModuleInfo {

      XPathModuleAttribute moduleAttr;
      string _Namespace;
      Dictionary<string, string> _NamespaceBindings;
      ReadOnlyCollection<XPathFunctionInfo> _Functions;
      ReadOnlyCollection<XPathDependencyInfo> _Dependencies;
      string _PredeclarePrefix;

      public Type Type { get; private set; }

      public bool TypeIsStatic { 
         get { return Type.IsAbstract && Type.IsSealed; } 
      }

      public string Namespace { 
         get {
            if (_Namespace == null) {
               _Namespace = (moduleAttr != null && moduleAttr.Namespace.HasValue()) ? 
                  moduleAttr.Namespace 
                  : "clitype:" + Type.FullName;
            }
            return _Namespace;
         } 
      }

      public Dictionary<string, string> NamespaceBindings {
         get {
            if (_NamespaceBindings == null) {
               _NamespaceBindings = new Dictionary<string, string> { 
                  { "xs", WellKnownNamespaces.XMLSchema }
               };

               if (moduleAttr != null) {

                  if (moduleAttr.Prefix.HasValue()) 
                     _NamespaceBindings.Add(moduleAttr.Prefix, Namespace);

                  string[] nsBindings = moduleAttr.GetNamespaceBindings();

                  if (nsBindings != null
                     && nsBindings.Length > 0) {

                     for (int i = 0; i < nsBindings.Length; i += 2) {
                        
                        string prefix = nsBindings[i];
                        string ns = nsBindings[i + 1];

                        if (!prefix.HasValue() || !ns.HasValue()) {
                           throw new InvalidOperationException(
                              "{0}.NamespaceBindings cannot have null or empty parameters ({1})."
                                 .FormatInvariant(typeof(XPathModuleAttribute).Name, Type.FullName)
                           );
                        }

                        if (_NamespaceBindings.ContainsKey(prefix)) {
                           throw new InvalidOperationException(
                              "The {0} prefix is already bound to the {1} namespace ({2})."
                                 .FormatInvariant(prefix, _NamespaceBindings[prefix], Type.FullName)
                           );
                        }

                        const string xmlNs = "http://www.w3.org/XML/1998/namespace";

                        if ((prefix == "xml" && ns != xmlNs)) {
                           
                           throw new InvalidOperationException(
                              "The {0} prefix must be bound to {1}.".FormatInvariant(prefix, xmlNs)
                           );
                        }

                        _NamespaceBindings.Add(prefix, ns);
                     }
                  }
               }
            }
            return _NamespaceBindings;
         }
      }

      public ReadOnlyCollection<XPathFunctionInfo> Functions {
         get {
            if (_Functions == null) {
               BindingFlags bindingFlags = BindingFlags.Public | ((TypeIsStatic) ? BindingFlags.Static : BindingFlags.Instance);

               IEnumerable<MethodInfo> methods = Type.GetMethods(bindingFlags);

               if (HasModuleAttribute)
                  methods = methods.Where(m => Attribute.IsDefined(m, typeof(XPathFunctionAttribute)));
               else
                  methods = methods.Where(m => !m.ContainsGenericParameters && m.GetParameters().All(p => !p.IsOut));

               _Functions = new ReadOnlyCollection<XPathFunctionInfo>(
                  methods.Select(m => new XPathFunctionInfo(m, this)).ToArray()
               );

               var firstBadOverload =
                  (from f in _Functions
                   group f by f.Name into grp
                   let distinctOverloadsByParamLength = grp.Select(f => f.Parameters.Count).Distinct().Count()
                   let hasOverloadsWithSameParamLength = distinctOverloadsByParamLength != grp.Count()
                   let distinctOverloadsByReturnType = grp.Select(f => f.ReturnType.ClrType).Distinct().Count()
                   let hasOverloadsWithDifferentReturnType = distinctOverloadsByReturnType > 1
                   where hasOverloadsWithSameParamLength
                     || hasOverloadsWithDifferentReturnType
                   select new { 
                     FunctionName = grp.Key,
                     hasOverloadsWithSameParamLength,
                     hasOverloadsWithDifferentReturnType
                   }).FirstOrDefault();

               if (firstBadOverload != null) {
                  
                  if (firstBadOverload.hasOverloadsWithSameParamLength) {
                     
                     throw new InvalidOperationException(
                        "Overloaded functions with same number of parameters are not allowed ({0} on {1})."
                           .FormatInvariant(firstBadOverload.FunctionName, Type.FullName)
                     );
                  
                  } /*else if (firstBadOverload.hasOverloadsWithDifferentReturnType) {

                     throw new InvalidOperationException(
                        "Overloaded functions must have the same return type ({0} on {1})."
                           .FormatInvariant(firstBadOverload.FunctionName, Type.FullName)
                     );
                  }*/
               }
            }
            return _Functions;
         }
      }

      public ReadOnlyCollection<XPathDependencyInfo> Dependencies {
         get {
            if (_Dependencies == null) {

               if (TypeIsStatic) {
                  _Dependencies = new ReadOnlyCollection<XPathDependencyInfo>(new XPathDependencyInfo[0]);
               } else {

                  BindingFlags bindingFlags = BindingFlags.Public | ((TypeIsStatic) ? BindingFlags.Static : BindingFlags.Instance);

                  IEnumerable<PropertyInfo> properties = Type.GetProperties(bindingFlags)
                     .Where(p => Attribute.IsDefined(p, typeof(XPathDependencyAttribute)));

                  _Dependencies = new ReadOnlyCollection<XPathDependencyInfo>(
                     properties.Select(p => new XPathDependencyInfo(p)).ToArray()
                  );
               }
            }
            return _Dependencies;
         }
      }

      public bool Predeclare { get; internal set; }

      public string PredeclarePrefix {
         get {
            if (!Predeclare)
               return null;

            if (_PredeclarePrefix == null) 
               _PredeclarePrefix = NamespaceBindings.First(p => p.Value == Namespace).Key;

            return _PredeclarePrefix;
         }
      }

      internal bool HasModuleAttribute {
         get {
            return moduleAttr != null;
         }
      }

      internal XPathModuleInfo(Type type) {
         
         this.Type = type;
         this.moduleAttr = Attribute.GetCustomAttribute(this.Type, typeof(XPathModuleAttribute)) 
            as XPathModuleAttribute;
      }
   }
}
