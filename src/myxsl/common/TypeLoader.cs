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
using System.Reflection;

namespace myxsl.common {
   
   class TypeLoader {

      static TypeLoader _Instance = new TypeLoader();

      public static TypeLoader Instance {
         get { return _Instance; }
         set {

            if (value == null) throw new ArgumentNullException("value");

            _Instance = value;
         }
      }

      public virtual Type GetType(string typeName, bool throwOnError, bool ignoreCase) {
         return Type.GetType(typeName, throwOnError, ignoreCase);
      }

      public virtual IEnumerable<Assembly> GetReferencedAssemblies() {
         return AppDomain.CurrentDomain.GetAssemblies();
      }
   }
}
