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
using System.Web;
using System.Xml;
using myxsl.net.common;

namespace myxsl.net.web.ui {

   public abstract class XQueryPage : BasePage {

      public abstract XQueryExecutable Executable { get; }

      public override void Render(TextWriter writer) {
         Render(writer, new XQueryRuntimeOptions());
      }

      public void Render(TextWriter writer, XQueryRuntimeOptions options) {

         try {
            this.Executable.Run(writer, options);

         } catch (ProcessorException ex) {
            throw CreateRenderException(ex);
         }
      }
   }
}
