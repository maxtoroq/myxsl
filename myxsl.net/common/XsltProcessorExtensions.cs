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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace myxsl.net.common {
   
   static class XsltProcessorExtensions {

      static readonly ConcurrentDictionary<IXsltProcessor, decimal> versions = new ConcurrentDictionary<IXsltProcessor, decimal>();

      public static decimal GetXsltVersion(this IXsltProcessor processor) {

         return versions.GetOrAdd(processor, p => {

            string stylesheet =
@"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
<xsl:output method='text'/>
<xsl:template match='/' name='main'>
	<xsl:value-of select=""system-property('xsl:version')""/>
</xsl:template>
</xsl:stylesheet>";

            using (var writer = new StringWriter(CultureInfo.InvariantCulture)) {

               processor
                  .Compile(new StringReader(stylesheet), new XsltCompileOptions())
                  .Run(writer, new XsltRuntimeOptions {
                     InitialTemplate = new XmlQualifiedName("main"),
                     Serialization = { 
                        Method = XPathSerializationMethods.Text
                     }
                  });

               return Decimal.Parse(writer.ToString(), CultureInfo.InvariantCulture);
	         }
         });
      }
   }
}
