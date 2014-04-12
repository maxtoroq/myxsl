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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Saxon.Api;

namespace myxsl.net.saxon {

   class BugHandler {
      
      static readonly Version CurrentVersion = typeof(Saxon.Api.Processor).Assembly.GetName().Version;
      static readonly ConcurrentDictionary<int, Version> BugMaxVersions = new ConcurrentDictionary<int, Version>();
      
      public static void ThrowIfBug1675(XdmNode initialContextNode) {

         if (initialContextNode.Root.NodeKind != XmlNodeType.Document) {
            CurrentVersionHigherThan("9.4.0.6", 1675, () => "Cannot use an initial context node that isn't a descendant of a document node.");
         }
      }

      static bool CurrentVersionHigherThan(string version, int bug, Func<string> specificMessage) {

         Version ver = BugMaxVersions.GetOrAdd(bug, b => Version.Parse(version));

         if (CurrentVersion > ver) {
            return true;
         }

         throw new InvalidOperationException(specificMessage() + 
            ". Consider upgrading Saxon to a version higher than {0}. See https://saxonica.plan.io/issues/{1} for more details.".FormatInvariant(ver, bug));
      }
   }
}
