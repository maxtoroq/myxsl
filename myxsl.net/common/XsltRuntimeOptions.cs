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
using System.Xml;
using System.Xml.XPath;

namespace myxsl.net.common {

   public class XsltRuntimeOptions {

      readonly IDictionary<XmlQualifiedName, object> _Parameters = new Dictionary<XmlQualifiedName, object>();
      XmlResolver _InputXmlResolver;

      XPathSerializationOptions _Serialization;

      public IXPathNavigable InitialContextNode { get; set; }
      public XmlQualifiedName InitialTemplate { get; set; }
      public XmlQualifiedName InitialMode { get; set; }
      public IDictionary<XmlQualifiedName, object> Parameters { get { return _Parameters; } }

      public Uri BaseOutputUri { get; set; }

      public XmlResolver InputXmlResolver {
         get {
            if (_InputXmlResolver == null) 
               InputXmlResolver = new XmlDynamicResolver();
            return _InputXmlResolver;
         }
         set {
            _InputXmlResolver = value;
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

      // TODO:
      public XsltRuntimeOptions() { }
   }
}
