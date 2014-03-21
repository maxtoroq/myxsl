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
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;
using myxsl.net.common;

namespace myxsl.net.system {

   public sealed class SystemXsltProcessor : IXsltProcessor {

      readonly SystemItemFactory _ItemFactory;

      public XsltSettings Settings { get; set; }
      public SystemItemFactory ItemFactory { get { return _ItemFactory; } }

      XPathItemFactory IXsltProcessor.ItemFactory { get { return this.ItemFactory; } }

      public SystemXsltProcessor() {
         
         this.Settings = XsltSettings.TrustedXslt;
         this._ItemFactory = new SystemItemFactory();
      }

      public XsltExecutable Compile(Stream module, XsltCompileOptions options) {
         return Compile(this.ItemFactory.CreateNodeReadOnly(module, new XmlParsingOptions { BaseUri = options.BaseUri, XmlResolver = options.XmlResolver }), options);
      }

      public XsltExecutable Compile(TextReader module, XsltCompileOptions options) {
         return Compile(this.ItemFactory.CreateNodeReadOnly(module, new XmlParsingOptions { BaseUri = options.BaseUri, XmlResolver = options.XmlResolver }), options);
      }

      public XsltExecutable Compile(XmlReader module, XsltCompileOptions options) {

         XslCompiledTransform transform = CreateTransform();

         try {
            transform.Load(module, this.Settings, options.XmlResolver);
         } catch (XsltException ex) {
            throw new SystemXsltException(ex);
         }

         return CreateExecutable(transform, options, module.BaseURI);
      }

      public XsltExecutable Compile(IXPathNavigable module, XsltCompileOptions options) {

         XslCompiledTransform transform = CreateTransform();

         XPathNavigator nav = module.CreateNavigator();

         try {
            transform.Load(nav, this.Settings, options.XmlResolver);
         } catch (XsltException ex) {
            throw new SystemXsltException(ex);
         }

         return CreateExecutable(transform, options, nav.BaseURI);
      }

      static XslCompiledTransform CreateTransform() {
         return new XslCompiledTransform(System.Diagnostics.Debugger.IsAttached);
      }

      XsltExecutable CreateExecutable(XslCompiledTransform transform, XsltCompileOptions options, string baseUri) {

         Uri parsedBaseUri = null;

         if (!String.IsNullOrEmpty(baseUri)) {
            try {
               parsedBaseUri = new Uri(baseUri);
            } catch (UriFormatException) { }
         }

         return CreateExecutable(transform, options, parsedBaseUri);
      }

      XsltExecutable CreateExecutable(XslCompiledTransform transform, XsltCompileOptions options, Uri baseUri) {
         return new SystemXsltExecutable(transform, this, baseUri ?? options.BaseUri);      
      }
   }
}
