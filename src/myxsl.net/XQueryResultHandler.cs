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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.common;

namespace myxsl.net {

   public class XQueryResultHandler {

      readonly XQueryExecutable executable;
      readonly XQueryRuntimeOptions options;
      readonly XPathSerializationOptions defaultSerialization;

      internal XQueryResultHandler(XQueryExecutable executable, XQueryRuntimeOptions options) {

         this.executable = executable;
         this.options = options;
         this.defaultSerialization = options.Serialization;
      }

      public IEnumerable<XPathItem> Result() {
         return this.executable.Run(this.options);
      }

      public void To(Stream output) {
         To(output, null);
      }

      public void To(Stream output, XPathSerializationOptions options) {

         OverrideSerialization(options);

         this.executable.Run(output, this.options);

         RestoreSerialization(options);
      }

      public void To(TextWriter output) {
         To(output, null);
      }

      public void To(TextWriter output, XPathSerializationOptions options) {

         OverrideSerialization(options);

         this.executable.Run(output, this.options);

         RestoreSerialization(options);
      }

      public void To(XmlWriter output) {
         To(output, null);
      }

      public void To(XmlWriter output, XPathSerializationOptions options) {

         OverrideSerialization(options);

         this.executable.Run(output, this.options);

         RestoreSerialization(options);
      }

      void OverrideSerialization(XPathSerializationOptions options) {

         if (options == null) {
            return;
         }

         this.options.Serialization = options;
      }

      void RestoreSerialization(XPathSerializationOptions options) {

         if (options == null) {
            return;
         }

         this.options.Serialization = this.defaultSerialization;
      }
   }
}
