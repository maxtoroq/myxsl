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
using System.CodeDom;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.XPath;
using myxsl.common;
using myxsl.web.ui;

namespace myxsl.web.compilation {
   
   public class XsltPageCodeDomTreeGenerator : BasePageCodeDomTreeGenerator {

      readonly XsltPageParser parser;
      CodeMemberField executableField;
      CodeMemberField initialContextNodeField;

      Type _PageBaseClass;

      protected override Type PageBaseClass {
         get { return _PageBaseClass; }
      }

      public XsltPageCodeDomTreeGenerator(XsltPageParser parser)
         : base(parser) {

         this.parser = parser;
         _PageBaseClass = typeof(XsltPage);
      }

      protected override void AddPageFields(CodeTypeMemberCollection members) {
         
         base.AddPageFields(members);

         executableField = new CodeMemberField { 
            Name = "_Executable",
            Type = new CodeTypeReference(typeof(XsltExecutable)),
            Attributes = MemberAttributes.Private | MemberAttributes.Static
         };
         members.Add(executableField);

         if (parser.PageType == XsltPageType.AssociatedStylesheet) {
            
            initialContextNodeField = new CodeMemberField { 
               Name = "_initialContextNode",
               Type = new CodeTypeReference(typeof(IXPathNavigable)),
               Attributes = MemberAttributes.Private | MemberAttributes.Static
            };

            members.Add(initialContextNodeField);
         }
      }

      protected override void AddPageTypeCtorStatements(CodeStatementCollection statements) {
         
         base.AddPageTypeCtorStatements(statements);

         bool useInitialContextNode = initialContextNodeField != null;
         var uriType = new CodeTypeReference(typeof(Uri));

         var procVar = new CodeVariableDeclarationStatement {
            Name = "proc",
            Type = new CodeTypeReference(typeof(IXsltProcessor)),
            InitExpression = new CodeIndexerExpression {
               TargetObject = new CodePropertyReferenceExpression {
                  PropertyName = "Xslt",
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
            InitExpression = new CodePrimitiveExpression(VirtualPathUtility.ToAppRelative(this.parser.XsltVirtualPath))
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

         var icnSourceVar = new CodeVariableDeclarationStatement {
            Name = "initialContextNodeSource",
            Type = new CodeTypeReference(typeof(Stream)),
            InitExpression = new CodePrimitiveExpression(null)
         };

         statements.AddRange(new CodeStatement[] { procVar, sourceVar, virtualPathVar, sourceUriVar });

         if (useInitialContextNode) {
            statements.Add(icnSourceVar);
         }

         var optionsVar = new CodeVariableDeclarationStatement {
            Name = "options",
            Type = new CodeTypeReference(typeof(XsltCompileOptions)),
         };
         optionsVar.InitExpression = new CodeObjectCreateExpression(optionsVar.Type);

         var trySt = new CodeTryCatchFinallyStatement {
            TryStatements = { 
               new CodeAssignStatement {
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
               },
               optionsVar,
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     PropertyName = "BaseUri",
                     TargetObject = new CodeVariableReferenceExpression(optionsVar.Name)
                  },
                  Right = new CodeVariableReferenceExpression(sourceUriVar.Name)
               },
               new CodeAssignStatement {
                  Left = new CodeFieldReferenceExpression {
                     FieldName = executableField.Name,
                     TargetObject = PageTypeReferenceExpression
                  },
                  Right = new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression { 
                        MethodName = "Compile",
                        TargetObject = new CodeVariableReferenceExpression(procVar.Name)
                     },
                     new CodeVariableReferenceExpression(sourceVar.Name),
                     new CodeVariableReferenceExpression(optionsVar.Name)
                  )
               }
            },
            FinallyStatements = { 
               new CodeConditionStatement {
                  Condition = new CodeBinaryOperatorExpression {
                     Left = new CodeVariableReferenceExpression(sourceVar.Name),
                     Operator = CodeBinaryOperatorType.IdentityInequality,
                     Right = new CodePrimitiveExpression(null)
                  },
                  TrueStatements = {
                     new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression {
                           MethodName = "Dispose",
                           TargetObject = new CodeVariableReferenceExpression(sourceVar.Name)
                        }
                     )
                  }
               }
            }
         };

         if (useInitialContextNode) {
            
            trySt.TryStatements.Add(new CodeAssignStatement {
               Left = new CodeVariableReferenceExpression(icnSourceVar.Name),
               Right = new CodeCastExpression {
                  TargetType = new CodeTypeReference(typeof(Stream)),
                  Expression = new CodeMethodInvokeExpression {
                     Method = new CodeMethodReferenceExpression {
                        MethodName = "OpenRead",
                        TargetObject = new CodeTypeReferenceExpression(typeof(File))
                     },
                     Parameters = { 
                        new CodeMethodInvokeExpression {
                           Method = new CodeMethodReferenceExpression {
                              MethodName = "MapPath",
                              TargetObject = new CodeTypeReferenceExpression(typeof(HostingEnvironment))
                           },
                           Parameters = {
                              new CodePrimitiveExpression(this.parser.AppRelativeVirtualPath)
                           }
                        }
                     }
                  }
               }
            });

            var icnOptVar = new CodeVariableDeclarationStatement {
               Name = "initialContextNodeOptions",
               Type = new CodeTypeReference(typeof(XmlParsingOptions)),
               InitExpression = new CodeObjectCreateExpression { 
                  CreateType = new CodeTypeReference(typeof(XmlParsingOptions))
               }
            };

            trySt.TryStatements.Add(icnOptVar);

            trySt.TryStatements.Add(
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression { 
                     PropertyName = "BaseUri",
                     TargetObject = new CodeVariableReferenceExpression(icnOptVar.Name)
                  },
                  Right = new CodeVariableReferenceExpression(sourceUriVar.Name)
               }
            );

            trySt.TryStatements.Add(
               new CodeAssignStatement {
                  Left = new CodeFieldReferenceExpression(this.PageTypeReferenceExpression, initialContextNodeField.Name),
                  Right = new CodeMethodInvokeExpression {
                     Method = new CodeMethodReferenceExpression {
                        MethodName = "CreateNodeReadOnly",
                        TargetObject = new CodePropertyReferenceExpression {
                           PropertyName = "ItemFactory",
                           TargetObject = new CodeVariableReferenceExpression {
                              VariableName = procVar.Name
                           }
                        }
                     },
                     Parameters = {
                        new CodeVariableReferenceExpression(icnSourceVar.Name),
                        new CodeVariableReferenceExpression(icnOptVar.Name)
                     }
                  }
               }
            );

            trySt.FinallyStatements.Add(new CodeConditionStatement {
               Condition = new CodeBinaryOperatorExpression {
                  Left = new CodeVariableReferenceExpression(icnSourceVar.Name),
                  Operator = CodeBinaryOperatorType.IdentityInequality,
                  Right = new CodePrimitiveExpression(null)
               },
               TrueStatements = {
                  new CodeMethodInvokeExpression(
                     new CodeMethodReferenceExpression {
                        MethodName = "Dispose",
                        TargetObject = new CodeVariableReferenceExpression(icnSourceVar.Name)
                     }
                  )
               }
            });
         }

         statements.Add(trySt);
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

      protected override void AddPageMethods(CodeTypeMemberCollection members) {
         
         base.AddPageMethods(members);

         var optionsVar = new CodeVariableReferenceExpression { 
            VariableName = "options"
         };

         var optionsInit = new CodeMemberMethod {
            Name = "InitializeRuntimeOptions",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            CustomAttributes = { 
               new CodeAttributeDeclaration(this.DebuggerNonUserCodeTypeReference)
            },
            Parameters = { 
               new CodeParameterDeclarationExpression { 
                  Name = optionsVar.VariableName,
                  Type = new CodeTypeReference(typeof(XsltRuntimeOptions))
               }
            }
         };

         AddInitializeRuntimeOptionsStatements(optionsInit.Statements, optionsVar);

         if (optionsInit.Statements.Count > 0) {

            optionsInit.Statements.Insert(0,
               new CodeExpressionStatement(
                  new CodeMethodInvokeExpression {
                     Method = new CodeMethodReferenceExpression {
                        MethodName = "InitializeRuntimeOptions",
                        TargetObject = new CodeBaseReferenceExpression()
                     },
                     Parameters = { new CodeVariableReferenceExpression(optionsVar.VariableName) }
                  }
               )
            );

            members.Add(optionsInit);
         }
      }

      protected void AddInitializeRuntimeOptionsStatements(CodeStatementCollection statements, CodeVariableReferenceExpression optionsVar) {
         
         // Initial template
         //------------------------------------------------

         BindingExpressionInfo bind = parser.InitialTemplateBinding;
         XmlQualifiedName initialTempl = parser.InitialTemplate;

         if (bind != null || initialTempl != null) {

            CodeAssignStatement assignFromPrimitive = null;

            if (initialTempl != null) {

               assignFromPrimitive = new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     PropertyName = "InitialTemplate",
                     TargetObject = optionsVar
                  },
                  Right = new CodeObjectCreateExpression(
                     typeof(XmlQualifiedName),
                     new CodePrimitiveExpression(initialTempl.Name),
                     new CodePrimitiveExpression(initialTempl.Namespace)
                  )
               };
            }

            if (bind != null) {

               bool bindRequired = initialTempl == null;

               var paramName = new CodePrimitiveExpression(bind.ParsedValues.ContainsKey("name") ? bind.ParsedValues["name"] : bind.Expression);

               var initialTemplVar = new CodeVariableDeclarationStatement {
                  Name = "initialTempl",
                  Type = new CodeTypeReference(typeof(String)),
                  InitExpression = new CodeMethodInvokeExpression {
                     Method = new CodeMethodReferenceExpression {
                        MethodName = "CheckParamLength",
                        TargetObject = new CodeThisReferenceExpression()
                     },
                     Parameters = { 
                        paramName,
                        bind.GetCodeExpression(),
                        new CodePrimitiveExpression(bindRequired ? 1 : 0),
                        new CodePrimitiveExpression(1)
                     }
                  }
               };

               if (bind.LineNumber > 0) {
                  initialTemplVar.LinePragma = new CodeLinePragma(this.parser.PhysicalPath.LocalPath, bind.LineNumber);
               }

               if (bind.ParsedValues.ContainsKey("accept")) {

                  initialTemplVar.InitExpression = new CodeMethodInvokeExpression {
                     Method = new CodeMethodReferenceExpression {
                        MethodName = "CheckParamValues",
                        TargetObject = new CodeThisReferenceExpression()
                     },
                     Parameters = { 
                        paramName,
                        initialTemplVar.InitExpression,
                        new CodeArrayCreateExpression(typeof(string), ((string[])bind.ParsedValues["accept"]).Select(s => new CodePrimitiveExpression(s)).ToArray())
                     }
                  };
               }

               initialTemplVar.InitExpression = new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "AsString",
                     TargetObject = new CodeThisReferenceExpression()
                  },
                  Parameters = { 
                     initialTemplVar.InitExpression
                  }
               };

               statements.Add(initialTemplVar);

               var assignFromVar = new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     PropertyName = "InitialTemplate",
                     TargetObject = optionsVar
                  },
                  Right = new CodeObjectCreateExpression(
                     typeof(XmlQualifiedName),
                     new CodeMethodInvokeExpression {
                        Method = new CodeMethodReferenceExpression {
                           MethodName = "ToString",
                           TargetObject = new CodeVariableReferenceExpression(initialTemplVar.Name)
                        }
                     }
                  )
               };

               if (bindRequired) {
                  statements.Add(assignFromVar);
               } else {

                  var ifStatement = new CodeConditionStatement {
                     Condition = new CodeBinaryOperatorExpression {
                        Left = new CodeVariableReferenceExpression(initialTemplVar.Name),
                        Operator = CodeBinaryOperatorType.IdentityInequality,
                        Right = new CodePrimitiveExpression(null)
                     },
                     TrueStatements = { assignFromVar },
                     FalseStatements = { assignFromPrimitive }
                  };

                  statements.Add(ifStatement);
               }

            } else {
               statements.Add(assignFromPrimitive);
            }
         }

         // Initial context node
         //------------------------------------------------

         if (initialContextNodeField != null || this.parser.PageType == XsltPageType.SimplifiedStylesheet) {

            if (initialContextNodeField != null) {

               statements.Add(
                  new CodeAssignStatement {
                     Left = new CodePropertyReferenceExpression {
                        PropertyName = "InitialContextNode",
                        TargetObject = optionsVar
                     },
                     Right = new CodeFieldReferenceExpression {
                        FieldName = initialContextNodeField.Name,
                        TargetObject = PageTypeReferenceExpression
                     }
                  }
               );

            } else {

               statements.Add(
                  new CodeAssignStatement {
                     Left = new CodePropertyReferenceExpression {
                        PropertyName = "InitialContextNode",
                        TargetObject = optionsVar
                     },
                     Right = new CodeMethodInvokeExpression {
                        Method = new CodeMethodReferenceExpression {
                           MethodName = "CreateNodeReadOnly",
                           TargetObject = new CodePropertyReferenceExpression {
                              PropertyName = "ItemFactory",
                              TargetObject = new CodePropertyReferenceExpression {
                                 PropertyName = "Processor",
                                 TargetObject = new CodePropertyReferenceExpression {
                                    PropertyName = "Executable",
                                    TargetObject = new CodeThisReferenceExpression()
                                 }
                              }
                           }
                        },
                     }
                  }
               );
            }
         }
      }
   }
}
