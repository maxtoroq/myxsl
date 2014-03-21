// Copyright 2010 Max Toro Q.
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
using System.CodeDom;

namespace myxsl.net.web.compilation {
   
   public sealed class CodeExpressionBuilder : BindingExpressionBuilder {

      internal const string Namespace = "http://myxsl.net/ns/code";

      public override BindingExpressionInfo ParseExpression(string expression, BindingExpressionContext context) {
         return new BindingExpressionInfo(expression);
      }

      public override CodeExpression GetCodeExpression(BindingExpressionInfo exprInfo) {
         return new CodeSnippetExpression(exprInfo.Expression);
      }
   }
}
