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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Compilation;
using myxsl.web.ui;

namespace myxsl.web.compilation {

   [BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
   public abstract class BasePageBuildProvider : BaseBuildProvider {

      ICollection _VirtualPathDependencies;

      public override ICollection VirtualPathDependencies {
         get {
            return _VirtualPathDependencies
               ?? (_VirtualPathDependencies = ((BasePageParser)base.Parser).SourceDependencies as ICollection
                  ?? base.VirtualPathDependencies);
         }
      }

      public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
         
         base.GenerateCode(assemblyBuilder);
         assemblyBuilder.GenerateTypeFactory(this.GeneratedTypeFullName);
      }
   }
}
