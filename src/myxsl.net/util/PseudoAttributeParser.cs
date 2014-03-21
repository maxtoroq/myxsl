// Copyright 2012 Max Toro Q.
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace myxsl.net {
   
   static class PseudoAttributeParser {

      static readonly Regex AttributeParser = new Regex(@"[^\s]+[\s]*=[\s]*""[^""]*""|[^\s]+[\s]*=[\s]*'[^']*'");

      public static IDictionary<string, string> GetAttributes(string content) {

         return (from m in AttributeParser.Matches(content).Cast<Match>()
                 let eqIndex = m.Value.IndexOf('=')
                 let name = m.Value.Substring(0, eqIndex).Trim()
                 let quotedValue = m.Value.Substring(eqIndex + 1).Trim()
                 let encodedValue = quotedValue.Substring(1, quotedValue.Length - 2)
                 let reader = XmlReader.Create(new StringReader(encodedValue), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment })
                 let read = reader.Read()
                 let value = reader.Value
                 select new { name, value }).ToDictionary(m => m.name, m => m.value);
      }
   }
}
