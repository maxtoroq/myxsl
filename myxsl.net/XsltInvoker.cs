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
using InlineCacheByProcessor = System.Collections.Concurrent.ConcurrentDictionary<myxsl.net.common.IXsltProcessor, System.Collections.Concurrent.ConcurrentDictionary<System.Int32, myxsl.net.common.XsltExecutable>>;
using UriCacheByProcessor = System.Collections.Concurrent.ConcurrentDictionary<myxsl.net.common.IXsltProcessor, System.Collections.Concurrent.ConcurrentDictionary<System.Uri, myxsl.net.common.XsltExecutable>>;

namespace myxsl.net {
   
   public class XsltInvoker {

      static readonly UriCacheByProcessor uriCache = new UriCacheByProcessor();
      static readonly InlineCacheByProcessor inlineCache = new InlineCacheByProcessor();

      readonly XsltExecutable executable;
      readonly Assembly withCallingAssembly;

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
         
         if (!stylesheetUri.IsAbsoluteUri)
            stylesheetUri = resolver.ResolveUri(null, stylesheetUri.OriginalString);

         if (processor == null)
            processor = Processors.Xslt.DefaultProcessor;

         ConcurrentDictionary<Uri, XsltExecutable> cache = 
            uriCache.GetOrAdd(processor, p => new ConcurrentDictionary<Uri, XsltExecutable>());

         XsltExecutable executable = cache.GetOrAdd(stylesheetUri, u => {

            using (var stylesheetSource = (Stream)resolver.GetEntity(stylesheetUri, null, typeof(Stream))) {
               return processor.Compile(stylesheetSource, new XsltCompileOptions {
                  BaseUri = stylesheetUri,
                  XmlResolver = resolver
               });
            }
         });

         return new XsltInvoker(executable, callingAssembly);
      }

      public static XsltInvoker With(IXPathNavigable stylesheet) {
         return With(stylesheet, (IXsltProcessor)null, Assembly.GetCallingAssembly());
      }

      public static XsltInvoker With(IXPathNavigable stylesheet, string processor) {
         return With(stylesheet, Processors.Xslt[processor], Assembly.GetCallingAssembly());
      }

      public static XsltInvoker With(IXPathNavigable stylesheet, IXsltProcessor processor) {
         return With(stylesheet, processor, Assembly.GetCallingAssembly());
      }

      static XsltInvoker With(IXPathNavigable stylesheet, IXsltProcessor processor, Assembly callingAssembly) {

         if (stylesheet == null) throw new ArgumentNullException("stylesheet");

         var resolver = new XmlDynamicResolver(callingAssembly);

         if (processor == null)
            processor = Processors.Xslt.DefaultProcessor;

         int hashCode = XPathNavigatorEqualityComparer.Instance.GetHashCode(stylesheet.CreateNavigator());

         ConcurrentDictionary<int, XsltExecutable> cache =
            inlineCache.GetOrAdd(processor, p => new ConcurrentDictionary<int, XsltExecutable>());

         XsltExecutable exec = cache.GetOrAdd(hashCode, i => 
            processor.Compile(stylesheet, new XsltCompileOptions {
               XmlResolver = resolver
            })
         );

         return new XsltInvoker(exec, callingAssembly);
      }

      private XsltInvoker(XsltExecutable executable, Assembly withCallingAssembly) {
         
         this.executable = executable;
         this.withCallingAssembly = withCallingAssembly;
      }

      public XsltResultHandler Transform(Stream input) {
         return Transform(input, null);
      }

      public XsltResultHandler Transform(Stream input, object parameters) {

         IXPathNavigable doc = this.executable.Processor.ItemFactory
            .CreateNodeReadOnly(input);

         return Transform(doc, parameters);
      }

      public XsltResultHandler Transform(TextReader input) {
         return Transform(input, null);
      }

      public XsltResultHandler Transform(TextReader input, object parameters) {

         IXPathNavigable doc = this.executable.Processor.ItemFactory
            .CreateNodeReadOnly(input);

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
            InputXmlResolver = new XmlDynamicResolver(this.withCallingAssembly)
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
