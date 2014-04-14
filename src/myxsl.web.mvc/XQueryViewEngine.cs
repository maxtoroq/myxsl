// Copyright 2011 Max Toro Q.
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
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace myxsl.web.mvc {
   
   public class XQueryViewEngine : BuildManagerViewEngine {

      public XQueryViewEngine() {

         base.ViewLocationFormats = new[] { "~/Views/{1}/{0}.xqy", "~/Views/Shared/{0}.xqy" };
         base.PartialViewLocationFormats = base.ViewLocationFormats;
      }

      protected override IView CreatePartialView(ControllerContext controllerContext, string partialPath) {
         return new XQueryView(controllerContext, partialPath);
      }

      protected override IView CreateView(ControllerContext controllerContext, string viewPath, string masterPath) {
         return new XQueryView(controllerContext, viewPath);
      }
   }
}
