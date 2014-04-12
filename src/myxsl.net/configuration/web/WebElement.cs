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
   
   sealed class WebElement : ConfigurationElement {

      static readonly ConfigurationPropertyCollection _Properties;
      static readonly ConfigurationProperty _CompilationProperty;
      static readonly ConfigurationProperty _PagesProperty;

      CompilationElement _Compilation;
      PagesElement _Pages;

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

      static WebElement() {

         _CompilationProperty = new ConfigurationProperty("compilation", typeof(CompilationElement));
         _PagesProperty = new ConfigurationProperty("pages", typeof(PagesElement));

         _Properties = new ConfigurationPropertyCollection { 
            _CompilationProperty,
            _PagesProperty
         };
      }
   }
}
