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
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using PIAttribs = System.Collections.Generic.IDictionary<string, string>;
using myxsl.net.configuration;
using myxsl.net.common;

namespace myxsl.net.web.ui {
   
   public class SchematronParser : BaseParser {
      
      public string ProcessorName { get; set; }

      protected XPathNavigator Navigator { get; private set; }

      public override void Parse(TextReader source) {
         
         if (Processors.Xslt.Count == 0)
            throw CreateParseException("There are no XSLT processors registered to compile this schema.");

         this.ProcessorName = Processors.Xslt.DefaultProcessorName;

         this.Navigator = new XPathDocument(source).CreateNavigator();

         XPathNavigator nav = this.Navigator;

         bool validatorPiFound = false;

         for (bool moved = nav.MoveToFirstChild(); moved; moved = nav.MoveToNext()) {
            if (nav.NodeType == XPathNodeType.ProcessingInstruction) {

               switch (nav.LocalName) {
                  case validator.it:
                     if (validatorPiFound) goto non_unique;
                     GetValidatorSettings(nav);
                     validatorPiFound = true;
                     break;
               }

               continue;

            non_unique:
               throw CreateParseException("There can be only one '{0}' processing instruction.", nav.LocalName);
            }
         }

         if (this.ProcessorName == null)
            throw CreateParseException("Please specify a default XSLT processor.");
      }

      protected void GetValidatorSettings(XPathNavigator nav) {

         PIAttribs attribs = GetAttributes(nav.Value);

         // class-name
         string className = GetFullClassNameAttribute(attribs, validator.class_name);

         if (!String.IsNullOrEmpty(className))
            this.GeneratedTypeFullName = className;

         // processor
         string processor = GetNonEmptyAttribute(attribs, validator.processor);

         if (processor != null) {

            if (!Processors.Xslt.Exists(processor))
               throw CreateParseException("The processor '{0}' is not registered.", processor);

            this.ProcessorName = processor;
         }
      }

      protected override Exception CreateParseException(string format, params object[] args) {
         return CreateParseException(Navigator as IXmlLineInfo, format, args);
      }

      internal struct validator {

         public const string it = "validator";

         public const string class_name = "class-name";
         public const string processor = "processor";
      }
   }
}
