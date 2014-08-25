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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using myxsl.web.compilation;

namespace myxsl.web.configuration {
   
   sealed class WebSection : ConfigurationSection {

      internal static readonly string SectionName = "myxsl.web";

      static readonly ConfigurationPropertyCollection _Properties;
      static readonly ConfigurationProperty _CompilationProperty;
      static readonly ConfigurationProperty _PagesProperty;

      static WebSection _Instance;
      static readonly object padlock = new object();

      CompilationElement _Compilation;
      PagesElement _Pages;

      internal static WebSection Instance {
         get {

            if (_Instance == null) {
               lock (padlock) {
                  if (_Instance == null) {

                     WebSection configSection = ConfigurationManager.GetSection(SectionName) as WebSection;

                     if (configSection == null) {
                        configSection = new WebSection();
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

      public CompilationElement Compilation {
         get {
            return _Compilation
               ?? (_Compilation = (CompilationElement)base[_CompilationProperty]);
         }
      }

      public PagesElement Pages {
         get {
            return _Pages
               ?? (_Pages = (PagesElement)base[_PagesProperty]);
         }
      }

      static WebSection() {

         _CompilationProperty = new ConfigurationProperty("compilation", typeof(CompilationElement));
         _PagesProperty = new ConfigurationProperty("pages", typeof(PagesElement));

         _Properties = new ConfigurationPropertyCollection { 
            _CompilationProperty,
            _PagesProperty
         };
      }

      private WebSection() { }

      protected override void InitializeDefault() {

         ExpressionBuilderElementCollection exprBuilders = this.Compilation.ExpressionBuilders;

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

         NamespaceCollection pagesNamespaces = this.Pages.Namespaces;

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
