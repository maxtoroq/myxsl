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

namespace myxsl.net {
   
   [AttributeUsage(AttributeTargets.Class)]
   public sealed class XPathModuleAttribute : Attribute {

      readonly string[] namespaceBindings;

      public string Prefix { get; private set; }
      public string Namespace { get; private set; }

      public XPathModuleAttribute() { }

      public XPathModuleAttribute(string @namespace) {
         this.Namespace = @namespace;
      }

      public XPathModuleAttribute(string prefix, string @namespace) 
         : this(@namespace) {
         
         this.Prefix = prefix;
      }

      public XPathModuleAttribute(string prefix, string @namespace, string prefix1, string ns1) 
         : this(prefix, @namespace, new string[] { prefix1, ns1 }) {  }

      public XPathModuleAttribute(string prefix, string @namespace, string prefix1, string ns1, string prefix2, string ns2)
         : this(prefix, @namespace, new string[] { prefix1, ns1, prefix2, ns2 }) { }

      public XPathModuleAttribute(string prefix, string @namespace, string prefix1, string ns1, string prefix2, string ns2, string prefix3, string ns3)
         : this(prefix, @namespace, new string[] { prefix1, ns1, prefix2, ns2, prefix3, ns3 }) { }

      public XPathModuleAttribute(string prefix, string @namespace, string prefix1, string ns1, string prefix2, string ns2, string prefix3, string ns3, string prefix4, string ns4)
         : this(prefix, @namespace, new string[] { prefix1, ns1, prefix2, ns2, prefix3, ns3, prefix4, ns4 }) { }

      public XPathModuleAttribute(string prefix, string @namespace, string prefix1, string ns1, string prefix2, string ns2, string prefix3, string ns3, string prefix4, string ns4, string prefix5, string ns5)
         : this(prefix, @namespace, new string[] { prefix1, ns1, prefix2, ns2, prefix3, ns3, prefix4, ns4, prefix5, ns5 }) { }

      public XPathModuleAttribute(string prefix, string @namespace, string prefix1, string ns1, string prefix2, string ns2, string prefix3, string ns3, string prefix4, string ns4, string prefix5, string ns5, string prefix6, string ns6)
         : this(prefix, @namespace, new string[] { prefix1, ns1, prefix2, ns2, prefix3, ns3, prefix4, ns4, prefix5, ns5, prefix6, ns6 }) { }

      public XPathModuleAttribute(string prefix, string @namespace, params string[] namespaceBindings) 
         : this(prefix, @namespace) {

         if (namespaceBindings != null
            && namespaceBindings.Length % 2 != 0) {
            throw new ArgumentException(
               "The length of namespaceBindings must be even.",
               "namespaceBindings"
            );
         }

         this.namespaceBindings = namespaceBindings;
      }

      internal string[] GetNamespaceBindings() {
         return this.namespaceBindings;
      }
   }
}
