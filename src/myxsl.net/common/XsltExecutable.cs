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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.ComponentModel;

namespace myxsl.net.common {

   public abstract class XsltExecutable {

      [Browsable(false)]
      public IXsltProcessor Processor { get { return this.XsltProcessor; } }
      protected abstract IXsltProcessor XsltProcessor { get; }

      public abstract Uri StaticBaseUri { get; }

      public abstract void Run(Stream output, XsltRuntimeOptions options);
      public abstract void Run(TextWriter output, XsltRuntimeOptions options);
      public abstract void Run(XmlWriter output, XsltRuntimeOptions options);
      public abstract IXPathNavigable Run(XsltRuntimeOptions options);
   }
}
