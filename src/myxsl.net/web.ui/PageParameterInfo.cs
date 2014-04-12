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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using myxsl.net.web.compilation;

namespace myxsl.net.web.ui {
   
   public class PageParameterInfo {

      static readonly Regex OcurrenceIndicatorRegex = new Regex(@"[\*\+\?]$");

      public string Name { get; private set; }
      public XmlQualifiedName AtomicTypeName { get; set; }
      public int MinLength { get; set; }
      public int MaxLength { get; set; }

      public BindingExpressionInfo Binding { get; set; }

      public PageParameterInfo(string name) {
         
         if (name == null) throw new ArgumentNullException("name");
         if (name.Length == 0) throw new ArgumentException("name cannot be empty.", "name");
         
         XmlConvert.VerifyNCName(name);

         this.Name = name;
         this.AtomicTypeName = XmlQualifiedName.Empty;
         this.MinLength = 0;
         this.MaxLength = -1;
      }

      public static PageParameterInfo FromSequenceType(string name, string sequenceType, IDictionary<string, string> namespacesInScope) {

         var param = new PageParameterInfo(name);

         if (!String.IsNullOrEmpty(sequenceType)) {

            if (sequenceType.Length < 3) {
               throw new ArgumentException("Unrecognized SequenceType.", "sequenceType");
            }

            string occurrenceIndicator = OcurrenceIndicatorRegex.Match(sequenceType).Value;
            
            string itemType = String.IsNullOrEmpty(occurrenceIndicator) ? sequenceType 
               : sequenceType.Substring(0, sequenceType.Length - 1);

            // ensure non ParenthesizedItemType
            if (itemType[0] == '(') {
               itemType = itemType.Substring(1, itemType.Length - 1);
            }
            
            switch (occurrenceIndicator) {
               default:
                  param.MinLength = 1;
                  param.MaxLength = 1;
                  break;
               
               case "?":
                  param.MinLength = 0;
                  param.MaxLength = 1;
                  break;

               case "*":
                  param.MinLength = 0;
                  param.MaxLength = -1;
                  break;

               case "+":
                  param.MinLength = 1;
                  param.MaxLength = -1;
                  break;
            }

            if (itemType.Contains("(")) {
               // not AtomicType

               if (itemType == "empty-sequence()") {
                  param.MinLength = param.MaxLength = 0;
               }

            } else if (itemType.Contains(":")) {
               string[] parts = itemType.Split(':');

               string atomicTypePrefix = parts[0];
               string atomicTypeLocal = parts[1];

               if (namespacesInScope == null) {
                  throw new ArgumentNullException("namespacesInScope", "namespacesInScope is needed to resolve the ItemType.");
               }

               if (!namespacesInScope.ContainsKey(atomicTypePrefix)) {
                  throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "namespacesInScope does not contain a mapping for prefix '{0}'.", atomicTypePrefix), "namespacesInScope");
               }

               string atomicTypeNamespace = namespacesInScope[atomicTypePrefix];

               param.AtomicTypeName = new XmlQualifiedName(atomicTypeLocal, atomicTypeNamespace);

            } else {
               throw new ArgumentException("Unrecognized ItemType.", "sequenceType");
            }
         }

         return param;
      }
   }

   public class PageParameterInfoCollection : KeyedCollection<string, PageParameterInfo> {

      protected override string GetKeyForItem(PageParameterInfo item) {
         return item.Name;
      }
   }
}
