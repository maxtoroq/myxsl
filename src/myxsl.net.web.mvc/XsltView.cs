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
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.XPath;
using myxsl.net.common;
using myxsl.net.web.ui;

namespace myxsl.net.web.mvc {

   public class XsltView : BuildManagerCompiledView {

      public XsltView(ControllerContext controllerContext, string viewPath) 
         : base(controllerContext, viewPath) { }

      protected override void RenderView(ViewContext viewContext, TextWriter writer, object instance) {

         if (viewContext == null) throw new ArgumentNullException("viewContext");

         XsltPage page = instance as XsltPage;
         
         if (page == null)
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Compiled view '{0}' must inherit from {1}.", this.ViewPath, typeof(XsltPage).AssemblyQualifiedName));

         var options = new XsltRuntimeOptions();

         page.SetIntrinsics(HttpContext.Current);
         page.AddFileDependencies();
         page.InitializeRuntimeOptions(options);

         IXPathNavigable inputNode = page.Executable.Processor.ItemFactory.CreateDocument(viewContext.ViewData.Model);

         if (inputNode != null) {
            if (options.InitialContextNode == null)
               options.InitialTemplate = null;

            options.InitialContextNode = inputNode;
         }

         foreach (var item in viewContext.ViewData)
            options.Parameters[new XmlQualifiedName(item.Key)] = item.Value;

         page.Render(viewContext.Writer, options);
      }
   }
}
