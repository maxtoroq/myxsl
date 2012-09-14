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
using System.Configuration;
using System.Text;
using System.Web.Configuration;
using System.Xml;
using myxsl.net.configuration.web;
using myxsl.net.web;
using myxsl.net.web.compilation;

namespace myxsl.net.configuration {

   sealed class LibraryConfigSection : ConfigurationSection {

      internal static readonly string SectionName = "myxsl.net";

      static readonly ConfigurationPropertyCollection _Properties;
      static readonly ConfigurationProperty _ProcessorsProperty;
      static readonly ConfigurationProperty _ResolversProperty;
      static readonly ConfigurationProperty _XsltProperty;
      static readonly ConfigurationProperty _XQueryProperty;
      static readonly ConfigurationProperty _WebProperty;

      static LibraryConfigSection _Instance;
      static readonly object padlock = new object();

      ProcessorElementCollection _Processors;
      ResolverElementCollection _Resolvers;
      XsltElement _Xslt;
      XQueryElement _XQuery;
      WebElement _Web;

      public static LibraryConfigSection Instance {
         get {
            if (_Instance == null) {
               lock (padlock) {
                  if (_Instance == null) {

                     LibraryConfigSection configSection = ConfigurationManager.GetSection(SectionName) as LibraryConfigSection;
                     
                     if (configSection == null) {
                        configSection = new LibraryConfigSection();
                        configSection.Init();
                        configSection.InitializeDefault();
                     }
                     _Instance = configSection;
                  }
               }
            }
            return _Instance;
         }
      }

      protected override ConfigurationPropertyCollection Properties {
         get { return _Properties; }
      }

      public ProcessorElementCollection Processors {
         get {
            if (_Processors == null)
               _Processors = (ProcessorElementCollection)base[_ProcessorsProperty];
            return _Processors;
         }
      }

      public ResolverElementCollection Resolvers {
         get {
            if (_Resolvers == null)
               _Resolvers = (ResolverElementCollection)base[_ResolversProperty];
            return _Resolvers;
         }
      }

      public XsltElement Xslt {
         get {
            if (_Xslt == null)
               _Xslt = (XsltElement)base[_XsltProperty];
            return _Xslt;
         }
      }

      public XQueryElement XQuery {
         get {
            if (_XQuery == null)
               _XQuery = (XQueryElement)base[_XQueryProperty];
            return _XQuery;
         }
      }

      public WebElement Web {
         get {
            if (_Web == null)
               _Web = (WebElement)base[_WebProperty];
            return _Web;
         }
      }

      static LibraryConfigSection() {
         
         _ProcessorsProperty = new ConfigurationProperty("processors", typeof(ProcessorElementCollection));
         _ResolversProperty = new ConfigurationProperty("resolvers", typeof(ResolverElementCollection));
         _XsltProperty = new ConfigurationProperty("xslt", typeof(XsltElement));
         _XQueryProperty = new ConfigurationProperty("xquery", typeof(XQueryElement));
         _WebProperty = new ConfigurationProperty("web", typeof(WebElement));

         _Properties = new ConfigurationPropertyCollection { 
            _ProcessorsProperty, 
            _ResolversProperty, 
            _XsltProperty,
            _XQueryProperty,
            _WebProperty
         };
      }

      private LibraryConfigSection() { }

      protected override void InitializeDefault() {

         var sysProc = new ProcessorElement {
            Name = "system",
            Type = typeof(system.SystemXsltProcessor).AssemblyQualifiedName,
            LockItem = true
         };

         this.Processors.Add(sysProc);
         this.Xslt.DefaultProcessor = sysProc.Name;

         ExpressionBuilderElementCollection exprBuilders = this.Web.Compilation.ExpressionBuilders;

         exprBuilders.Add(
            new ExpressionBuilderElement { 
               Namespace = RequestExpressionBuilder.Namespace,
               Type = typeof(RequestExpressionBuilder).AssemblyQualifiedName,
               LockItem = true
            }
         );

         exprBuilders.Add(
            new ExpressionBuilderElement {
               Namespace = SessionExpressionBuilder.Namespace,
               Type = typeof(SessionExpressionBuilder).AssemblyQualifiedName,
               LockItem = true
            }
         );

         exprBuilders.Add(
            new ExpressionBuilderElement { 
               Namespace = CodeExpressionBuilder.Namespace,
               Type = typeof(CodeExpressionBuilder).AssemblyQualifiedName,
               LockItem = true
            }
         );

         ResolverElementCollection resolvers = this.Resolvers;

         resolvers.Add(
            new ResolverElement { 
               Scheme = Uri.UriSchemeFile,
               Type = typeof(XmlVirtualPathAwareUrlResolver).AssemblyQualifiedName
            }
         );

         resolvers.Add(
             new ResolverElement { 
               Scheme = Uri.UriSchemeHttp,
               Type = typeof(XmlVirtualPathAwareUrlResolver).AssemblyQualifiedName
            }
         );

         resolvers.Add(
             new ResolverElement {
                Scheme = XmlEmbeddedResourceResolver.UriSchemeClires,
                Type = typeof(XmlEmbeddedResourceResolver).AssemblyQualifiedName
             }
         );

         NamespaceCollection pagesNamespaces = this.Web.Pages.Namespaces;

         pagesNamespaces.Add(new NamespaceInfo("System"));
         pagesNamespaces.Add(new NamespaceInfo("System.Collections.Generic"));
         pagesNamespaces.Add(new NamespaceInfo("System.IO"));
         pagesNamespaces.Add(new NamespaceInfo("System.Linq"));
         pagesNamespaces.Add(new NamespaceInfo("System.Net"));
         pagesNamespaces.Add(new NamespaceInfo("System.Web"));
         pagesNamespaces.Add(new NamespaceInfo("System.Web.Security"));
         pagesNamespaces.Add(new NamespaceInfo("System.Web.UI"));

         base.InitializeDefault();
      }
   }
}
