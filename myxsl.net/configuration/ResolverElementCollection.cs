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
using System.Configuration;

namespace myxsl.net.configuration {

   sealed class ResolverElementCollection : ConfigurationElementCollection {

      protected override ConfigurationElement CreateNewElement() {
         return new ResolverElement();
      }

      protected override object GetElementKey(ConfigurationElement element) {
         return ((ResolverElement)element).Scheme;
      }

      public ResolverElement Get(string scheme) {
         return (ResolverElement)base.BaseGet(scheme);
      }

      internal void Add(ResolverElement element) {
         base.BaseAdd(element, true);
      }
   }
}
