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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Routing;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.common;
using myxsl.net.util;
using myxsl.net.validation;
using myxsl.net.web.ui;
using CacheByProcessor = System.Collections.Concurrent.ConcurrentDictionary<myxsl.net.common.IXsltProcessor, System.Collections.Concurrent.ConcurrentDictionary<System.Uri, myxsl.net.validation.SchematronValidator>>;

namespace myxsl.net {

   public class SchematronInvoker {

      static readonly CacheByProcessor cacheByProc = new CacheByProcessor();
      static readonly ConcurrentDictionary<Uri, SchematronValidator> cachePrecompiled = new ConcurrentDictionary<Uri, SchematronValidator>();

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

         SchematronValidator validator = null;

         if (schemaUri.Scheme == TypeResolver.UriSchemeClitype) {

            validator = cachePrecompiled.GetOrAdd(schemaUri, u => {

               Type schemaType = TypeResolver.ResolveUri(schemaUri);

               if (schemaType == null)
                  throw new ArgumentException("Could not found schema type {0}.".FormatInvariant(schemaUri), "u");

               if (!typeof(SchematronValidator).IsAssignableFrom(schemaType))
                  throw new ArgumentException("The type must be derived from {0}.".FormatInvariant(typeof(SchematronValidator).FullName), "u");

               return (SchematronValidator)Activator.CreateInstance(schemaType);
            });
         }

         if (validator == null) {

            IXPathNavigable schemaDoc = null;

            if (processor == null) {

               using (var schemaSource = (Stream)resolver.GetEntity(schemaUri, null, typeof(Stream))) {
                  using (var schemaReader = XmlReader.Create(schemaSource, new XmlReaderSettings { 
                     DtdProcessing = DtdProcessing.Ignore, 
                     IgnoreComments = true, 
                     IgnoreWhitespace = true,
                     XmlResolver = resolver }, schemaUri.AbsoluteUri)) {

                     while (schemaReader.Read()) {

                        if (processor == null
                           && schemaReader.NodeType == XmlNodeType.ProcessingInstruction
                           && schemaReader.Name == SchematronParser.validator.it) {

                           IDictionary<string, string> attribs = PseudoAttributeParser.GetAttributes(schemaReader.Value);

                           string procKey = SchematronParser.validator.processor;

                           if (attribs.ContainsKey(procKey))
                              processor = Processors.Xslt[attribs[procKey]];

                           continue;
                        }

                        if (schemaReader.NodeType == XmlNodeType.Element)
                           break;
                     }

                     // Saxon calls Read and ignores current node
                     //schemaDoc = processor.ItemFactory.CreateNodeReadOnly(schemaReader);
                     schemaDoc = new system.SystemItemFactory().CreateNodeReadOnly(schemaReader);
                  }
               }
            }

            if (processor == null)
               processor = Processors.Xslt.DefaultProcessor;

            ConcurrentDictionary<Uri, SchematronValidator> cache =
               cacheByProc.GetOrAdd(processor, p => new ConcurrentDictionary<Uri, SchematronValidator>());

            validator = cache.GetOrAdd(schemaUri, u => {

               if (schemaDoc == null) {
                  using (var schemaSource = (Stream)resolver.GetEntity(schemaUri, null, typeof(Stream))) {

                     schemaDoc = processor.ItemFactory.CreateNodeReadOnly(schemaSource, new XmlParsingOptions {
                        BaseUri = schemaUri,
                        XmlResolver = resolver
                     });
                  }
               }
               
               return processor.CreateSchematronValidator(schemaDoc);
            });
         }

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
