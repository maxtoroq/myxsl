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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;

namespace myxsl.net.configuration.web {
   
   sealed class PagesElement : ConfigurationElement {

      static readonly ConfigurationPropertyCollection _Properties;
      static readonly ConfigurationProperty _NamespacesProperty;
      static readonly ConfigurationProperty _ValidateRequestProperty;
      static readonly ConfigurationProperty _EnableSessionStateProperty;

      NamespaceCollection _Namespaces;

      protected override ConfigurationPropertyCollection Properties {
         get { return _Properties; }
      }

      public NamespaceCollection Namespaces {
         get {
            if (_Namespaces == null)
               _Namespaces = (NamespaceCollection)base[_NamespacesProperty];
            return _Namespaces;
         }
      }

      public bool ValidateRequest {
         get {
            return (bool)this[_ValidateRequestProperty];
         }
      }

      public PagesEnableSessionState EnableSessionState {
         get {

            switch (((string)base[_EnableSessionStateProperty])) {
               case "true":
                  return PagesEnableSessionState.True;

               case "false":
                  return PagesEnableSessionState.False;

               case "ReadOnly":
                  return PagesEnableSessionState.ReadOnly;
            }

            throw new ConfigurationErrorsException(
               "The '{0}' attribute must be one of the following values: true, false, ReadOnly."
                  .FormatInvariant(_EnableSessionStateProperty.Name),
               null,
               this.ElementInformation.Source,
               this.ElementInformation.LineNumber
            );
         }
      }

      static PagesElement() {

         _NamespacesProperty = new ConfigurationProperty("namespaces", typeof(NamespaceCollection));
         _ValidateRequestProperty = new ConfigurationProperty("validateRequest", typeof(bool), true);
         _EnableSessionStateProperty = new ConfigurationProperty("enableSessionState", typeof(string), "true");

         _Properties = new ConfigurationPropertyCollection { 
            _NamespacesProperty,
            _ValidateRequestProperty,
            _EnableSessionStateProperty
         };
      }
   }
}
