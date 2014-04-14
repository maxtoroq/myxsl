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
using System.Text;
using System.Xml;

namespace myxsl.common {
   
   public class XPathSerializationMethods {

      static readonly XmlQualifiedName _Xml = new XmlQualifiedName("xml");
      static readonly XmlQualifiedName _Html = new XmlQualifiedName("html");
      static readonly XmlQualifiedName _XHtml = new XmlQualifiedName("xhtml");
      static readonly XmlQualifiedName _Text = new XmlQualifiedName("text");

      public static XmlQualifiedName Xml { get { return _Xml; } }
      public static XmlQualifiedName Html { get { return _Html; } }
      public static XmlQualifiedName XHtml { get { return _XHtml; } }
      public static XmlQualifiedName Text { get { return _Text; } }
   }
}
