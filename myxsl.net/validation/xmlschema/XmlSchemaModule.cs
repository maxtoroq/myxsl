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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using myxsl.net.common;

namespace myxsl.net.validation.xmlschema {
   
   [XPathModule("xml-schema", "http://myxsl.net/ns/validation/xml-schema")]
   public class XmlSchemaModule {

      [XPathDependency]
      public XPathItemFactory ItemFactory { get; set; }

      [XPathDependency]
      public XmlResolver Resolver { get; set; }

      [XPathFunction("validate", "node()", "item()")]
      public XPathNavigator Validate(XPathItem instance) {
         return Validate(instance, null);
      }

      [XPathFunction("validate", "node()", "item()", "item()?")]
      public XPathNavigator Validate(XPathItem instance, XPathItem schema) {

         var readerSettings = new XmlReaderSettings { 
            XmlResolver = this.Resolver,
            // XmlReader: Cannot change conformance checking to Document. Make sure the ConformanceLevel in XmlReaderSettings is set to Auto for wrapping scenarios.
            ConformanceLevel = ConformanceLevel.Auto
         };

         if (schema != null) {

            var schemas = new XmlSchemaSet();

            Uri schemaUri;

            XmlReader schemaReader = (schema.IsNode) ?
               XmlReader.Create(((XPathNavigator)schema).ReadSubtree(), readerSettings)
               : XmlReader.Create((schemaUri = ItemAsUri(schema)).AbsoluteUri, readerSettings);

            using (schemaReader)
               schemas.Add(null, schemaReader);

            readerSettings.Schemas = schemas;
         }

         readerSettings.ValidationType = ValidationType.Schema;
         readerSettings.ValidationFlags = XmlSchemaValidationFlags.AllowXmlAttributes
            | XmlSchemaValidationFlags.ProcessIdentityConstraints
            | XmlSchemaValidationFlags.ProcessInlineSchema
            | XmlSchemaValidationFlags.ProcessSchemaLocation;

         Uri instanceUri;

         XmlReader instanceReader = (instance.IsNode) ?
            XmlReader.Create(((XPathNavigator)instance).ReadSubtree(), readerSettings)
            : XmlReader.Create((instanceUri = ItemAsUri(instance)).AbsoluteUri, readerSettings);

         XPathNavigator validatedInstance;

         using (instanceReader) {
            // XPathDocument does not retain schema info
            validatedInstance = this.ItemFactory.CreateNodeEditable(instanceReader).CreateNavigator();
         }

         return validatedInstance;
      }

      [XPathFunction("is-validated", "xs:boolean", "node()")]
      public bool IsValidated(XPathNavigator instance) {
         return instance.SchemaInfo != null;
      }

      Uri ItemAsUri(XPathItem item) {

         Uri itemUri = item.TypedValue as Uri;

         if (itemUri == null) {

            if (this.Resolver == null)
               throw new InvalidOperationException("Resolver cannot be null.");

            itemUri = this.Resolver.ResolveUri(null, item.Value);
         }

         return itemUri;
      }
   }
}
