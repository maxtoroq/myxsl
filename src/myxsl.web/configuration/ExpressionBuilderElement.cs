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
using myxsl.web.compilation;
using myxsl.configuration;
using myxsl.common;

namespace myxsl.web.configuration {

   sealed class ExpressionBuilderElement : ConfigurationElement {
      
      static readonly ConfigurationPropertyCollection _Properties;
      static readonly ConfigurationProperty _NamespaceProperty;
      static readonly ConfigurationProperty _TypeProperty;

      Type _TypeInternal;

      protected override ConfigurationPropertyCollection Properties {
         get { return _Properties; }
      }

      public string Namespace {
         get { return (string)this[_NamespaceProperty]; }
         private set { this[_NamespaceProperty] = value; }
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
                     _TypeInternal = TypeLoader.Instance.LoadType(Type, typeof(BindingExpressionBuilder), this, "type");
                  }
               }
            }
            return _TypeInternal;
         }
         private set {

            if (value == null) throw new ArgumentNullException("value");

            _TypeInternal = value;
            Type = value.AssemblyQualifiedName;
         }
      }

      static ExpressionBuilderElement() {

         _NamespaceProperty = new ConfigurationProperty("namespace", typeof(String), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
         _TypeProperty = new ConfigurationProperty("type", typeof(String), null, ConfigurationPropertyOptions.IsRequired);

         _Properties = new ConfigurationPropertyCollection { 
            _NamespaceProperty, _TypeProperty
         };
      }

      public ExpressionBuilderElement() { }

      internal ExpressionBuilderElement(string @namespace, Type type) {

         if (@namespace == null) throw new ArgumentNullException("namespace");
         if (type == null) throw new ArgumentNullException("type");

         this.Namespace = @namespace;
         this.TypeInternal = type;
      }
   }
}
