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
using System.Web.Compilation;
using System.Web.Routing;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using myxsl.net.web.ui;
using myxsl.net.common;
using CacheByProcessor = System.Collections.Concurrent.ConcurrentDictionary<myxsl.net.common.IXsltProcessor, System.Collections.Concurrent.ConcurrentDictionary<System.Uri, myxsl.net.common.XsltExecutable>>;

namespace myxsl.net {
   
   public class XsltInvoker {

      static readonly CacheByProcessor cacheByProc = new CacheByProcessor();

      readonly XsltExecutable executable;
      readonly XmlResolver resolver;

      public static XsltInvoker With(string stylesheetUri) {
         return With(stylesheetUri, (IXsltProcessor)null, Assembly.GetCallingAssembly());
      }

      public static XsltInvoker With(string stylesheetUri, string processor) {
         return With(stylesheetUri, Processors.Xslt[processor], Assembly.GetCallingAssembly());
      }

      public static XsltInvoker With(string stylesheetUri, IXsltProcessor processor) {
         return With(stylesheetUri, processor, Assembly.GetCallingAssembly());
      }

      static XsltInvoker With(string stylesheetUri, IXsltProcessor processor, Assembly callingAssembly) {
         return With(new Uri(stylesheetUri, UriKind.RelativeOrAbsolute), processor, callingAssembly);
      }

      public static XsltInvoker With(Uri stylesheetUri) {
         return With(stylesheetUri, (IXsltProcessor)null, Assembly.GetCallingAssembly());
      }

      public static XsltInvoker With(Uri stylesheetUri, string processor) {
         return With(stylesheetUri, Processors.Xslt[processor], Assembly.GetCallingAssembly());
      }

      public static XsltInvoker With(Uri stylesheetUri, IXsltProcessor processor) {
         return With(stylesheetUri, processor, Assembly.GetCallingAssembly());
      }

      static XsltInvoker With(Uri stylesheetUri, IXsltProcessor processor, Assembly callingAssembly) {

         if (stylesheetUri == null) throw new ArgumentNullException("stylesheetUri");

         var resolver = new XmlDynamicResolver(callingAssembly);
         
         string originalUri = stylesheetUri.OriginalString;

         if (!stylesheetUri.IsAbsoluteUri)
            stylesheetUri = resolver.ResolveUri(null, originalUri);

         if (processor == null)
            processor = Processors.Xslt.DefaultProcessor;

         ConcurrentDictionary<Uri, XsltExecutable> cache = 
            cacheByProc.GetOrAdd(processor, p => new ConcurrentDictionary<Uri, XsltExecutable>());

         XsltExecutable executable = cache.GetOrAdd(stylesheetUri, u => {

            XsltExecutable exec = null;

            if (originalUri[0] == '~') {
               XsltPage page = BuildManager.CreateInstanceFromVirtualPath(originalUri, typeof(object)) as XsltPage;

               if (page != null
                  && processor.GetType() == page.Executable.Processor.GetType()) {
                  
                  exec = page.Executable;
               }
            }

            if (exec == null) {

               using (var stylesheetSource = (Stream)resolver.GetEntity(stylesheetUri, null, typeof(Stream))) {
                  exec = processor.Compile(stylesheetSource, new XsltCompileOptions {
                     BaseUri = stylesheetUri,
                     XmlResolver = resolver
                  });
               }
            }

            return exec;
         });

         return new XsltInvoker(executable, resolver);
      }

      private XsltInvoker(XsltExecutable executable, XmlResolver resolver) {
         
         this.executable = executable;
         this.resolver = resolver;
      }

      public XsltResultHandler Transform(Stream input) {
         return Transform(input, null);
      }

      public XsltResultHandler Transform(Stream input, object parameters) {

         IXPathNavigable doc = this.executable.Processor.ItemFactory
            .CreateNodeReadOnly(input, new XmlParsingOptions {
               XmlResolver = this.resolver
            });

         return Transform(doc, parameters);
      }

      public XsltResultHandler Transform(TextReader input) {
         return Transform(input, null);
      }

      public XsltResultHandler Transform(TextReader input, object parameters) {

         IXPathNavigable doc = this.executable.Processor.ItemFactory
            .CreateNodeReadOnly(input, new XmlParsingOptions { 
               XmlResolver = this.resolver
            });

         return Transform(doc, parameters);
      }

      public XsltResultHandler Transform(XmlReader input) {
         return Transform(input, null);
      }

      public XsltResultHandler Transform(XmlReader input, object parameters) {

         IXPathNavigable doc = this.executable.Processor.ItemFactory.CreateNodeReadOnly(input);

         return Transform(doc, parameters);
      }

      public XsltResultHandler Transform(IXPathNavigable input) {
         return Transform(input, null);
      }

      public XsltResultHandler Transform(IXPathNavigable input, object parameters) {

         if (input == null) throw new ArgumentNullException("input");

         var options = new XsltRuntimeOptions { 
            InitialContextNode = input,
            InputXmlResolver = this.resolver,
            BaseOutputUri = this.executable.StaticBaseUri
         };

         if (parameters != null) {
            var paramDictionary = new RouteValueDictionary(parameters);

            foreach (var pair in paramDictionary) 
               options.Parameters.Add(new XmlQualifiedName(pair.Key), pair.Value);
         }

         return Transform(options);
      }

      public XsltResultHandler Transform(object input) {
         return Transform(input, null);
      }

      public XsltResultHandler Transform(object input, object parameters) {
         return Transform(this.executable.Processor.ItemFactory.CreateDocument(input), parameters);
      }

      public XsltResultHandler Transform(XsltRuntimeOptions options) {
         return new XsltResultHandler(this.executable, options);
      }
   }
}
