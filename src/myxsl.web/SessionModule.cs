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
using System.Web;

namespace myxsl.web {

   [XPathModule(Prefix, Namespace, Predeclare = true)]
   public static class SessionModule {

      internal const string Prefix = "session";
      internal const string Namespace = XPathModuleAttribute.BuiltInModulesBaseNamespace + "web/session";

      static HttpContext Context {
         get { return HttpContext.Current; }
      }

      [XPathFunction("get", "xs:string", As = "item()?")]
      public static object Get(string name) {
         return Context.Session[name];
      }

      /// <summary>
      /// This member supports the myxsl infrastructure and is not intended to be used directly from your code.
      /// </summary>
      public static object GetAndRemove(string name) {

         object val = Get(name);
         Remove(name);

         return val;
      }

      [XPathFunction("set", "xs:string", "item()", As = "empty-sequence()")]
      public static void Set(string name, object value) {
         Context.Session[name] = value;
      }

      [XPathFunction("remove", "xs:string", As = "empty-sequence()")]
      public static void Remove(string name) {
         Context.Session.Remove(name);
      }

      [XPathFunction("remove-all", As = "empty-sequence()")]
      public static void RemoveAll() {
         Context.Session.RemoveAll();
      }

      [XPathFunction("timeout", As = "xs:integer")]
      public static int Timeout() {
         return Context.Session.Timeout;
      }
   }
}
