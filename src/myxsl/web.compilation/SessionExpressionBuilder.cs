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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.XPath;
using myxsl.web.ui;

namespace myxsl.web.compilation {
   
   public sealed class SessionExpressionBuilder : BindingExpressionBuilder {

      internal const string Namespace = SessionModule.Namespace;

      static readonly CodeTypeReference SessionModuleTypeReference;

      static SessionExpressionBuilder() {
         SessionModuleTypeReference = new CodeTypeReference(typeof(SessionModule));
      }

      public override BindingExpressionInfo ParseExpression(string expression, BindingExpressionContext context) {

         var uri = new Uri(expression, UriKind.RelativeOrAbsolute);

         if (!uri.IsAbsoluteUri) {
            uri = new Uri(String.Concat(SessionModule.Prefix, ":", uri.OriginalString), UriKind.Absolute);
         }

         var validValues = new List<string>() { bind.it };

         string path = uri.AbsolutePath;
         string nodeName = context.NodeName ?? context.BoundNode.Name;

         if (!validValues.Contains(path))
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The value of the '{0}' attribute must be one of these values: {1}.", nodeName, String.Join(", ", validValues.ToArray())));

         if (context.AffectsXsltInitiation) {
            throw new ArgumentException("Cannot bind to session when the parameter affects XSLT initiation.");
         }

         BasePageParser pageParser = context.Parser as BasePageParser;

         if (path == bind.it && pageParser != null && pageParser.EnableSessionState == PagesEnableSessionState.False) 
            throw new ArgumentException(
               String.Format(CultureInfo.InvariantCulture, "Cannot bind to {0} because session state is disabled for this page. Try setting enable-session-state=\"true\" on the page processing instruction.", bind.it)
            );

         NameValueCollection query = (uri.Query.Length > 1) ?
            HttpUtility.ParseQueryString(uri.Query.Replace(';', '&')) :
            new NameValueCollection();

         var exprInfo = new BindingExpressionInfo(expression) { 
            ParsedObject = uri
         };

         if (query["name"] != null) {

            exprInfo.ParsedValues["name"] = query["name"];
            query.Remove("name");

         } else {

            XPathNavigator nav = context.BoundNode.Clone();
            nav.MoveToParent();

            if (nav.NodeType == XPathNodeType.Element 
               && nav.LocalName == "param" 
               && nav.NamespaceURI == WellKnownNamespaces.XSLT) {

               exprInfo.ParsedValues["name"] = nav.GetAttribute("name", "");
            }
         }

         switch (path) {
            case bind.it:
               exprInfo.ParsedValues["remove"] = GetBooleanOrDefault(query["remove"]);
               query.Remove("remove");
               break;

            default:
               if (query["remove"] != null) {
                  throw new ArgumentException(
                     String.Format(CultureInfo.InvariantCulture, "The remove option is not valid for {0}.", path)
                  );
               }
               break;
         }

         foreach (string key in query.AllKeys) {
            exprInfo.ParsedValues.Add(key, query[key]);
         }

         return exprInfo;
      }

      public override CodeExpression GetCodeExpression(BindingExpressionInfo exprInfo) {

         IDictionary<string, object> options = exprInfo.ParsedValues;
         Uri uri = (Uri)exprInfo.ParsedObject;

         string inputName = options.ContainsKey("name") ? 
            options["name"].ToString() 
            : null;

         string path = uri.AbsolutePath;

         switch (path) {
            case bind.it: 
               return GetSessionExpression(inputName, (bool)options["remove"]);
            
            default:
               throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Invalid path '{0}'.", path), "exprInfo");
         }
      }

      CodeExpression GetSessionExpression(string name, bool remove) {

         string method = (remove) ? "GetAndRemove" : "Get";

         return new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = method,
               TargetObject = new CodeTypeReferenceExpression(SessionModuleTypeReference)
            },
            Parameters = { 
               new CodePrimitiveExpression(name)
            }
         };
      }

      bool GetBooleanOrDefault(string value) {
         return (value != null) ? XmlConvert.ToBoolean(value) : false;
      }

      struct bind {
         public const string it = "";
      }
   }
}
