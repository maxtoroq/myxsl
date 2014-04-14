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
using myxsl.common;
using UriCacheByProcessor = System.Collections.Concurrent.ConcurrentDictionary<myxsl.common.IXQueryProcessor, System.Collections.Concurrent.ConcurrentDictionary<System.Uri, myxsl.common.XQueryExecutable>>;
using InlineCacheByProcessor = System.Collections.Concurrent.ConcurrentDictionary<myxsl.common.IXQueryProcessor, System.Collections.Concurrent.ConcurrentDictionary<System.Int32, myxsl.common.XQueryExecutable>>;

namespace myxsl.xquery {
   
   public class XQueryInvoker {

      static readonly UriCacheByProcessor uriCache = new UriCacheByProcessor();
      static readonly InlineCacheByProcessor inlineCache = new InlineCacheByProcessor();
      
      readonly XQueryExecutable executable;
      readonly Assembly withCallingAssembly;

      public static XQueryInvoker With(string queryUri) {
         return With(queryUri, (IXQueryProcessor)null, Assembly.GetCallingAssembly());
      }

      public static XQueryInvoker With(string queryUri, string processor) {
         return With(queryUri, (processor != null) ? Processors.XQuery[processor] : null, Assembly.GetCallingAssembly());
      }

      public static XQueryInvoker With(string queryUri, IXQueryProcessor processor) {
         return With(queryUri, processor, Assembly.GetCallingAssembly());
      }

      static XQueryInvoker With(string queryUri, IXQueryProcessor processor, Assembly callingAssembly) {
         return With(new Uri(queryUri, UriKind.RelativeOrAbsolute), processor, callingAssembly);
      }

      public static XQueryInvoker With(Uri queryUri) {
         return With(queryUri, (IXQueryProcessor)null, Assembly.GetCallingAssembly());
      }

      public static XQueryInvoker With(Uri queryUri, string processor) {
         return With(queryUri, (processor != null) ? Processors.XQuery[processor] : null, Assembly.GetCallingAssembly());
      }

      public static XQueryInvoker With(Uri queryUri, IXQueryProcessor processor) {
         return With(queryUri, processor, Assembly.GetCallingAssembly());
      }

      static XQueryInvoker With(Uri queryUri, IXQueryProcessor processor, Assembly callingAssembly) {

         if (queryUri == null) throw new ArgumentNullException("queryUri");

         var resolver = new XmlDynamicResolver(callingAssembly);

         if (!queryUri.IsAbsoluteUri) {
            queryUri = resolver.ResolveUri(null, queryUri.OriginalString);
         }

         if (processor == null) {
            processor = Processors.XQuery.DefaultProcessor;
         }

         ConcurrentDictionary<Uri, XQueryExecutable> cache =
            uriCache.GetOrAdd(processor, p => new ConcurrentDictionary<Uri, XQueryExecutable>());

         XQueryExecutable executable = cache.GetOrAdd(queryUri, u => {

            using (var stylesheetSource = (Stream)resolver.GetEntity(queryUri, null, typeof(Stream))) {
               return processor.Compile(stylesheetSource, new XQueryCompileOptions {
                  BaseUri = queryUri,
                  XmlResolver = resolver
               });
            }
         });

         return new XQueryInvoker(executable, callingAssembly);
      }

      public static XQueryInvoker WithQuery(string query) {
         return WithQuery(query, (IXQueryProcessor)null, Assembly.GetCallingAssembly());
      }

      public static XQueryInvoker WithQuery(string query, string processor) {
         return WithQuery(query, (processor != null) ? Processors.XQuery[processor] : null, Assembly.GetCallingAssembly());
      }

      public static XQueryInvoker WithQuery(string query, IXQueryProcessor processor) {
         return WithQuery(query, processor, Assembly.GetCallingAssembly());
      }

      static XQueryInvoker WithQuery(string query, IXQueryProcessor processor, Assembly callingAssembly) {

         int hashCode;

         return WithQuery(query, processor, callingAssembly, out hashCode);
      }

      internal static XQueryInvoker WithQuery(string query, IXQueryProcessor processor, Assembly callingAssembly, out int hashCode) {

         if (query == null) throw new ArgumentNullException("query");

         if (processor == null) {
            processor = Processors.XQuery.DefaultProcessor;
         }

         hashCode = query.GetHashCode();

         ConcurrentDictionary<int, XQueryExecutable> cache =
            inlineCache.GetOrAdd(processor, p => new ConcurrentDictionary<int, XQueryExecutable>());

         XQueryExecutable exec = cache.GetOrAdd(hashCode, i => {

            var resolver = new XmlDynamicResolver(callingAssembly);

            return processor.Compile(new StringReader(query), new XQueryCompileOptions {
               XmlResolver = resolver
            });
         });

         return new XQueryInvoker(exec, callingAssembly);
      }

      internal static XQueryInvoker WithQuery(int stylesheetHashCode, IXQueryProcessor processor) {

         if (processor == null) {
            processor = Processors.XQuery.DefaultProcessor;
         }

         return new XQueryInvoker(inlineCache[processor][stylesheetHashCode], null);
      }

      private XQueryInvoker(XQueryExecutable executable, Assembly withCallingAssembly) {
         
         this.executable = executable;
         this.withCallingAssembly = withCallingAssembly;
      }

      public XQueryResultHandler Query() {
         return Query((object)null);
      }

      public XQueryResultHandler Query(Stream inputNode) {
         return Query(inputNode, null);
      }

      public XQueryResultHandler Query(Stream inputNode, object parameters) {

         IXPathNavigable doc = this.executable.Processor.ItemFactory
            .CreateNodeReadOnly(inputNode);

         return Query(doc, parameters);
      }

      public XQueryResultHandler Query(TextReader inputNode) {
         return Query(inputNode, null);
      }

      public XQueryResultHandler Query(TextReader inputNode, object parameters) {

         IXPathNavigable doc = this.executable.Processor.ItemFactory
            .CreateNodeReadOnly(inputNode);

         return Query(doc, parameters);
      }

      public XQueryResultHandler Query(XmlReader inputNode) {
         return Query(inputNode, null);
      }

      public XQueryResultHandler Query(XmlReader inputNode, object parameters) {

         IXPathNavigable doc = this.executable.Processor.ItemFactory
            .CreateNodeReadOnly(inputNode);

         return Query(doc, parameters);
      }

      public XQueryResultHandler Query(object input) {
         return Query(input, null);
      }

      public XQueryResultHandler Query(object input, object parameters) {

         if (input == null) throw new ArgumentNullException("input");

         var options = new XQueryRuntimeOptions {
            ContextItem = input,
            InputXmlResolver = new XmlDynamicResolver(this.withCallingAssembly)
         };

         if (parameters != null) {
            
            var paramDictionary = new RouteValueDictionary(parameters);

            foreach (var pair in paramDictionary) {
               options.ExternalVariables.Add(new XmlQualifiedName(pair.Key), pair.Value);
            }
         }

         return Query(options);
      }

      public XQueryResultHandler Query(XQueryRuntimeOptions options) {
         return new XQueryResultHandler(this.executable, options);
      }
   }
}
