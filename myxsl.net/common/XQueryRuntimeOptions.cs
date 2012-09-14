// Copyright 2009 Max Toro Q.
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
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace myxsl.net.common {

   public class XQueryRuntimeOptions {

      readonly IDictionary<XmlQualifiedName, object> _ExternalVariables = new Dictionary<XmlQualifiedName, object>();
      Assembly callingAssembly = Assembly.GetCallingAssembly();
      XmlResolver _InputXmlResolver;

      XPathSerializationOptions _Serialization;

      public object ContextItem { get; set; }
      public IDictionary<XmlQualifiedName, object> ExternalVariables { get { return _ExternalVariables; } }

      public XmlResolver InputXmlResolver {
         get {
            if (_InputXmlResolver == null
               && callingAssembly != null) {
               InputXmlResolver = new XmlDynamicResolver(callingAssembly);
            }
            return _InputXmlResolver;
         }
         set {
            _InputXmlResolver = value;
            callingAssembly = null;
         }
      }

      public XPathSerializationOptions Serialization {
         get {
            if (_Serialization == null)
               _Serialization = new XPathSerializationOptions();
            return _Serialization;
         }
         set { _Serialization = value; }
      }
   }
}
