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

namespace myxsl.net.configuration.web {
   
   sealed class CompilationElement : ConfigurationElement {

      static readonly ConfigurationPropertyCollection _Properties;
      static readonly ConfigurationProperty _ExpressionBuildersProperty;

      ExpressionBuilderElementCollection _ExpressionBuilders;

      protected override ConfigurationPropertyCollection Properties {
         get { return _Properties; }
      }

      public ExpressionBuilderElementCollection ExpressionBuilders {
         get {
            if (_ExpressionBuilders == null)
               _ExpressionBuilders = (ExpressionBuilderElementCollection)base[_ExpressionBuildersProperty];
            return _ExpressionBuilders;
         }
      }

      static CompilationElement() {
         
         _ExpressionBuildersProperty = new ConfigurationProperty("expressionBuilders", typeof(ExpressionBuilderElementCollection));

         _Properties = new ConfigurationPropertyCollection { 
            _ExpressionBuildersProperty,
         };
      }
   }
}
