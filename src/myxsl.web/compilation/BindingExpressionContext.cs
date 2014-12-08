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
using System.Text;
using System.Xml;
using System.Xml.XPath;
using myxsl.web.ui;

namespace myxsl.web.compilation {
   
   public class BindingExpressionContext {

      readonly IDictionary<string, string> _InScopeNamespaces;

      public BaseParser Parser { get; private set; }
      public XPathNavigator BoundNode { get; private set; }

      public IDictionary<string, string> InScopeNamespaces {
         get { return _InScopeNamespaces; } 
      }

      public string NodeName { get; set; }

      internal bool AffectsXsltInitiation { get; set; }

      public BindingExpressionContext(BaseParser parser, XPathNavigator boundNode) 
         : this(parser, boundNode, null) { }

      public BindingExpressionContext(BaseParser parser, XPathNavigator boundNode, IDictionary<string, string> inScopeNamespaces) {
         
         this.Parser = parser;
         this.BoundNode = boundNode;

         if (inScopeNamespaces == null) {
            inScopeNamespaces = boundNode.GetNamespacesInScope(XmlNamespaceScope.All);
         }

         this._InScopeNamespaces = inScopeNamespaces;
      }
   }
}
