// Copyright 2010 Max Toro Q.
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
using System.Xml;
using myxsl.common;

namespace myxsl.configuration {

   sealed class ResolverElement : ConfigurationElement {
      
      static readonly ConfigurationPropertyCollection _Properties;
      static readonly ConfigurationProperty _SchemeProperty;
      static readonly ConfigurationProperty _TypeProperty;

      Type _TypeInternal;

      protected override ConfigurationPropertyCollection Properties {
         get { return _Properties; }
      }

      public string Scheme {
         get { return (string)this[_SchemeProperty]; }
         private set { this[_SchemeProperty] = value; }
      }

      public string Type {
         get { return (string)this[_TypeProperty]; }
         private set { this[_TypeProperty] = value; }
      }

      internal Type TypeInternal {
         get {
            if (_TypeInternal == null) {
               lock (this) {
                  if (_TypeInternal == null) {
                     _TypeInternal = TypeLoader.Instance.LoadType(Type, typeof(XmlResolver), this, "type");
                  }
               }
            }
            return _TypeInternal;
         }
         set {

            if (value == null) throw new ArgumentNullException("value");

            // The configuration is read only.
            //Type = value.AssemblyQualifiedName;
            _TypeInternal = value;
         }
      }

      static ResolverElement() {

         _SchemeProperty = new ConfigurationProperty("scheme", typeof(String), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
         _TypeProperty = new ConfigurationProperty("type", typeof(String), null, ConfigurationPropertyOptions.IsRequired);

         _Properties = new ConfigurationPropertyCollection { 
            _SchemeProperty, _TypeProperty
         };
      }

      public ResolverElement() { }

      internal ResolverElement(string scheme, Type type) {

         if (scheme == null) throw new ArgumentNullException("scheme");
         if (type == null) throw new ArgumentNullException("type");

         this.Scheme = scheme;
         this.TypeInternal = type;
         this.Type = type.AssemblyQualifiedName;
      }
   }
}
