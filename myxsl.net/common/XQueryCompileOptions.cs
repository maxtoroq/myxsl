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
using System.Xml;
using System.Reflection;

namespace myxsl.net.common {

   public class XQueryCompileOptions {

      Assembly callingAssembly = Assembly.GetCallingAssembly();
      XmlResolver _XmlResolver;

      public Uri BaseUri { get; set; }

      public XmlResolver XmlResolver {
         get {
            if (_XmlResolver == null
               && callingAssembly != null) {
               XmlResolver = new XmlDynamicResolver(callingAssembly);
            }
            return _XmlResolver;
         }
         set {
            _XmlResolver = value;
            callingAssembly = null;
         }
      }
   }
}
