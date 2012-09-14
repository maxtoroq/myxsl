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
using myxsl.net.common;
using Saxon.Api;

namespace myxsl.net.saxon {

   public sealed class SaxonXQueryExecutable : common.XQueryExecutable {

      readonly Saxon.Api.XQueryExecutable executable;
      readonly SaxonProcessor _Processor;
      readonly Uri _StaticBaseUri;

      public new SaxonProcessor Processor { get { return _Processor; } }
      protected override IXQueryProcessor XQueryProcessor { get { return this.Processor; } }

      public override Uri StaticBaseUri { get { return this._StaticBaseUri; } }

      internal SaxonXQueryExecutable(Saxon.Api.XQueryExecutable executable, SaxonProcessor processor, Uri staticBaseUri) {
         
         this.executable = executable;
         this._Processor = processor;
         this._StaticBaseUri = staticBaseUri;
      }

      public override void Run(Stream output, XQueryRuntimeOptions options) {

         if (options == null) throw new ArgumentNullException("options");

         Serializer serializer = this.Processor.ItemFactory.CreateSerializer(options.Serialization);
         serializer.SetOutputStream(output);

         Run(serializer, options);
      }

      public override void Run(TextWriter output, XQueryRuntimeOptions options) {

         if (options == null) throw new ArgumentNullException("options");

         Serializer serializer = this.Processor.ItemFactory.CreateSerializer(options.Serialization);
         serializer.SetOutputWriter(output);

         Run(serializer, options);
      }

      public override void Run(XmlWriter output, XQueryRuntimeOptions options) {

         XmlDestination builder = new TextWriterDestination(output);

         Run(builder, options);
      }

      public override IEnumerable<XPathItem> Run(XQueryRuntimeOptions options) {

         XQueryEvaluator eval = GetEvaluator(options);

         XdmValue val;

         try {
            val = eval.Evaluate();

         } catch (DynamicError ex) {
            throw new SaxonException(ex);

         } catch (Exception ex) {
            throw new SaxonException(ex.Message, ex);
         }

         return val.ToXPathItems();
      }

      void Run(XmlDestination destination, XQueryRuntimeOptions options) {

         XQueryEvaluator eval = GetEvaluator(options);

         try {
            eval.Run(destination);

         } catch (DynamicError ex) {
            throw new SaxonException(ex);

         } catch (Exception ex) {
            throw new SaxonException(ex.Message, ex);
         }
      }
      
      XQueryEvaluator GetEvaluator(XQueryRuntimeOptions options) {
         
         if (options == null) throw new ArgumentNullException("options");
         
         XQueryEvaluator eval = this.executable.Load();

         if (options.InputXmlResolver != null)
            eval.InputXmlResolver = options.InputXmlResolver;

         if (options.ContextItem != null) 
            eval.ContextItem = options.ContextItem.ToXdmItem(this.Processor.ItemFactory);

         foreach (var pair in options.ExternalVariables) {

            var qname = new QName(pair.Key);
            XdmValue xdmValue = pair.Value.ToXdmValue(this.Processor.ItemFactory);

            eval.SetExternalVariable(qname, xdmValue);
         }

         return eval;
      }
   }
}
