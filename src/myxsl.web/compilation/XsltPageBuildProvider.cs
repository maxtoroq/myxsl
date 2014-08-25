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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using System.Xml;
using myxsl.common;
using myxsl.web.ui;

namespace myxsl.web.compilation {
   
   public class XsltPageBuildProvider : BasePageBuildProvider {

      public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
         
         base.GenerateCode(assemblyBuilder);

         // test compilation

         XsltPageParser pageParser = (XsltPageParser)this.Parser;

         IXsltProcessor proc = Processors.Xslt[pageParser.ProcessorName];

         using (Stream source = OpenStream(pageParser.XsltVirtualPath)) {

            try {
               proc.Compile(source, new XsltCompileOptions(baseUri: pageParser.XsltPhysicalUri));
            } catch (ProcessorException ex) {
               throw CreateCompileException(ex);
            }
         }
      }

      protected override BaseParser CreateParser() {
         return new XsltPageParser();
      }

      protected override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(BaseParser parser) {
         return new XsltPageCodeDomTreeGenerator((XsltPageParser)parser);
      }
   }
}
