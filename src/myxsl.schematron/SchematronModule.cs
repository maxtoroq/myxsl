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
using System.Xml;
using System.Xml.XPath;
using myxsl.common;

namespace myxsl.schematron {

   [XPathModule("schematron", XPathModuleAttribute.BuiltInModulesBaseNamespace + "schematron", "svrl", SchematronInvoker.SvrlNamespace)]
   public class SchematronModule {

      [XPathDependency]
      public IXsltProcessor Processor { get; set; }

      [XPathDependency]
      public XmlResolver Resolver { get; set; }

      [XPathFunction("report", "item()", "node()", As = "document-node(element(svrl:schematron-output))")]
      public XPathNavigator Report(XPathItem schema, XPathNavigator source) {
         return Report(schema, source, null);
      }

      [XPathFunction("report", "item()", "node()", "xs:string?", As = "document-node(element(svrl:schematron-output))")]
      public XPathNavigator Report(XPathItem schema, XPathNavigator source, string phase) {
         return Report(schema, source, phase, null);
      }

      [XPathFunction("report", "item()", "node()", "xs:string?", "node()*", As = "document-node(element(svrl:schematron-output))")]
      public XPathNavigator Report(XPathItem schema, XPathNavigator source, string phase, IEnumerable<XPathNavigator> parameters) {

         var options = new SchematronRuntimeOptions { 
            Instance = source,
            Phase = phase
         };

         if (parameters != null) {

            foreach (XPathNavigator n in parameters) {
               options.Parameters.Add(new XmlQualifiedName(n.Name, n.NamespaceURI), n.TypedValue);
            }
         }

         SchematronInvoker invoker;
         
         if (schema.IsNode) {

            if (this.Processor == null) {
               throw new InvalidOperationException("Processor cannot be null");
            }

            invoker = SchematronInvoker.With((XPathNavigator)schema, this.Processor);

         } else {

            Uri schemaUri = SchemaAsUri(schema);

            invoker = SchematronInvoker.With(schemaUri: schema.Value);
         }

         return invoker
            .Validate(options)
            .ToDocument()
            .CreateNavigator();
      }

      Uri SchemaAsUri(XPathItem schema) {

         Uri schemaUri = schema.TypedValue as Uri;

         if (schemaUri == null) {

            if (this.Resolver == null) {
               throw new InvalidOperationException("Resolver cannot be null.");
            }

            schemaUri = this.Resolver.ResolveUri(null, schema.Value);
         }

         return schemaUri;
      }
   }
}
