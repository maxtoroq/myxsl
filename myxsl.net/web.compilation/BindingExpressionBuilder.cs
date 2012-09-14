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
using System.CodeDom;
using System.Globalization;
using System.Linq;
using myxsl.net.configuration;
using myxsl.net.configuration.web;

namespace myxsl.net.web.compilation {
   
   public abstract class BindingExpressionBuilder {

      public static BindingExpressionInfo ParseExpr(string expression, BindingExpressionContext context) {

         if (expression == null) throw new ArgumentNullException("expression");
         if (context == null) throw new ArgumentNullException("context");

         string ns = context.Namespace ?? context.BoundNode.NamespaceURI;
         
         BindingExpressionBuilder exprBuilder;

         ExpressionBuilderElement el = LibraryConfigSection.Instance.Web.Compilation.ExpressionBuilders.Get(ns);

         if (el == null)
            throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "There are no expression builders registered for namespace '{0}'.", ns));

         exprBuilder = (BindingExpressionBuilder)Activator.CreateInstance(el.TypeInternal);

         BindingExpressionInfo exprInfo = exprBuilder.ParseExpression(expression, context);

         if (exprInfo == null)
            exprInfo = new BindingExpressionInfo(expression);

         exprInfo.ExpressionBuilder = exprBuilder;

         return exprInfo;
      }

      public static string[] GetNamespaces() {
         return LibraryConfigSection.Instance.Web.Compilation.ExpressionBuilders.Cast<ExpressionBuilderElement>().Select(e => e.Namespace).ToArray();
      }

      public virtual BindingExpressionInfo ParseExpression(string expression, BindingExpressionContext context) {
         return null;
      }

      public abstract CodeExpression GetCodeExpression(BindingExpressionInfo exprInfo);
   }
}
