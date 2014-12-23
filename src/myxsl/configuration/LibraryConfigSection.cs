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
using System.Configuration;
using System.Xml;

namespace myxsl.configuration {

   sealed class LibraryConfigSection : ConfigurationSection {

      internal static readonly string SectionName = "myxsl";

      static readonly ConfigurationPropertyCollection _Properties;
      static readonly ConfigurationProperty _ProcessorsProperty;
      static readonly ConfigurationProperty _ResolversProperty;
      static readonly ConfigurationProperty _XsltProperty;
      static readonly ConfigurationProperty _XQueryProperty;

      static LibraryConfigSection _Instance;
      static readonly object padlock = new object();

      ProcessorElementCollection _Processors;
      ResolverElementCollection _Resolvers;
      XsltElement _Xslt;
      XQueryElement _XQuery;

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
            return _Processors
               ?? (_Processors = (ProcessorElementCollection)base[_ProcessorsProperty]);
         }
      }

      public ResolverElementCollection Resolvers {
         get {
            return _Resolvers
               ?? (_Resolvers = (ResolverElementCollection)base[_ResolversProperty]);
         }
      }

      public XsltElement Xslt {
         get {
            return _Xslt
               ?? (_Xslt = (XsltElement)base[_XsltProperty]);
         }
      }

      public XQueryElement XQuery {
         get {
            return _XQuery
               ?? (_XQuery = (XQueryElement)base[_XQueryProperty]);
         }
      }

      static LibraryConfigSection() {
         
         _ProcessorsProperty = new ConfigurationProperty("processors", typeof(ProcessorElementCollection));
         _ResolversProperty = new ConfigurationProperty("resolvers", typeof(ResolverElementCollection));
         _XsltProperty = new ConfigurationProperty("xslt", typeof(XsltElement));
         _XQueryProperty = new ConfigurationProperty("xquery", typeof(XQueryElement));

         _Properties = new ConfigurationPropertyCollection { 
            _ProcessorsProperty, 
            _ResolversProperty, 
            _XsltProperty,
            _XQueryProperty
         };
      }

      private LibraryConfigSection() { }

      protected override void InitializeDefault() {
         
         // cannot use System.Web.Compilation.BuildManager during pre-start initialization phase
         // therefore, not using TypeLoader.Instance

         Type sysProcType = Type.GetType("myxsl.xml.xsl.SystemXsltProcessor, myxsl.xml.xsl", throwOnError: false, ignoreCase: false);

         if (sysProcType != null) {

            var sysProc = new ProcessorElement("system", sysProcType) {
               LockItem = true
            };

            this.Processors.Add(sysProc);
            this.Xslt.DefaultProcessor = sysProc.Name; 
         }

         ResolverElementCollection resolvers = this.Resolvers;

         resolvers.Add(new ResolverElement(Uri.UriSchemeFile, typeof(XmlUrlResolver)));
         resolvers.Add(new ResolverElement(Uri.UriSchemeHttp, typeof(XmlUrlResolver)));
         resolvers.Add(new ResolverElement(XmlEmbeddedResourceResolver.UriSchemeClires, typeof(XmlEmbeddedResourceResolver)));

         base.InitializeDefault();
      }
   }
}
