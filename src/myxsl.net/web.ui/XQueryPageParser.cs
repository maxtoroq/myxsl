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
using System.Web.Configuration;
using System.Web.UI;
using System.IO;
using myxsl.net.configuration;
using myxsl.net.common;

namespace myxsl.net.web.ui {
   
   public class XQueryPageParser : BasePageParser {

      public string ProcessorName { get; set; }

      public override void Parse(TextReader source) {

         if (Processors.XQuery.Count == 0) {
            throw CreateParseException("There are no XQuery processors registered to render this page.");
         }

         this.ProcessorName = Processors.XQuery.DefaultProcessorName;

         if (this.ProcessorName == null) {
            throw CreateParseException("Please specify a default XQuery processor.");
         }
      }
   }
}
