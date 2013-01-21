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
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Web.Routing;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.common;
using myxsl.net.validation;
using InlineCacheByProcessor = System.Collections.Concurrent.ConcurrentDictionary<myxsl.net.common.IXsltProcessor, System.Collections.Concurrent.ConcurrentDictionary<System.Int32, myxsl.net.validation.SchematronValidator>>;
using UriCacheByProcessor = System.Collections.Concurrent.ConcurrentDictionary<myxsl.net.common.IXsltProcessor, System.Collections.Concurrent.ConcurrentDictionary<System.Uri, myxsl.net.validation.SchematronValidator>>;

namespace myxsl.net {

   public class SchematronInvoker {

      static readonly Lazy<UriCacheByProcessor> uriCache = new Lazy<UriCacheByProcessor>(() => new UriCacheByProcessor(), isThreadSafe: true);
      static readonly Lazy<InlineCacheByProcessor> inlineCache = new Lazy<InlineCacheByProcessor>(() => new InlineCacheByProcessor(), isThreadSafe: true);

      readonly SchematronValidator validator;
      readonly XmlResolver resolver;

      public static SchematronInvoker With(string schemaUri) {
         return With(schemaUri, (IXsltProcessor)null, Assembly.GetCallingAssembly());
      }

      public static SchematronInvoker With(string schemaUri, string processor) {
         return With(schemaUri, Processors.Xslt[processor], Assembly.GetCallingAssembly());
      }

      public static SchematronInvoker With(string schemaUri, IXsltProcessor processor) {
         return With(schemaUri, processor, Assembly.GetCallingAssembly());
      }

      static SchematronInvoker With(string schemaUri, IXsltProcessor processor, Assembly callingAssembly) {
         return With(new Uri(schemaUri, UriKind.RelativeOrAbsolute), processor, callingAssembly);
      }

      public static SchematronInvoker With(Uri schemaUri) {
         return With(schemaUri, (IXsltProcessor)null, Assembly.GetCallingAssembly());
      }

      public static SchematronInvoker With(Uri schemaUri, string processor) {
         return With(schemaUri, Processors.Xslt[processor], Assembly.GetCallingAssembly());
      }

      public static SchematronInvoker With(Uri schemaUri, IXsltProcessor processor) {
         return With(schemaUri, processor, Assembly.GetCallingAssembly());
      }

      static SchematronInvoker With(Uri schemaUri, IXsltProcessor processor, Assembly callingAssembly) {

         if (schemaUri == null) throw new ArgumentNullException("schemaUri");
         
         var resolver = new XmlDynamicResolver(callingAssembly);

         if (!schemaUri.IsAbsoluteUri)
            schemaUri = resolver.ResolveUri(null, schemaUri.OriginalString);

         if (processor == null) 
            processor = Processors.Xslt.DefaultProcessor;

         ConcurrentDictionary<Uri, SchematronValidator> cache =
            uriCache.Value.GetOrAdd(processor, p => new ConcurrentDictionary<Uri, SchematronValidator>());

         SchematronValidator validator = cache.GetOrAdd(schemaUri, u => {

            using (var schemaSource = (Stream)resolver.GetEntity(schemaUri, null, typeof(Stream))) {

               IXPathNavigable schemaDoc = processor.ItemFactory.CreateNodeReadOnly(schemaSource, new XmlParsingOptions {
                  BaseUri = schemaUri,
                  XmlResolver = resolver
               });

               return processor.CreateSchematronValidator(schemaDoc);
            }
         });

         return new SchematronInvoker(validator, resolver);
      }

      public static SchematronInvoker With(IXPathNavigable schema) {
         return With(schema, (IXsltProcessor)null, Assembly.GetCallingAssembly());
      }

      public static SchematronInvoker With(IXPathNavigable schema, string processor) {
         return With(schema, Processors.Xslt[processor], Assembly.GetCallingAssembly());
      }

      public static SchematronInvoker With(IXPathNavigable schema, IXsltProcessor processor) {
         return With(schema, processor, Assembly.GetCallingAssembly());
      }

      static SchematronInvoker With(IXPathNavigable schema, IXsltProcessor processor, Assembly callingAssembly) {

         if (schema == null) throw new ArgumentNullException("schema");

         if (processor == null)
            processor = Processors.Xslt.DefaultProcessor;

         int hashCode = XPathNavigatorEqualityComparer.Instance.GetHashCode(schema.CreateNavigator());

         ConcurrentDictionary<int, SchematronValidator> cache = 
            inlineCache.Value.GetOrAdd(processor, p => new ConcurrentDictionary<int, SchematronValidator>());

         SchematronValidator validator = cache.GetOrAdd(hashCode, i => processor.CreateSchematronValidator(schema));
         var resolver = new XmlDynamicResolver(callingAssembly);

         return new SchematronInvoker(validator, resolver);
      }

      private SchematronInvoker(SchematronValidator validator, XmlResolver resolver) {
         
         this.validator = validator;
         this.resolver = resolver;
      }

      public SchematronResultHandler Validate(Stream input) {
         return Validate(input, null);
      }

      public SchematronResultHandler Validate(Stream input, string phase) {
         return Validate(input, phase, null);
      }

      public SchematronResultHandler Validate(Stream input, string phase, object parameters) {
         return Validate(this.validator.ItemFactory.CreateNodeReadOnly(input), phase, parameters);
      }

      public SchematronResultHandler Validate(TextReader input) {
         return Validate(input, null);
      }

      public SchematronResultHandler Validate(TextReader input, string phase) {
         return Validate(input, phase, null);
      }

      public SchematronResultHandler Validate(TextReader input, string phase, object parameters) {
         return Validate(this.validator.ItemFactory.CreateNodeReadOnly(input), phase, parameters);
      }

      public SchematronResultHandler Validate(XmlReader input) {
         return Validate(input, null);
      }

      public SchematronResultHandler Validate(XmlReader input, string phase) {
         return Validate(input, phase, null);
      }

      public SchematronResultHandler Validate(XmlReader input, string phase, object parameters) {
         return Validate(this.validator.ItemFactory.CreateNodeReadOnly(input), phase, parameters);
      }

      public SchematronResultHandler Validate(IXPathNavigable input) {
         return Validate(input, null);
      }

      public SchematronResultHandler Validate(IXPathNavigable input, string phase) {
         return Validate(input, phase, null);
      }

      public SchematronResultHandler Validate(IXPathNavigable input, string phase, object parameters) {

         if (input == null) throw new ArgumentNullException("input");

         var options = new SchematronRuntimeOptions {
            Instance = input,
            InputXmlResolver = this.resolver,
            Phase = phase
         };

         if (parameters != null) {
            var paramDictionary = new RouteValueDictionary(parameters);

            foreach (var pair in paramDictionary)
               options.Parameters.Add(new XmlQualifiedName(pair.Key), pair.Value);
         }

         return Validate(options);
      }

      public SchematronResultHandler Validate(object input) {
         return Validate(input, null);
      }

      public SchematronResultHandler Validate(object input, string phase) {
         return Validate(input, phase, null);

      }

      public SchematronResultHandler Validate(object input, string phase, object parameters) {
         return Validate(this.validator.ItemFactory.CreateDocument(input), phase, parameters);
      }

      public SchematronResultHandler Validate(SchematronRuntimeOptions options) {
         return new SchematronResultHandler(this.validator, options);
      }
   }
}
