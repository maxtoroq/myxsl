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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using myxsl.net.common;

namespace myxsl.net {

   [XPathModule("xslt", Namespace)]
   public class XsltModule {

      const string Namespace = "http://myxsl.net/ns/xslt";

      [XPathDependency]
      public XPathItemFactory ItemFactory { get; set; }

      [XPathDependency]
      public XmlResolver Resolver { get; set; }

      [XPathDependency]
      public IXsltProcessor CurrentXsltProcessor { get; set; }

      [XPathFunction("compile", "item()", "item()")]
      public XPathItem Compile(XPathItem stylesheet) {
         return Compile(stylesheet, null);
      }

      [XPathFunction("compile", "item()", "item()", "xs:string?")]
      public XPathItem Compile(XPathItem stylesheet, string processor) {

         CompiledStylesheetReference reference;

         if (stylesheet.IsNode) {

            IXsltProcessor proc = (processor != null) ?
               Processors.Xslt[processor]
               : this.CurrentXsltProcessor ?? Processors.Xslt.DefaultProcessor;
            
            int hashCode;

            XsltInvoker.With((XPathNavigator)stylesheet, proc, null, out hashCode);
            
            if (processor == null)
               return this.ItemFactory.CreateAtomicValue(hashCode, XmlTypeCode.Integer);

            reference = new CompiledStylesheetReference {
               HashCode = hashCode,
               Processor = processor
            };

         } else {

            Uri stylesheetUri = StylesheetAsUri(stylesheet);

            if (processor == null
               || processor == Processors.Xslt.DefaultProcessorName) {

               return this.ItemFactory.CreateAtomicValue(stylesheetUri.ToString(), XmlTypeCode.String);
            }

            XsltInvoker.With(stylesheetUri, processor);

            reference = new CompiledStylesheetReference { 
               Uri = stylesheetUri.ToString(),
               Processor = processor
            };
         }

         return this.ItemFactory
            .CreateDocument(reference)
            .CreateNavigator();
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

         IXsltProcessor currentOrDefaultProc = this.CurrentXsltProcessor ?? Processors.Xslt.DefaultProcessor;

         if (stylesheet.IsNode) {

            XPathNavigator node = ((XPathNavigator)stylesheet).Clone();

            if (node.NodeType != XPathNodeType.Element) 
               node.MoveToChild(XPathNodeType.Element);

            if (node.NodeType != XPathNodeType.Element)
               throw new ArgumentException("stylesheet must be either a document or element node.", "stylesheet");

            if (node.NamespaceURI == Namespace) {

               XmlSerializer serializer = XPathItemFactory.GetSerializer(typeof(CompiledStylesheetReference));

               var reference = (CompiledStylesheetReference)serializer.Deserialize(node.ReadSubtree());

               IXsltProcessor specifiedProcessor = (reference.Processor != null) ?
                  Processors.Xslt[reference.Processor]
                  : null;

               invoker = (reference.HashCode > 0) ? 
                  XsltInvoker.With(reference.HashCode, specifiedProcessor ?? currentOrDefaultProc)
                  : XsltInvoker.With(reference.Uri, specifiedProcessor);

            } else {
               invoker = XsltInvoker.With((XPathNavigator)stylesheet, currentOrDefaultProc);
            }

         } else {

            object value = stylesheet.TypedValue;

            if (value.GetType().IsPrimitive) {

               int hashCode = Convert.ToInt32(value, CultureInfo.InvariantCulture);

               invoker = XsltInvoker.With(hashCode, currentOrDefaultProc);
            
            } else {

               Uri stylesheetUri = StylesheetAsUri(stylesheet);
               
               invoker = XsltInvoker.With(stylesheetUri);
            }
         }

         return invoker.Transform(options)
            .Result()
            .CreateNavigator();
      }

      Uri StylesheetAsUri(XPathItem stylesheet) {

         Uri stylesheetUri = stylesheet.TypedValue as Uri;

         if (stylesheetUri == null){

            if (this.Resolver == null)
               throw new InvalidOperationException("Resolver cannot be null.");

            stylesheetUri = this.Resolver.ResolveUri(null, stylesheet.Value);
         }

         return stylesheetUri;
      }

      [XmlRoot(Namespace = Namespace)]
      public class CompiledStylesheetReference {

         [XmlAttribute]
         public string Processor;

         [XmlAttribute]
         public string Uri;

         [XmlAttribute]
         public int HashCode;
      }
   }
}
