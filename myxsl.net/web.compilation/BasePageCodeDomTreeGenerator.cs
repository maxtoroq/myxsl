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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using System.Xml;
using myxsl.net.web.ui;
using System.Xml.XPath;

namespace myxsl.net.web.compilation {

   public abstract class BasePageCodeDomTreeGenerator : BaseCodeDomTreeGenerator {

      readonly BasePageParser parser;

      CodeMemberField fileDependenciesField, outputCacheField;
      CodeBaseReferenceExpression @base = new CodeBaseReferenceExpression();
      CodeThisReferenceExpression @this = new CodeThisReferenceExpression();

      protected abstract Type PageBaseClass { get; }
      protected CodeTypeReferenceExpression PageTypeReferenceExpression { get; private set; }

      protected BasePageCodeDomTreeGenerator(BasePageParser parser) {
         this.parser = parser;
      }

      public override void BuildCodeDomTree(CodeCompileUnit compileUnit) {
         
         var codeNamespace = new CodeNamespace(GeneratedTypeNamespace);
         BuildNamespace(codeNamespace);

         compileUnit.Namespaces.Add(codeNamespace);
      }

      protected virtual void BuildNamespace(CodeNamespace codeNamespace) {

         CodeTypeDeclaration codeType = new CodeTypeDeclaration {
            Name = GeneratedTypeName,
            IsClass = true
         };

         foreach (ParsedValue<string> item in this.parser.Namespaces) {
            
            var import = new CodeNamespaceImport(item.Value);

            if (item.FileName != null) 
               import.LinePragma = new CodeLinePragma(item.FileName, item.LineNumber);

            codeNamespace.Imports.Add(import);
         }

         BuildPageClass(codeType);

         codeNamespace.Types.Add(codeType);
      }

      protected virtual void BuildPageClass(CodeTypeDeclaration codeType) {

         this.PageTypeReferenceExpression = new CodeTypeReferenceExpression(new CodeTypeReference(codeType.Name));

         AddPageBaseTypes(codeType.BaseTypes);
         AddPageFields(codeType.Members);
         AddPageProperties(codeType.Members);
         
         CodeTypeConstructor cctor = new CodeTypeConstructor();
         cctor.CustomAttributes.Add(new CodeAttributeDeclaration(DebuggerNonUserCodeTypeReference));
         AddPageTypeCtorStatements(cctor.Statements);
         
         if (cctor.Statements.Count > 0)
            codeType.Members.Add(cctor);

         AddPageMethods(codeType.Members);
      }

      protected virtual void AddPageBaseTypes(CodeTypeReferenceCollection baseTypes) {

         baseTypes.Add(PageBaseClass);
         baseTypes.Add(typeof(System.Web.IHttpHandler));

         switch (parser.EnableSessionState) {
            case PagesEnableSessionState.ReadOnly:
               baseTypes.Add(typeof(IReadOnlySessionState));
               break;
            case PagesEnableSessionState.True:
               baseTypes.Add(typeof(IRequiresSessionState));
               break;
         }
      }

      protected virtual void AddPageFields(CodeTypeMemberCollection members) {

         fileDependenciesField = new CodeMemberField {
            Name = "__fileDependencies",
            Type = new CodeTypeReference(typeof(string[])),
            Attributes = MemberAttributes.Private | MemberAttributes.Static
         };

         members.Add(fileDependenciesField);

         if (parser.OutputCache != null) {
            outputCacheField = new CodeMemberField {
               Name = "__outputCacheSettings",
               Type = new CodeTypeReference(typeof(System.Web.UI.OutputCacheParameters)),
               Attributes = MemberAttributes.Private | MemberAttributes.Static
            };
            members.Add(outputCacheField);
         }
      }

      protected virtual void AddPageProperties(CodeTypeMemberCollection members) { }

      protected virtual void AddPageTypeCtorStatements(CodeStatementCollection statements) {

         var fileDepArr = new CodeArrayCreateExpression {
            CreateType = new CodeTypeReference(typeof(string))
         };

         for (int i = 0; i < this.parser.SourceDependencies.Count; i++) {
            string appRelPath = VirtualPathUtility.ToAppRelative(this.parser.SourceDependencies[i]);
            fileDepArr.Initializers.Add(new CodeMethodInvokeExpression {
               Method = new CodeMethodReferenceExpression {
                  MethodName = "MapPath",
                  TargetObject = new CodeTypeReferenceExpression(typeof(HostingEnvironment))
               },
               Parameters = { new CodePrimitiveExpression(appRelPath) }
            });
         }

         statements.Add(new CodeAssignStatement {
            Left = new CodeFieldReferenceExpression(this.PageTypeReferenceExpression, fileDependenciesField.Name),
            Right = fileDepArr
         });

         if (outputCacheField != null) {
            CodeFieldReferenceExpression outputCacheFieldRef =
               new CodeFieldReferenceExpression(this.PageTypeReferenceExpression, outputCacheField.Name);

            statements.AddRange(new[] {
               new CodeAssignStatement {
                  Left = outputCacheFieldRef,
                  Right = new CodeObjectCreateExpression(typeof(System.Web.UI.OutputCacheParameters))
               },
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     TargetObject = outputCacheFieldRef,
                     PropertyName = "CacheProfile"
                  },
                  Right = new CodePrimitiveExpression(this.parser.OutputCache.CacheProfile)
               },
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     TargetObject = outputCacheFieldRef,
                     PropertyName = "Duration"
                  },
                  Right = new CodePrimitiveExpression(this.parser.OutputCache.Duration)
               },
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     TargetObject = outputCacheFieldRef,
                     PropertyName = "Location"
                  },
                  Right = new CodePropertyReferenceExpression { 
                     TargetObject = new CodeTypeReferenceExpression(typeof(System.Web.UI.OutputCacheLocation)),
                     PropertyName = this.parser.OutputCache.Location.ToString()
                  }
               },
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     TargetObject = outputCacheFieldRef,
                     PropertyName = "NoStore"
                  },
                  Right = new CodePrimitiveExpression(this.parser.OutputCache.NoStore)
               },
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     TargetObject = outputCacheFieldRef,
                     PropertyName = "VaryByContentEncoding"
                  },
                  Right = new CodePrimitiveExpression(this.parser.OutputCache.VaryByContentEncoding)
               },
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     TargetObject = outputCacheFieldRef,
                     PropertyName = "VaryByCustom"
                  },
                  Right = new CodePrimitiveExpression(this.parser.OutputCache.VaryByCustom)
               },
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     TargetObject = outputCacheFieldRef,
                     PropertyName = "VaryByHeader"
                  },
                  Right = new CodePrimitiveExpression(this.parser.OutputCache.VaryByHeader)
               },
               new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression {
                     TargetObject = outputCacheFieldRef,
                     PropertyName = "VaryByParam"
                  },
                  Right = new CodePrimitiveExpression(this.parser.OutputCache.VaryByParam)
               }
            });
         }
      }

      protected virtual void AddPageMethods(CodeTypeMemberCollection members) {

         var processRequest = new CodeMemberMethod {
            Name = "ProcessRequest",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            Parameters = { 
               new CodeParameterDeclarationExpression(typeof(System.Web.HttpContext), "context")
            }
         };
         processRequest.Statements.Add(new CodeMethodInvokeExpression(@base, processRequest.Name, new CodeVariableReferenceExpression(processRequest.Parameters[0].Name)));
         processRequest.CustomAttributes.Add(new CodeAttributeDeclaration(this.DebuggerNonUserCodeTypeReference));
         members.Add(processRequest);

         var fxInit = new CodeMemberMethod {
            Name = "FrameworkInitialize",
            Attributes = MemberAttributes.Family | MemberAttributes.Override,
            CustomAttributes = { 
               new CodeAttributeDeclaration(this.DebuggerNonUserCodeTypeReference)
            }
         };

         AddFrameworkInitializeStatements(fxInit.Statements);

         if (fxInit.Statements.Count > 0) {

            fxInit.Statements.Add( 
               new CodeExpressionStatement(
                  new CodeMethodInvokeExpression { 
                     Method = new CodeMethodReferenceExpression {
                        MethodName = "FrameworkInitialize",
                        TargetObject = new CodeBaseReferenceExpression()
                     }
                  }
               )
            );

            members.Add(fxInit);
         }

         var addFileDep = new CodeMemberMethod {
            Name = "AddFileDependencies",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            CustomAttributes = { 
               new CodeAttributeDeclaration(this.DebuggerNonUserCodeTypeReference)
            },
            Statements = { 
               new CodeMethodInvokeExpression(@this, "AddFileDependencies", new CodeFieldReferenceExpression(this.PageTypeReferenceExpression, this.fileDependenciesField.Name))
            }
         };

         members.Add(addFileDep);

         if (parser.Parameters.Count > 0)
            members.Add(GetSetBoundParametersMethod());
      }

      protected virtual void AddFrameworkInitializeStatements(CodeStatementCollection statements) {

         if (this.parser.AcceptVerbs.Count > 0) {
            
            statements.Add(
               new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "CheckHttpMethod",
                     TargetObject = @this
                  },
                  Parameters = { 
                     new CodeArrayCreateExpression(typeof(string), this.parser.AcceptVerbs.Select(s => new CodePrimitiveExpression(s)).ToArray())
                  }
               }
            );
         }

         if (this.parser.ValidateRequest != default(bool)) {
            statements.Add(
               new CodeMethodInvokeExpression(
                  new CodePropertyReferenceExpression(@this, "Request"),
                  "ValidateInput"
               )
            );
         }

         if (this.parser.ContentType != null) {
            statements.Add(new CodeAssignStatement {
               Left = new CodePropertyReferenceExpression {
                  TargetObject = new CodePropertyReferenceExpression(@this, "Response"),
                  PropertyName = "ContentType"
               },
               Right = new CodePrimitiveExpression(this.parser.ContentType)
            });
         }

         if (this.outputCacheField != null) {
            statements.Add(
               new CodeMethodInvokeExpression(@this, "InitOutputCache", new CodeFieldReferenceExpression(this.PageTypeReferenceExpression, this.outputCacheField.Name))
            );
         }
      }

      CodeMemberMethod GetSetBoundParametersMethod() {

         var setParams = new CodeMemberMethod {
            Name = "SetBoundParameters",
            Attributes = MemberAttributes.Family | MemberAttributes.Override,
            CustomAttributes = { 
               new CodeAttributeDeclaration(this.DebuggerNonUserCodeTypeReference)
            },
            Parameters = { 
               new CodeParameterDeclarationExpression {
                  Name = "parameters",
                  Type = new CodeTypeReference(typeof(IDictionary<XmlQualifiedName, object>))
               }
            }
         };

         var @this = new CodeThisReferenceExpression();
         var parameters = new CodeVariableReferenceExpression(setParams.Parameters[0].Name);

         int pindex = 0;

         foreach (PageParameterInfo param in this.parser.Parameters) {

            if (param.Binding == null)
               continue;

            pindex++;

            BindingExpressionInfo bind = param.Binding;
            string paramName = param.Name;
            bool atomicValueSequence = (param.AtomicTypeName != null && !param.AtomicTypeName.IsEmpty);

            CodeExpression valueExpr = bind.GetCodeExpression();

            valueExpr = new CodeMethodInvokeExpression {
               Method = new CodeMethodReferenceExpression {
                  MethodName = "CheckParamLength",
                  TargetObject = @this
               },
               Parameters = {
                  new CodePrimitiveExpression(param.Name),
                  valueExpr,
                  new CodePrimitiveExpression(param.MinLength),
                  new CodePrimitiveExpression(param.MaxLength)
               }
            };

            if (bind.ParsedValues.ContainsKey("accept")) {

               valueExpr = new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "CheckParamValues",
                     TargetObject = new CodeThisReferenceExpression()
                  },
                  Parameters = { 
                     new CodePrimitiveExpression(param.Name),
                     valueExpr,
                     new CodeArrayCreateExpression(typeof(string), ((string[])bind.ParsedValues["accept"]).Select(s => new CodePrimitiveExpression(s)).ToArray())
                  }
               };
            }

            if (atomicValueSequence) {
               
               valueExpr = new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "CreateAtomicValueSequence",
                     TargetObject = new CodePropertyReferenceExpression {
                        PropertyName = "ItemFactory",
                        TargetObject = new CodePropertyReferenceExpression {
                           PropertyName = "Processor",
                           TargetObject = new CodePropertyReferenceExpression {
                              PropertyName = "Executable",
                              TargetObject = @this
                           }
                        }
                     }
                  },
                  Parameters = {
                     valueExpr,
                     new CodeObjectCreateExpression {
                        CreateType = new CodeTypeReference(typeof(XmlQualifiedName)),
                        Parameters = {
                           new CodePrimitiveExpression(param.AtomicTypeName.Name),
                           new CodePrimitiveExpression(param.AtomicTypeName.Namespace)
                        }
                     }
                  }
               };
            }

            CodeVariableDeclarationStatement pvarStatement =
               new CodeVariableDeclarationStatement { 
                  Type = new CodeTypeReference((atomicValueSequence)? typeof(IEnumerable<XPathItem>) : typeof(object)),
                  Name = "p" + pindex,
                  InitExpression = valueExpr
               };

            if (bind.LineNumber > 0)
               pvarStatement.LinePragma = new CodeLinePragma(this.parser.PhysicalPath.LocalPath, bind.LineNumber);

            var pvarReference = new CodeVariableReferenceExpression {
               VariableName = pvarStatement.Name
            };

            CodeBinaryOperatorExpression pvarCheckCondition =
               new CodeBinaryOperatorExpression {
                  Left = pvarReference,
                  Operator = CodeBinaryOperatorType.IdentityInequality,
                  Right = new CodePrimitiveExpression(null)
               };

            if (atomicValueSequence) {
               pvarCheckCondition.Left = new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "Count",
                     TargetObject = new CodeTypeReferenceExpression(typeof(Enumerable))
                  },
                  Parameters = { 
                     pvarCheckCondition.Left
                  }
               };

               pvarCheckCondition.Operator = CodeBinaryOperatorType.GreaterThan;
               pvarCheckCondition.Right = new CodePrimitiveExpression(0);
            }

            CodeConditionStatement pvarCheckStatement =
               new CodeConditionStatement {
                  Condition = pvarCheckCondition,
                  TrueStatements = {
                     new CodeAssignStatement {
                        Left = new CodeIndexerExpression {
                           Indices = { 
                              new CodeObjectCreateExpression {
                                 CreateType = new CodeTypeReference(typeof(XmlQualifiedName)),
                                 Parameters = { 
                                    new CodePrimitiveExpression(paramName)
                                 }
                              }
                           },
                           TargetObject = parameters
                        },
                        Right = pvarReference
                     }
                  }
               };
           
            setParams.Statements.Add(pvarStatement);
            setParams.Statements.Add(pvarCheckStatement);
         }

         return setParams;
      }
   }
}
