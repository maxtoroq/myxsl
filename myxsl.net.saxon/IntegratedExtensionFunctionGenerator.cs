// Copyright 2012 Max Toro Q.
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.CSharp;
using myxsl.net.common;
using Saxon.Api;

namespace myxsl.net.saxon {
   
   class IntegratedExtensionFunctionGenerator {

      const string XMLSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

      readonly CodeDomProvider provider;

      public IntegratedExtensionFunctionGenerator() {
         this.provider = new CSharpCodeProvider();
      }

      public Type[] Generate(IEnumerable<XPathModuleInfo> modules) {

         bool debuggerIsAttached = Debugger.IsAttached;

         var generatedTypeNames = new List<string>();

         var compilerParams = new CompilerParameters {
            GenerateInMemory = !debuggerIsAttached,
            ReferencedAssemblies = { 
               typeof(IntegratedExtensionFunctionGenerator).Assembly.Location,
               typeof(ExtensionFunctionDefinition).Assembly.Location,
               typeof(Enumerable).Assembly.Location,
               typeof(Uri).Assembly.Location
            }
         };
         
         var namespaces = new CodeNamespaceCollection();

         foreach (XPathModuleInfo module in modules) {

            string[] functionDefTypeNames;

            namespaces.Add(GenerateModuleTypes(module, out functionDefTypeNames));
            generatedTypeNames.AddRange(functionDefTypeNames);

            AddAssemblyReference(compilerParams, module.Type.Assembly);

            foreach (XPathFunctionInfo function in module.Functions) {
               AddAssemblyReference(compilerParams, function.ReturnType.ClrType.Assembly);

               foreach (XPathVariableInfo param in function.Parameters) {
                  AddAssemblyReference(compilerParams, param.Type.ClrType.Assembly);
               }
            }
         }
         
         var compileUnit = new CodeCompileUnit();
         compileUnit.Namespaces.AddRange(namespaces);

         CompilerResults result = this.provider.CompileAssemblyFromDom(compilerParams, compileUnit);

         CompilerError firstError = result.Errors.Cast<CompilerError>().FirstOrDefault(e => !e.IsWarning);

         if (firstError != null) {

            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture)) {
               
               provider.GenerateCodeFromCompileUnit(compileUnit, stringWriter, null);

               if (debuggerIsAttached) {
                  Debugger.Log(0, Debugger.DefaultCategory, "Integrated extension function generation error:" + Environment.NewLine);
                  Debugger.Log(0, Debugger.DefaultCategory, stringWriter.ToString());
               }
            }

            throw new ArgumentException(firstError.ErrorText, "modules");
         }

         if (debuggerIsAttached)
            Debugger.Log(0, Debugger.DefaultCategory, "Integrated extension function(s) generated " + result.PathToAssembly + Environment.NewLine);

         return generatedTypeNames
            .Select(n => result.CompiledAssembly.GetType(n, throwOnError: true))
            .ToArray();
      }

      static void AddAssemblyReference(CompilerParameters compilerParams, Assembly assembly) {

         string location = assembly.Location;

         if (location.HasValue()
            && !compilerParams.ReferencedAssemblies.Contains(location)) {
            compilerParams.ReferencedAssemblies.Add(location);
         }
      }

      CodeNamespace GenerateModuleTypes(XPathModuleInfo module, out string[] functionDefTypeNames) {

         var namespaceBuilder = new StringBuilder()
            .Append(typeof(IntegratedExtensionFunctionGenerator).Namespace)
            .Append(".modules.")
            .Append(module.Type.FullName.Replace('.', '_'))
            .Append("_functions");

         var nspace = new CodeNamespace {
            Name = namespaceBuilder.ToString(),
            Imports = { 
               new CodeNamespaceImport(typeof(Enumerable).Namespace),
               new CodeNamespaceImport(typeof(SaxonExtensions).Namespace)
            }
         };

         var groupedByName =
            from f in module.Functions
            group f by f.Name;

         var functionDefNames = new List<string>();

         foreach (var functions in groupedByName) {

            Tuple<CodeTypeDeclaration, CodeTypeDeclaration> funcDefAndCall = GenerateFunctionType(functions.ToArray());

            nspace.Types.Add(funcDefAndCall.Item1);
            nspace.Types.Add(funcDefAndCall.Item2);

            functionDefNames.Add(nspace.Name + "." + funcDefAndCall.Item1.Name);
         }

         functionDefTypeNames = functionDefNames.ToArray();

         return nspace;
      }

      Tuple<CodeTypeDeclaration, CodeTypeDeclaration> GenerateFunctionType(XPathFunctionInfo[] functions) {

         XPathFunctionInfo first = functions.First();
         XPathFunctionInfo leastParameters = functions.OrderBy(f => f.Parameters.Count).First();
         XPathFunctionInfo mostParameters = functions.OrderByDescending(f => f.Parameters.Count).First();
         XPathModuleInfo module = first.Module;
         int minArgs = leastParameters.Parameters.Count;
         int maxArgs = mostParameters.Parameters.Count;

         CodeExpression thisRef = new CodeThisReferenceExpression();

         var processorField = new CodeMemberField { 
            Name = "_processor",
            Type = new CodeTypeReference(typeof(SaxonProcessor))
         };

         var funcNameField = new CodeMemberField {
            Name = "_FunctionName",
            Type = new CodeTypeReference(typeof(QName)),
            InitExpression = new CodeObjectCreateExpression(
               typeof(QName),
               new CodePrimitiveExpression(module.Namespace),
               new CodePrimitiveExpression(first.Name)
            )
         };

         var argTypesField = new CodeMemberField {
            Name = "_ArgumentTypes",
            Type = new CodeTypeReference(typeof(XdmSequenceType[])),
            InitExpression = new CodeArrayCreateExpression(
               typeof(XdmSequenceType),
               mostParameters.Parameters.Select(p => 
                  new CodeObjectCreateExpression(
                     typeof(XdmSequenceType), 
                     GetXdmItemTypeExpression(p.Type.ItemType),
                     new CodePrimitiveExpression(GetOcurrenceIndicator(p.Type.Cardinality))
                  )).ToArray()
            )
         };

         var resultTypesField = new CodeMemberField {
            Name = "_resultTypes",
            Type = new CodeTypeReference(typeof(XdmSequenceType[]))
         };

         var resultTypesFieldInit = new CodeArrayCreateExpression { 
            CreateType = resultTypesField.Type,
            Size = functions.Length
         };

         for (int i = 0; i < functions.Length; i++) {

            XPathFunctionInfo function = functions[i];

            resultTypesFieldInit.Initializers.Add(
               
               // Using item()? instead of empty-sequence()
               
               (function.ReturnType.IsEmptySequence) ?
                  
                  new CodeObjectCreateExpression(
                     typeof(XdmSequenceType),
                     new CodeFieldReferenceExpression(
                        new CodeTypeReferenceExpression(typeof(XdmAnyItemType)),
                        "Instance"
                     ),
                     new CodePrimitiveExpression(GetOcurrenceIndicator(XPathSequenceCardinality.ZeroOrOne))
                  )

                  : new CodeObjectCreateExpression(
                     typeof(XdmSequenceType),
                     GetXdmItemTypeExpression(function.ReturnType.ItemType),
                     new CodePrimitiveExpression(GetOcurrenceIndicator(function.ReturnType.Cardinality))
                  )
            );
         }

         resultTypesField.InitExpression = resultTypesFieldInit;

         var defClass = new CodeTypeDeclaration {
            Name = first.Method.Name + "Function",
            Attributes = MemberAttributes.Public,
            BaseTypes = { typeof(ExtensionFunctionDefinition) },
            Members = { 
               processorField,
               funcNameField,
               argTypesField,
               resultTypesField,
               new CodeConstructor {
                  Attributes = MemberAttributes.Public,
                  Parameters = {
                     new CodeParameterDeclarationExpression(processorField.Type, "processor")
                  },
                  Statements = {
                     new CodeAssignStatement(
                        new CodeFieldReferenceExpression(thisRef, processorField.Name),
                        new CodeVariableReferenceExpression("processor")
                     )
                  }
               },
               new CodeMemberProperty {
                  Name = "FunctionName",
                  Type = funcNameField.Type,
                  Attributes = MemberAttributes.Public | MemberAttributes.Override,
                  HasGet = true,
                  GetStatements = {
                     new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, funcNameField.Name))
                  }
               },
               new CodeMemberProperty {
                  Name = "ArgumentTypes",
                  Type = argTypesField.Type,
                  Attributes = MemberAttributes.Public | MemberAttributes.Override,
                  HasGet = true,
                  GetStatements = {
                     new CodeMethodReturnStatement(new CodeFieldReferenceExpression(thisRef, argTypesField.Name))
                  }
               },
               new CodeMemberProperty {
                  Name = "MaximumNumberOfArguments",
                  Type = new CodeTypeReference(typeof(int)),
                  Attributes = MemberAttributes.Public | MemberAttributes.Override,
                  HasGet = true,
                  GetStatements = {
                     new CodeMethodReturnStatement(new CodePrimitiveExpression(maxArgs))
                  }
               },
               new CodeMemberProperty {
                  Name = "MinimumNumberOfArguments",
                  Type = new CodeTypeReference(typeof(int)),
                  Attributes = MemberAttributes.Public | MemberAttributes.Override,
                  HasGet = true,
                  GetStatements = {
                     new CodeMethodReturnStatement(new CodePrimitiveExpression(minArgs))
                  }
               },
               GenerateResultTypeMethod(thisRef, resultTypesField)
            }
         };

         if (first.HasSideEffects) {

            defClass.Members.Add(new CodeMemberProperty {
               Name = "HasSideEffects",
               Type = new CodeTypeReference(typeof(bool)),
               Attributes = MemberAttributes.Public | MemberAttributes.Override,
               HasGet = true,
               GetStatements = { 
                  new CodeMethodReturnStatement(new CodePrimitiveExpression(true))
               }
            });
         }

         CodeMemberMethod initializeMethod;

         var callClass = new CodeTypeDeclaration {
            Name = defClass.Name + "Call",
            Attributes = MemberAttributes.Assembly,
            BaseTypes = { 
               new CodeTypeReference(typeof(ExtensionFunctionCall)) 
            },
            Members = { 
               processorField,
               new CodeConstructor {
                  Attributes = MemberAttributes.Public,
                  Parameters = {
                     new CodeParameterDeclarationExpression(processorField.Type, "processor")
                  },
                  Statements = {
                     new CodeAssignStatement(
                        new CodeFieldReferenceExpression(thisRef, processorField.Name),
                        new CodeVariableReferenceExpression("processor")
                     )
                  }
               },
               GenerateCallMethod(functions, minArgs, maxArgs, new CodeFieldReferenceExpression(thisRef, processorField.Name), out initializeMethod)
            }
         };

         if (initializeMethod != null) 
            callClass.Members.Add(initializeMethod);

         defClass.Members.Add(
            new CodeMemberMethod {
               Name = "MakeFunctionCall",
               Attributes = MemberAttributes.Public | MemberAttributes.Override,
               ReturnType = new CodeTypeReference(typeof(ExtensionFunctionCall)),
               Statements = { 
                  new CodeMethodReturnStatement(
                     new CodeObjectCreateExpression(
                        callClass.Name,
                        new CodeFieldReferenceExpression(thisRef, processorField.Name)
                     )
                  )
               }
            }
         );

         return Tuple.Create(defClass, callClass);
      }

      static CodeMemberMethod GenerateResultTypeMethod(CodeExpression thisRef, CodeMemberField resultTypesField) {

         var argTypesParam = new CodeParameterDeclarationExpression(typeof(XdmSequenceType[]), "ArgumentTypes");

         var method = new CodeMemberMethod {
            Name = "ResultType",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            ReturnType = new CodeTypeReference(typeof(XdmSequenceType)),
            Parameters = { 
               argTypesParam
            },
            Statements = {
               new CodeMethodReturnStatement(
                  new CodeArrayIndexerExpression(
                     new CodeFieldReferenceExpression(thisRef, resultTypesField.Name),
                     new CodeBinaryOperatorExpression(
                        new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(argTypesParam.Name), "Length"),
                        CodeBinaryOperatorType.Subtract,
                        new CodePropertyReferenceExpression(thisRef, "MinimumNumberOfArguments")
                     )
                  )
               )
            }
         };

         return method;
      }
      
      CodeMemberMethod GenerateCallMethod(XPathFunctionInfo[] functions, int minArgs, int maxArgs, CodeExpression processorRef, out CodeMemberMethod initializeMethod) {

         initializeMethod = null;

         XPathFunctionInfo mostParameters = functions.OrderByDescending(f => f.Parameters.Count).First();
         XPathModuleInfo module = mostParameters.Module;

         var argsParam = new CodeParameterDeclarationExpression(typeof(IXdmEnumerator[]), "arguments");

         var callMethod = new CodeMemberMethod {
            Name = "Call",
            Attributes = MemberAttributes.Public | MemberAttributes.Override,
            ReturnType = new CodeTypeReference(typeof(IXdmEnumerator)),
            Parameters = { 
               argsParam,
               new CodeParameterDeclarationExpression(typeof(DynamicContext), "context")
            }
         };

         var argumentsRef = new CodeVariableReferenceExpression(callMethod.Parameters[0].Name);

         CodeExpression moduleRef;

         if (module.TypeIsStatic) {
            moduleRef = new CodeTypeReferenceExpression(module.Type);
         } else {

            var moduleVar = new CodeVariableDeclarationStatement { 
               Name = "module",
               Type = new CodeTypeReference(module.Type),
               InitExpression = new CodeObjectCreateExpression(module.Type)
            };

            callMethod.Statements.Add(moduleVar);

            moduleRef = new CodeVariableReferenceExpression(moduleVar.Name);

            if (module.Dependencies.Count > 0) {
               initializeMethod = GenerateInitialize(module, processorRef);

               callMethod.Statements.Add(new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), initializeMethod.Name),
                  Parameters = { moduleRef }
               });
            }
         }

         CodeStatementCollection currentBlock = callMethod.Statements;
         var paramRef = new List<CodeExpression>();
         
         for (int pos = 0; pos <= maxArgs; pos++) {

            if (pos > 0) {

               XPathSequenceType paramInfo = mostParameters.Parameters[pos - 1].Type;

               var paramVar = new CodeVariableDeclarationStatement {
                  Name = "p" + pos,
                  Type = new CodeTypeReference(paramInfo.ClrType),
                  InitExpression = TransformInput(argumentsRef, pos, paramInfo)
               };

               currentBlock.Add(paramVar);

               paramRef.Add(new CodeVariableReferenceExpression(paramVar.Name));
            }

            if (pos >= minArgs) {

               XPathFunctionInfo fn = functions[pos - minArgs];

               CodeConditionStatement ifElse = null;

               if (minArgs != maxArgs
                  && pos < maxArgs) {
                  
                  ifElse = new CodeConditionStatement {
                     Condition = new CodeBinaryOperatorExpression {
                        Left = new CodePropertyReferenceExpression(argumentsRef, "Length"),
                        Operator = CodeBinaryOperatorType.ValueEquality,
                        Right = new CodePrimitiveExpression(pos)
                     }
                  };

                  currentBlock.Add(ifElse);
                  currentBlock = ifElse.TrueStatements;
               }

               var functionInvoke = new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression(moduleRef, fn.Method.Name)
               };

               functionInvoke.Parameters.AddRange(paramRef.ToArray());

               CodeExpression returnExpr;

               if (!fn.ReturnType.IsEmptySequence) {

                  returnExpr = TransformOutput(functionInvoke, fn.ReturnType, processorRef);

               } else {

                  currentBlock.Add(functionInvoke);

                  returnExpr = new CodePropertyReferenceExpression {
                     PropertyName = "INSTANCE",
                     TargetObject = new CodeTypeReferenceExpression(typeof(EmptyEnumerator))
                  };
               }

               currentBlock.Add(new CodeMethodReturnStatement(returnExpr));

               if (ifElse != null) 
                  currentBlock = ifElse.FalseStatements;
            }
         }

         return callMethod;
      }

      CodeMemberMethod GenerateInitialize(XPathModuleInfo module, CodeExpression processorRef) {

         var initializeMethod = new CodeMemberMethod {
            Name = "Initialize",
            Attributes = MemberAttributes.Private | MemberAttributes.Final,
            Parameters = { 
               new CodeParameterDeclarationExpression(module.Type, "module")
            }
         };

         for (int i = 0; i < module.Dependencies.Count; i++) {
            
            XPathDependencyInfo dependency = module.Dependencies[i];

            CodeExpression expr = null;

            if (dependency.Type == typeof(IXsltProcessor)
               || dependency.Type == typeof(IXQueryProcessor)) {

               expr = processorRef;
            
            } else if (dependency.Type == typeof(XPathItemFactory)) {
               expr = GetItemFactoryReference(processorRef);
            
            } else if (dependency.Type == typeof(XmlResolver)) {
               expr = new CodeObjectCreateExpression(typeof(XmlDynamicResolver));
            }

            if (expr != null) {

               initializeMethod.Statements.Add(new CodeAssignStatement {
                  Left = new CodePropertyReferenceExpression(
                     new CodeVariableReferenceExpression(initializeMethod.Parameters[0].Name), 
                     dependency.Property.Name
                  ),
                  Right = expr
               }); 
            }
         }

         return initializeMethod;
      }

      CodeExpression TransformInput(CodeVariableReferenceExpression argumentsRef, int position, XPathSequenceType sequenceType) {

         StringBuilder codeBuilder = new StringBuilder();
         codeBuilder.Append(argumentsRef.VariableName);
         codeBuilder.AppendFormatInvariant("[{0}]", position - 1);

         bool isAtomic = sequenceType.ItemType.Kind == XPathItemKind.Atomic;
         bool isNode = sequenceType.ItemType.KindIsNode;

         if (isAtomic) {
            codeBuilder.Append(".AsAtomicValues()");
         
         } else if (isNode) {
            codeBuilder.Append(".AsNodes()");
            
         } else {
            codeBuilder.Append(".AsItems()");
         }

         StringBuilder itemExpr = new StringBuilder("x");

         if (isAtomic) {
            
            itemExpr.Append(".Value");

            QName atomicSchemaType = GetAtomicSchemaType(sequenceType.ItemType);
            
            Type expectedType = sequenceType.ItemType.ClrType;
            Type actualType = (atomicSchemaType.Uri == XMLSchemaNamespace) ?
               SaxonAtomicMapping(atomicSchemaType.LocalName)
               : typeof(object);

            string expectedTypeName = GetCSharpFullName(expectedType);

            if (expectedType.IsAssignableFrom(actualType)) {
               itemExpr.Insert(0, "({0})".FormatInvariant(expectedTypeName));
            
            } else {

               if (actualType == typeof(QName)
                  && expectedType == typeof(XmlQualifiedName)) {

                  itemExpr.Insert(0, "(({0})".FormatInvariant(GetCSharpFullName(actualType)));
                  itemExpr.Append(").ToXmlQualifiedName()");

               } else {

                  itemExpr.Insert(0, "({0}){1}.ChangeType(".FormatInvariant(expectedTypeName, typeof(Convert).FullName));
                  itemExpr.AppendFormatInvariant(", typeof({0}))", expectedTypeName);
               }
            }

         } else if (isNode) {
            itemExpr.Append(".ToXPathNavigator()");

         } else {
            itemExpr.Append(".ToXPathItem()");
         }

         if (itemExpr.Length > 1) {

            codeBuilder.Append(".Select(x => ");
            codeBuilder.Append(itemExpr.ToString());
            codeBuilder.Append(")");
         }

         switch (sequenceType.Cardinality) {
            case XPathSequenceCardinality.One:
               codeBuilder.Append(".Single()");
               break;
            
            case XPathSequenceCardinality.ZeroOrOne:

               if (sequenceType.ClrTypeIsNullableValueType) {
                  codeBuilder.Insert(0, typeof(SaxonExtensions).FullName + ".SingleOrNull(");
                  codeBuilder.Append(")");
               
               } else {
                  codeBuilder.Append(".SingleOrDefault()");
               }

               break;

            case XPathSequenceCardinality.OneOrMore:
            case XPathSequenceCardinality.ZeroOrMore:

               if (sequenceType.ClrType.IsArray) 
                  codeBuilder.Append(".ToArray()");

               break;
         }

         return new CodeSnippetExpression(codeBuilder.ToString());
      }

      string GetCSharpFullName(Type type) {
         using (var writer = new StringWriter()) {
            this.provider.GenerateCodeFromExpression(new CodeTypeReferenceExpression(type), writer, null);
            return writer.ToString();
         }
      }

      static CodeExpression TransformOutput(CodeExpression functionResultRef, XPathSequenceType sequenceType, CodeExpression processorRef) {

         CodeExpression itemFactoryRef = GetItemFactoryReference(processorRef);

         CodeExpression expr = new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = "ToXdmValue",
               TargetObject = new CodeTypeReferenceExpression(typeof(SaxonExtensions))
            },
            Parameters = { 
               functionResultRef,
               itemFactoryRef
            }
         };

         if (sequenceType.ItemType.Kind == XPathItemKind.Element
            || sequenceType.ItemType.Kind == XPathItemKind.SchemaElement) {

               expr = new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "FirstElementOrSelf",
                     TargetObject = new CodeTypeReferenceExpression(typeof(SaxonExtensions))
                  },
                  Parameters = { expr }
               };
         }

         return new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = "GetXdmEnumerator",
               TargetObject = new CodeTypeReferenceExpression(typeof(SaxonExtensions))
            },
            Parameters = { expr }
         };
      }

      static CodeExpression GetXdmItemTypeExpression(XPathItemType itemType) {

         switch (itemType.Kind) {
            case XPathItemKind.AnyItem:
               return new CodeFieldReferenceExpression(
                  new CodeTypeReferenceExpression(typeof(XdmAnyItemType)), 
                  "Instance"
               );

            case XPathItemKind.Atomic:

               CodeExpression qnameRef = null;

               QName atomicSchemaType = GetAtomicSchemaType(itemType);

               if (atomicSchemaType.Uri == XMLSchemaNamespace) {

                  FieldInfo qnameField = typeof(QName).GetField("XS_" + atomicSchemaType.LocalName.ToUpperInvariant(), BindingFlags.Public | BindingFlags.Static);

                  if (qnameField != null) {
                     qnameRef = new CodeFieldReferenceExpression {
                        FieldName = qnameField.Name,
                        TargetObject = new CodeTypeReferenceExpression(typeof(QName))
                     };
                  }
               }

               if (qnameRef == null) {
                  qnameRef = new CodeObjectCreateExpression(
                     typeof(QName),
                     new CodePrimitiveExpression(atomicSchemaType.Uri),
                     new CodePrimitiveExpression(atomicSchemaType.LocalName)
                  );
               }

               return new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "BuiltInAtomicType",
                     TargetObject = new CodeTypeReferenceExpression(typeof(XdmAtomicType))
                  },
                  Parameters = { 
                     qnameRef
                  }
               };

            case XPathItemKind.AnyNode:
               return new CodeFieldReferenceExpression(
                  new CodeTypeReferenceExpression(typeof(XdmAnyNodeType)),
                  "Instance"
               );

            case XPathItemKind.Attribute:
            case XPathItemKind.SchemaAttribute:
               return new CodeFieldReferenceExpression(
                  new CodeTypeReferenceExpression(typeof(XdmNodeKind)),
                  "Attribute"
               );

            case XPathItemKind.Comment:
               return new CodeFieldReferenceExpression(
                  new CodeTypeReferenceExpression(typeof(XdmNodeKind)),
                  "Comment"
               );
               
            case XPathItemKind.Document:
               return new CodeFieldReferenceExpression(
                  new CodeTypeReferenceExpression(typeof(XdmNodeKind)),
                  "Document"
               );
               
            case XPathItemKind.Element:
            case XPathItemKind.SchemaElement:
               return new CodeFieldReferenceExpression(
                  new CodeTypeReferenceExpression(typeof(XdmNodeKind)),
                  "Element"
               );
               
            case XPathItemKind.ProcessingInstruction:
               return new CodeFieldReferenceExpression(
                  new CodeTypeReferenceExpression(typeof(XdmNodeKind)),
                  "ProcessingInstruction"
               );
               
            case XPathItemKind.Text:
               return new CodeFieldReferenceExpression(
                  new CodeTypeReferenceExpression(typeof(XdmNodeKind)),
                  "Text"
               );
               
            default:
               throw new ArgumentException("itemType not supported.", "itemType");
         }
      }

      static char GetOcurrenceIndicator(XPathSequenceCardinality cardinality) {

         switch (cardinality) {
            case XPathSequenceCardinality.One:
               return ' ';
               
            case XPathSequenceCardinality.ZeroOrOne:
               return '?';
               
            case XPathSequenceCardinality.OneOrMore:
               return '+';
               
            case XPathSequenceCardinality.ZeroOrMore:
               return '*';

            default:
               throw new ArgumentOutOfRangeException("cardinality");
         }
      }

      static QName GetAtomicSchemaType(XPathItemType itemType) {

         if (itemType.AtomicTypeOrNodeName != null)
            return new QName(itemType.AtomicTypeOrNodeName);

         Type clrType = itemType.ClrType;

         switch (Type.GetTypeCode(clrType)) {
            case TypeCode.Boolean:
               return QName.XS_BOOLEAN;
               
            case TypeCode.DateTime:
               return new QName(XMLSchemaNamespace, "xs:dateTime");
               
            case TypeCode.Decimal:
               return QName.XS_DECIMAL;
               
            case TypeCode.Double:
               return QName.XS_DOUBLE;

            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
               return QName.XS_INTEGER;
            
            case TypeCode.Single:
               return QName.XS_FLOAT;
               
            default:
            case TypeCode.String:
               return QName.XS_STRING;

            case TypeCode.Object:

               if (clrType == typeof(QName)
                  || clrType == typeof(XmlQualifiedName)) {
                  return QName.XS_QNAME;
               }

               if (clrType == typeof(Uri))
                  return QName.XS_ANYURI;

               return new QName(XMLSchemaNamespace, "xs:anyAtomicType");
         }
      }

      static Type SaxonAtomicMapping(string xmlSchemaType) {

         switch (xmlSchemaType) {
            case "anyAtomicType":
               return typeof(object);

            case "anyURI":
               return typeof(Uri);
            
            case "boolean":
               return typeof(bool);
            
            case "byte":
               return typeof(sbyte);

            case "long":
            case "integer":
               return typeof(long);

            case "int":
               return typeof(int);

            case "decimal":
               return typeof(decimal);

            case "double":
               return typeof(double);

            case "float":
               return typeof(float);

            case "QName":
               return typeof(QName);

            case "short":
               return typeof(short);

            case "unsignedByte":
               return typeof(byte);

            case "unsignedShort":
               return typeof(ushort);

            case "unsignedInt":
               return typeof(uint);

            case "unsignedLong":
               return typeof(ulong);

            case "dateTime":
            case "date":
            case "string":
            default:
               return typeof(string);
         }
      }

      static CodeExpression GetItemFactoryReference(CodeExpression processorRef) {

         return new CodePropertyReferenceExpression {
            PropertyName = "ItemFactory",
            TargetObject = processorRef
         };
      }
   }
}
