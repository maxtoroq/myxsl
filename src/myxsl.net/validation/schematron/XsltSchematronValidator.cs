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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using myxsl.common;

namespace myxsl.schematron {

   class XsltSchematronValidator : SchematronValidator {

      readonly XsltExecutable executable;

      protected override XPathItemFactory XPathItemFactory {
         get { return executable.Processor.ItemFactory; }
      }

      protected internal XsltSchematronValidator(XsltExecutable executable) {

         if (executable == null) throw new ArgumentNullException("executable");

         this.executable = executable;
      }

      public override void Validate(XmlWriter output, SchematronRuntimeOptions options) {

         if (output == null) throw new ArgumentNullException("output");
         if (options == null) throw new ArgumentNullException("options");

         this.executable.Run(output, GetXsltOptions(options));
      }

      public override IXPathNavigable Validate(SchematronRuntimeOptions options) {

         if (options == null) throw new ArgumentNullException("options");

         return this.executable.Run(GetXsltOptions(options));
      }

      static XsltRuntimeOptions GetXsltOptions(SchematronRuntimeOptions options) {

         var xsltOptions = new XsltRuntimeOptions {
            InitialContextNode = options.Instance
         };

         if (!String.IsNullOrEmpty(options.Phase)) {
            xsltOptions.Parameters.Add(new XmlQualifiedName("phase"), options.Phase);
         }

         if (options.Parameters != null) {
            foreach (var p in options.Parameters) {
               xsltOptions.Parameters.Add(p);
            }
         }

         return xsltOptions;
      }
   }
}
