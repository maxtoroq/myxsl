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
using System.Xml;
using System.Xml.XPath;
using myxsl.net.common;

namespace myxsl.net {

   [XPathModule("xslt", "http://myxsl.net/ns/xslt")]
   public class XsltModule {

      [XPathDependency]
      public IXsltProcessor Processor { get; set; }

      [XPathFunction("compile", "xs:integer", "item()")]
      public int Compile(XPathItem stylesheet) {
         return Compile(stylesheet, null);
      }

      [XPathFunction("compile", "xs:integer", "item()", "xs:string?")]
      public int Compile(XPathItem stylesheet, string processor) { 
         
         // TODO:
         throw new NotImplementedException();
      }

      [XPathFunction("apply-templates", "document-node()", "item()", "node()")]
      public XPathNavigator ApplyTemplates(XPathItem stylesheet, XPathNavigator input) {
         return ApplyTemplates(stylesheet, input, null);
      }

      [XPathFunction("apply-templates", "document-node()", "item()", "node()", "node()*")]
      public XPathNavigator ApplyTemplates(XPathItem stylesheet, XPathNavigator input, IEnumerable<XPathNavigator> parameters) {
         return ApplyTemplates(stylesheet, input, parameters, null);
      }

      [XPathFunction("apply-templates", "document-node()", "item()", "node()", "node()*", "xs:QName?")]
      public XPathNavigator ApplyTemplates(XPathItem stylesheet, XPathNavigator input, IEnumerable<XPathNavigator> parameters, XmlQualifiedName mode) {

         XsltRuntimeOptions options = GetRuntimeOptions(input, parameters, null, mode);

         return ExecuteStylesheet(stylesheet, options);
      }

      [XPathFunction("call-template", "document-node()", "item()", "xs:QName")]
      public XPathNavigator CallTemplate(XPathItem stylesheet, XmlQualifiedName initialTemplate) {
         return CallTemplate(stylesheet, initialTemplate, null);
      }

      [XPathFunction("call-template", "document-node()", "item()", "xs:QName", "node()?")]
      public XPathNavigator CallTemplate(XPathItem stylesheet, XmlQualifiedName initialTemplate, XPathNavigator input) {
         return CallTemplate(stylesheet, initialTemplate, input, null);
      }

      [XPathFunction("call-template", "document-node()", "item()", "xs:QName", "node()?", "node()*")]
      public XPathNavigator CallTemplate(XPathItem stylesheet, XmlQualifiedName initialTemplate, XPathNavigator input, IEnumerable<XPathNavigator> parameters) {

         XsltRuntimeOptions options = GetRuntimeOptions(input, parameters, initialTemplate, null);

         return ExecuteStylesheet(stylesheet, options);
      }

      XsltRuntimeOptions GetRuntimeOptions(XPathNavigator input, IEnumerable<XPathNavigator> parameters, XmlQualifiedName initialTemplate, XmlQualifiedName mode) {

         var options = new XsltRuntimeOptions();

         if (input != null)
            options.InitialContextNode = input;

         if (parameters != null) {
            foreach (XPathNavigator n in parameters)
               options.Parameters.Add(new XmlQualifiedName(n.Name, n.NamespaceURI), n.TypedValue);
         }

         if (initialTemplate != null)
            options.InitialTemplate = initialTemplate;

         if (mode != null) 
            options.InitialMode = mode;

         return options;
      }

      XPathNavigator ExecuteStylesheet(XPathItem stylesheet, XsltRuntimeOptions options) {

         XsltInvoker invoker;

         if (stylesheet.IsNode) {

            if (this.Processor == null)
               throw new InvalidOperationException("Processor cannot be null");

            invoker = XsltInvoker.With((XPathNavigator)stylesheet, this.Processor);

         } else {

            invoker = XsltInvoker.With(stylesheetUri: stylesheet.Value);
         }

         return invoker
            .Transform(options)
            .Result()
            .CreateNavigator();
      }
   }
}
