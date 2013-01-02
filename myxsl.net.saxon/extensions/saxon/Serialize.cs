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
using System.IO;
using System.Linq;
using myxsl.net.common;
using Saxon.Api;

namespace myxsl.net.saxon.extensions.saxon {

   sealed class Serialize : ExtensionFunctionDefinition {

      readonly QName _FunctionName = new QName(Index.Namespace, "serialize");

      readonly XdmSequenceType[] _ArgumentTypes = new[] {
         new XdmSequenceType(XdmAnyNodeType.Instance, ' '),
         new XdmSequenceType(XdmAnyItemType.Instance, ' '),
      };

      readonly XdmSequenceType resultType = new XdmSequenceType(XdmAtomicType.BuiltInAtomicType(QName.XS_STRING), ' ');
      readonly SaxonItemFactory itemFactory;

      public override QName FunctionName {
         get { return _FunctionName; }
      }

      public override XdmSequenceType[] ArgumentTypes {
         get { return _ArgumentTypes; }
      }

      public override int MaximumNumberOfArguments {
         get { return 2; }
      }

      public override int MinimumNumberOfArguments {
         get { return 1; }
      }

      public Serialize(SaxonItemFactory itemFactory) {
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

            XdmNode node = arguments[0].AsNodes().Single();

            var options = new XPathSerializationOptions();

            XdmItem arg2 = arguments[1].AsItems().SingleOrDefault();

            if (arg2 != null) {

               if (arg2.IsAtomic()) {

                  string methodLexical = arg2.ToString();

                  QName method = (context.ContextItem == null || context.ContextItem.IsAtomic()) ?
                     new QName(methodLexical)
                     : new QName(methodLexical, (XdmNode)context.ContextItem);

                  options.Method = method.ToXmlQualifiedName();

               } else {
                  // TODO: xsl:output
                  throw new NotImplementedException();
               }
            }

            Serializer serializer = this.itemFactory.CreateSerializer(options);

            using (var writer = new StringWriter()) {

               serializer.SetOutputWriter(writer);

               this.itemFactory.processor.WriteXdmValue(node, serializer);

               return writer.ToString().ToXdmAtomicValue().GetXdmEnumerator();
            }
         }
      }
   }
}
