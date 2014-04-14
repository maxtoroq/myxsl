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
using System.Linq;
using System.Web.Configuration;
using System.Web.UI;
using myxsl.configuration;
using myxsl.configuration.web;

namespace myxsl.web.ui {
   
   public abstract class BasePageParser : BaseParser {

      PagesElement config;

      PagesEnableSessionState? _EnableSessionState;
      bool? _ValidateRequest;

      PageParameterInfoCollection _Parameters;
      IList<string> _AcceptVerbs;
      IList<string> _SourceDependencies;
      IList<ParsedValue<string>> _Namespaces;

      public string ContentType { get; set; }
      public IDictionary<string, object> OutputCache { get; set; }

      public PagesEnableSessionState EnableSessionState {
         get {
            if (_EnableSessionState == null) {
               EnsureConfig();
               _EnableSessionState = config.EnableSessionState;
            }
            return _EnableSessionState.Value;
         }
         set { _EnableSessionState = value; }
      }

      public bool ValidateRequest {
         get {
            if (_ValidateRequest == null) {
               EnsureConfig();
               _ValidateRequest = config.ValidateRequest;
            }
            return _ValidateRequest.Value;
         }
         set { _ValidateRequest = value; }
      }

      public PageParameterInfoCollection Parameters {
         get {
            return _Parameters
               ?? (_Parameters = new PageParameterInfoCollection());
         }
      }

      public IList<string> AcceptVerbs { 
         get { 
            return _AcceptVerbs
               ?? (_AcceptVerbs = new List<string>());
         } 
      }

      public IList<string> SourceDependencies {
         get {
            if (_SourceDependencies == null) {
               
               if (String.IsNullOrEmpty(this.VirtualPath)) {
                  throw new InvalidOperationException("VirtualPath cannot be null or empty.");
               }
               _SourceDependencies = new List<string> { this.VirtualPath };
            }
            return _SourceDependencies;
         }
      }

      public IList<ParsedValue<string>> Namespaces {
         get {
            if (_Namespaces == null) {
               _Namespaces = new List<ParsedValue<string>>();

               EnsureConfig();

               foreach (ParsedValue<string> item in config.Namespaces.Cast<NamespaceInfo>().Select(n => new ParsedValue<string>(n.Namespace, n.ElementInformation.Source, n.ElementInformation.LineNumber))) {
                  _Namespaces.Add(item);
               }
            }
            return _Namespaces;
         }
      }

      void EnsureConfig() {

         if (this.config != null) {
            return;
         }

         var localConfig = (LibraryConfigSection)WebConfigurationManager.GetSection(LibraryConfigSection.SectionName, this.AppRelativeVirtualPath);

         this.config = (localConfig != null) ? localConfig.Web.Pages
            : LibraryConfigSection.Instance.Web.Pages;
      }
   }
}
