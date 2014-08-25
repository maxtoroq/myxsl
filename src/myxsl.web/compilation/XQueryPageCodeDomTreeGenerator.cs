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
using myxsl.web.ui;
using System.CodeDom;
using System.IO;
using System.Xml;
using System.Web.Hosting;
using System.Web;
using myxsl.common;

namespace myxsl.web.compilation {
   
   public class XQueryPageCodeDomTreeGenerator : BasePageCodeDomTreeGenerator {

      XQueryPageParser parser;
      Type _PageBaseClass;
      CodeMemberField executableField;

      protected override Type PageBaseClass {
         get { return _PageBaseClass; }
      }

      public XQueryPageCodeDomTreeGenerator(XQueryPageParser parser)
         : base(parser) {

         this.parser = parser;
         _PageBaseClass = typeof(XQueryPage);
      }

      protected override void AddPageFields(CodeTypeMemberCollection members) {
         base.AddPageFields(members);

         executableField = new CodeMemberField {
            Name = "_Executable",
            Type = new CodeTypeReference(typeof(XQueryExecutable)),
            Attributes = MemberAttributes.Private | MemberAttributes.Static
         };

         members.Add(executableField);
      }

      protected override void AddPageTypeCtorStatements(CodeStatementCollection statements) {
         
         base.AddPageTypeCtorStatements(statements);

         var uriType = new CodeTypeReference(typeof(Uri));

         var procVar = new CodeVariableDeclarationStatement {
            Name = "proc",
            Type = new CodeTypeReference(typeof(IXQueryProcessor)),
            InitExpression = new CodeIndexerExpression {
               TargetObject = new CodePropertyReferenceExpression {
                  PropertyName = "XQuery",
                  TargetObject = new CodeTypeReferenceExpression(typeof(Processors))
               },
               Indices = { 
                  new CodePrimitiveExpression(parser.ProcessorName)
               }
            }
         };

         var sourceVar = new CodeVariableDeclarationStatement {
            Name = "source",
            Type = new CodeTypeReference(typeof(Stream)),
            InitExpression = new CodePrimitiveExpression(null)
         };

         var virtualPathVar = new CodeVariableDeclarationStatement {
            Name = "virtualPath",
            Type = new CodeTypeReference(typeof(String)),
            InitExpression = new CodePrimitiveExpression(this.parser.AppRelativeVirtualPath)
         };

         var sourceUriVar = new CodeVariableDeclarationStatement {
            Name = "sourceUri",
            Type = uriType,
            InitExpression = new CodeObjectCreateExpression {
               CreateType = uriType,
               Parameters = { 
                  new CodeMethodInvokeExpression {
                     Method = new CodeMethodReferenceExpression {
                        MethodName = "MapPath",
                        TargetObject = new CodeTypeReferenceExpression(typeof(HostingEnvironment))
                     },
                     Parameters = {
                        new CodeVariableReferenceExpression(virtualPathVar.Name)
                     }
                  },
                  new CodePropertyReferenceExpression {
                     PropertyName = "Absolute",
                     TargetObject = new CodeTypeReferenceExpression(typeof(UriKind))
                  }
               }
            }
         };
         
         var trySt = new CodeTryCatchFinallyStatement();
         trySt.TryStatements.Add(new CodeAssignStatement {
            Left = new CodeVariableReferenceExpression(sourceVar.Name),
            Right = new CodeCastExpression {
               TargetType = new CodeTypeReference(typeof(Stream)),
               Expression = new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "OpenRead",
                     TargetObject = new CodeTypeReferenceExpression(typeof(File))
                  },
                  Parameters = { 
                     new CodePropertyReferenceExpression {
                        PropertyName = "LocalPath",
                        TargetObject = new CodeVariableReferenceExpression(sourceUriVar.Name)
                     }
                  }
               }
            }
         });

         var optionsVar = new CodeVariableDeclarationStatement {
            Name = "options",
            Type = new CodeTypeReference(typeof(XQueryCompileOptions)),
         };
         optionsVar.InitExpression = new CodeObjectCreateExpression(optionsVar.Type);

         trySt.TryStatements.Add(optionsVar);
         trySt.TryStatements.Add(new CodeAssignStatement {
            Left = new CodePropertyReferenceExpression {
               PropertyName = "BaseUri",
               TargetObject = new CodeVariableReferenceExpression(optionsVar.Name)
            },
            Right = new CodeVariableReferenceExpression(sourceUriVar.Name)
         });
         trySt.TryStatements.Add(new CodeAssignStatement {
            Left = new CodeFieldReferenceExpression {
               FieldName = executableField.Name,
               TargetObject = PageTypeReferenceExpression
            },
            Right = new CodeMethodInvokeExpression { 
               Method = new CodeMethodReferenceExpression {
                  MethodName = "Compile",
                  TargetObject = new CodeVariableReferenceExpression(procVar.Name)
               },
               Parameters = {
                  new CodeVariableReferenceExpression(sourceVar.Name),
                  new CodeVariableReferenceExpression(optionsVar.Name)
               }
            }
         });

         var disposeIf = new CodeConditionStatement {
            Condition = new CodeBinaryOperatorExpression {
               Left = new CodeVariableReferenceExpression(sourceVar.Name),
               Operator = CodeBinaryOperatorType.IdentityInequality,
               Right = new CodePrimitiveExpression(null)
            }
         };
         disposeIf.TrueStatements.Add(new CodeMethodInvokeExpression(
            new CodeMethodReferenceExpression {
               MethodName = "Dispose",
               TargetObject = new CodeVariableReferenceExpression(sourceVar.Name)
            }
         ));

         trySt.FinallyStatements.Add(disposeIf);

         statements.AddRange(new CodeStatement[] { procVar, sourceVar, virtualPathVar, sourceUriVar, trySt });
      }

      protected override void AddPageProperties(CodeTypeMemberCollection members) {
         base.AddPageProperties(members);

         var executableProperty = new CodeMemberProperty {
            Name = "Executable",
            Type = executableField.Type,
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            HasSet = false,
            HasGet = true,
         };
         executableProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(PageTypeReferenceExpression, executableField.Name)));

         members.Add(executableProperty);
      }
   }
}
