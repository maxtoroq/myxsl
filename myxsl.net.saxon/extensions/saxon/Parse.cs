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
using myxsl.net.common;
using Saxon.Api;

namespace myxsl.net.saxon.extensions.saxon {
   
   sealed class Parse : ExtensionFunctionDefinition {

      readonly QName _FunctionName = new QName(Index.Namespace, "parse");

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), ' ')
      };

      readonly XdmSequenceType resultType = new XdmSequenceType(XdmNodeKind.Document, ' ');
      readonly SaxonItemFactory itemFactory;

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override int MaximumNumberOfArguments {
         get { return 1; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public Parse(SaxonItemFactory itemFactory) {
         this.itemFactory = itemFactory;
      }

      public override XdmSequenceType ResultType(XdmSequenceType[] ArgumentTypes) {
         return this.resultType;
      }

      public override ExtensionFunctionCall MakeFunctionCall() {
         return new FunctionCall(this.itemFactory);
      }

      class FunctionCall : ExtensionFunctionCall {

         readonly SaxonItemFactory itemFactory;

         public FunctionCall(SaxonItemFactory itemFactory) {
            this.itemFactory = itemFactory;
         }

         public override IXdmEnumerator Call(IXdmEnumerator[] arguments, DynamicContext context) {

            string xml = arguments[0].AsAtomicValues().Single().ToString();

            var parseOptions = new XmlParsingOptions { 
              ConformanceLevel = ConformanceLevel.Document,
              BaseUri = new Uri("", UriKind.Relative) // Saxon requires a base URI
            };

            using (var reader = new StringReader(xml)) {
               
               return this.itemFactory.CreateNodeReadOnly(reader, parseOptions)
                  .ToXdmNode(this.itemFactory)
                  .GetXdmEnumerator();
            }
         }
      }
   }
}
