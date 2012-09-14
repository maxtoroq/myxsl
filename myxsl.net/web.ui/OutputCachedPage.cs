// Copyright (c) Microsoft Corporation.  All rights reserved.
// Microsoft would like to thank its contributors, a list of whom
// are at http://aspnetwebstack.codeplex.com/wikipage?title=Contributors.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License. You may
// obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.UI;

//namespace System.Web.Mvc {
namespace myxsl.net.web.ui {

   [SuppressMessage("ASP.NET.Security", "CA5328:ValidateRequestShouldBeEnabled", Justification = "Instances of this type are not created in response to direct user input.")]
   sealed class OutputCachedPage : Page {
      
      OutputCacheParameters _cacheSettings;

      public OutputCachedPage(OutputCacheParameters cacheSettings) {
         // Tracing requires Page IDs to be unique.
         ID = Guid.NewGuid().ToString();
         _cacheSettings = cacheSettings;
      }

      protected override void FrameworkInitialize() {
         // when you put the <%@ OutputCache %> directive on a page, the generated code calls InitOutputCache() from here
         base.FrameworkInitialize();
         InitOutputCache(_cacheSettings);
      }
   }
}
