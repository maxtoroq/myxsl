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
using System.Web.Compilation;
using myxsl.net.common;
using myxsl.net.web.ui;

namespace myxsl.net.web.compilation {
   
   public class XQueryPageBuildProvider : BasePageBuildProvider {

      public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
         
         base.GenerateCode(assemblyBuilder);

         // test compilation

         XQueryPageParser xqueryParser = (XQueryPageParser)Parser;

         IXQueryProcessor proc = Processors.XQuery[xqueryParser.ProcessorName];

         using (Stream source = this.OpenStream()) {

            try {
               proc.Compile(source, new XQueryCompileOptions { BaseUri = this.PhysicalPath } );
            } catch (ProcessorException ex) {
               throw CreateCompileException(ex);
            }
         }
      }

      protected override BaseParser CreateParser() {
         return new XQueryPageParser();
      }

      protected override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(BaseParser parser) {
         return new XQueryPageCodeDomTreeGenerator((XQueryPageParser)parser);
      }
   }
}
