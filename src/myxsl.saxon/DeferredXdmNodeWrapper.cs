// Copyright 2014 Max Toro Q.
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
using System.Xml;
using System.Xml.XPath;
using Saxon.Api;
using net.sf.saxon;
using net.sf.saxon.@event;
using net.sf.saxon.lib;

namespace myxsl.saxon {

   sealed class DeferredXdmNodeWrapper : IXPathNavigable {

      internal Configuration config;
      internal XdmNode node;

      public DeferredXdmNodeWrapper(Configuration config) {
         
         if (config == null) throw new ArgumentNullException("config");

         this.config = config;
      }

      public XPathNavigator CreateNavigator() {

         if (this.node != null) {
            return new XdmNodeNavigator(this.node);
         }

         return new DeferredXdmNodeWrapper.DeferredNavigator(this);
      }

      sealed class DeferredNavigator : XdmNodeNavigator {

         DeferredXdmNodeWrapper nodeWrapper;

         public DeferredNavigator(DeferredXdmNodeWrapper nodeWrapper) {

            if (nodeWrapper == null) throw new ArgumentNullException("nodeWrapper");

            this.nodeWrapper = nodeWrapper;
         }

         public override XmlWriter AppendChild() {

            if (this.currentNode != null
               || this.nodeWrapper == null
               || this.nodeWrapper.node != null) {

               throw new InvalidOperationException();
            }

            var config = this.nodeWrapper.config;
            var options = new ParseOptions();
            var model = options.getModel();
            var pc = config.makePipelineConfiguration();

            var builder = model.makeBuilder(pc);

            var baseWriter = new DeferredNavigator.Writer(builder, this);

            // wrapping XdmWriter with XmlWellFormedWriter which implements LookupPrefix
            // required by XmlSerializer

            return XmlWriter.Create(baseWriter);
         }

         class Writer : XdmWriter {

            readonly DeferredNavigator nodeNavigator;
            readonly XmlWriterSettings settings;

            public override XmlWriterSettings Settings {
               get {
                  return settings;
               }
            }

            public Writer(Builder builder, DeferredNavigator nodeNavigator)
               : base(builder) {

               this.nodeNavigator = nodeNavigator;

               // using an XmlWriterSettings with ConformanceLevel = Fragment
               // enables this instance to be wrapped by XmlWellFormedWriter
               // see XmlWriterSettings.AddConformanceWrapper

               this.settings = new XmlWriterSettings {
                  ConformanceLevel = ConformanceLevel.Fragment
               };
            }

            public override void Close() {

               base.Close();

               if (this.nodeNavigator != null
                  && this.nodeNavigator.nodeWrapper != null
                  && this.nodeNavigator.nodeWrapper.node == null) {

                  this.nodeNavigator.nodeWrapper.node = this.nodeNavigator.currentNode = this.Document;
                  this.nodeNavigator.nodeWrapper.config = null;
                  this.nodeNavigator.nodeWrapper = null;
               }
            }
         }
      }
   }
}
