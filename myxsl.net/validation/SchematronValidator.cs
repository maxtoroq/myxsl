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
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.common;

namespace myxsl.net.validation {

   public abstract class SchematronValidator {

      [Browsable(false)]
      public XPathItemFactory ItemFactory { get { return this.XPathItemFactory; } }
      protected abstract XPathItemFactory XPathItemFactory { get; }

      public virtual void Validate(Stream output, SchematronRuntimeOptions options) {

         if (output == null) throw new ArgumentNullException("output");
         if (options == null) throw new ArgumentNullException("options");

         XmlWriter writer;
         XPathSerializationOptions serialization = options.Serialization;

         if (serialization != null) {

            XmlWriterSettings settings = new XmlWriterSettings();
            serialization.CopyTo(settings);

            writer = XmlWriter.Create(output, settings);
         } else {
            writer = XmlWriter.Create(output);
         }

         Validate(writer, options);

         writer.Close();
      }

      public virtual void Validate(TextWriter output, SchematronRuntimeOptions options) {

         if (output == null) throw new ArgumentNullException("output");
         if (options == null) throw new ArgumentNullException("options");

         XmlWriter writer;
         XPathSerializationOptions serialization = options.Serialization;

         if (serialization != null) {

            XmlWriterSettings settings = new XmlWriterSettings();
            serialization.CopyTo(settings);

            writer = XmlWriter.Create(output, settings);
         } else {
            writer = XmlWriter.Create(output);
         }

         Validate(writer, options);

         writer.Close();
      }

      public abstract void Validate(XmlWriter output, SchematronRuntimeOptions options);

      public abstract IXPathNavigable Validate(SchematronRuntimeOptions options);
   }
}
