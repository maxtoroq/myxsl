// Copyright 2013 Max Toro Q.
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
using System.Xml.Linq;
using System.Xml.XPath;

namespace myxsl.net {

   sealed class XPathNavigatorEqualityComparer : IEqualityComparer<XPathNavigator> {

      public static readonly XPathNavigatorEqualityComparer Instance = new XPathNavigatorEqualityComparer();

      private XPathNavigatorEqualityComparer() { }

      public bool Equals(XPathNavigator x, XPathNavigator y) {

         if (x == null) {
            return y == null;
         }

         if (y == null) {
            return false;
         }

         return Object.ReferenceEquals(x, y)
            || GetHashCode(x) == GetHashCode(y);
      }

      public int GetHashCode(XPathNavigator obj) {

         if (obj == null) {
            return 0;
         }

         XNode xnode = obj.UnderlyingObject as XNode;

         if (xnode == null) {
            
            using (var reader = obj.ReadSubtree()) {
               reader.Read();
               xnode = XNode.ReadFrom(reader);
            }
         }

         return XNode.EqualityComparer.GetHashCode(xnode);
      }
   }
}
