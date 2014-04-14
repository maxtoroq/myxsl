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
using System.Xml.XPath;
using myxsl.web.ui;

namespace myxsl.web.compilation {
   
   public class BindingExpressionContext {

      public BaseParser Parser { get; private set; }
      public XPathNavigator BoundNode { get; private set; }
      public string NodeName { get; set; }
      public string Namespace { get; set; }

      internal bool AffectsXsltInitiation { get; set; }

      public BindingExpressionContext(BaseParser parser, XPathNavigator boundNode) {
         
         this.Parser = parser;
         this.BoundNode = boundNode;
      }
   }
}
