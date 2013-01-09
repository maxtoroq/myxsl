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
using myxsl.net.common;

namespace myxsl.net.validation {

   [XPathModule("validation", "http://myxsl.net/ns/validation", SvrlPrefix, WellKnownNamespaces.SVRL)]
   public class ValidationModule {

      const string SvrlPrefix = "svrl";

      [XPathDependency]
      public IXsltProcessor Processor { get; set; }

      [XPathFunction("schematron-report", "document-node(element(" + SvrlPrefix + ":schematron-output))", "node()", "item()")]
      public XPathNavigator SchematronReport(XPathNavigator source, XPathItem schema) {
         return SchematronReport(source, schema, null);
      }

      [XPathFunction("schematron-report", "document-node(element(" + SvrlPrefix + ":schematron-output))", "node()", "item()", "xs:string?")]
      public XPathNavigator SchematronReport(XPathNavigator source, XPathItem schema, string phase) {
         return SchematronReport(source, schema, phase, null);
      }

      [XPathFunction("schematron-report", "document-node(element(" + SvrlPrefix + ":schematron-output))", "node()", "item()", "xs:string?", "node()*")]
      public XPathNavigator SchematronReport(XPathNavigator source, XPathItem schema, string phase, IEnumerable<XPathNavigator> parameters) {

         var options = new SchematronRuntimeOptions { 
            Instance = source,
            Phase = phase
         };

         if (parameters != null) {
            foreach (XPathNavigator n in parameters) 
               options.Parameters.Add(new XmlQualifiedName(n.Name, n.NamespaceURI), n.TypedValue);
         }

         SchematronInvoker invoker;
         
         if (schema.IsNode) { 

            if (this.Processor == null)
               throw new InvalidOperationException("Processor cannot be null");

            invoker = SchematronInvoker.With((XPathNavigator)schema, this.Processor);

         } else {
            invoker = SchematronInvoker.With(schema.Value);
         }

         return invoker
            .Validate(options)
            .ToDocument()
            .CreateNavigator();
      }
   }
}
