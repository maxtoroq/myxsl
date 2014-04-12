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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.XPath;
using Microsoft.CSharp;
using myxsl.net.common;

namespace myxsl.net.system {

   static class ExtensionObjectGenerator {

      static int dynamicAssemblyIndex;

      public static Type[] Generate(IEnumerable<XPathModuleInfo> modules) {

         bool debuggerIsAttached = Debugger.IsAttached;

         var nspace = new CodeNamespace {
            Name = typeof(ExtensionObjectGenerator).Namespace + ".modules"
         };

         var generatedTypeNames = new List<string>();

         var compilerParams = new CompilerParameters {
            GenerateInMemory = !debuggerIsAttached,
            ReferencedAssemblies = { 
               // System
               typeof(Uri).Assembly.Location
            }
         };

         foreach (XPathModuleInfo module in modules) {

            CodeTypeDeclaration typeDecl = GenerateType(module);
            nspace.Types.Add(typeDecl);

            generatedTypeNames.Add(nspace.Name + "." + typeDecl.Name);

            AddAssemblyReference(compilerParams, module.Type.Assembly);

            foreach (XPathFunctionInfo function in module.Functions) {
               AddAssemblyReference(compilerParams, function.ReturnType.ClrType.Assembly);

               foreach (XPathVariableInfo param in function.Parameters) {
                  AddAssemblyReference(compilerParams, param.Type.ClrType.Assembly);
               }
            }
         }

         var compileUnit = new CodeCompileUnit {
            Namespaces = { nspace }
         };

         CodeDomProvider provider = new CSharpCodeProvider();
         CompilerResults result = provider.CompileAssemblyFromDom(compilerParams, compileUnit);

         CompilerError firstError = result.Errors.Cast<CompilerError>().FirstOrDefault(e => !e.IsWarning);

         if (firstError != null) {

            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture)) {
               
               provider.GenerateCodeFromCompileUnit(compileUnit, stringWriter, null);

               if (debuggerIsAttached) {
                  Debugger.Log(0, Debugger.DefaultCategory, "Extension object code generation error:" + Environment.NewLine);
                  Debugger.Log(0, Debugger.DefaultCategory, stringWriter.ToString());
               } 
            }

            throw new ArgumentException(firstError.ErrorText, "modules");
         }

         if (debuggerIsAttached) 
            Debugger.Log(0, Debugger.DefaultCategory, "Extension object(s) generated {0}".FormatInvariant(result.PathToAssembly) + Environment.NewLine);

         return RenameMethodsIfNecessary(generatedTypeNames
               .Select(n => result.CompiledAssembly.GetType(n, throwOnError: true))
            ).ToArray();
      }

      static void AddAssemblyReference(CompilerParameters compilerParams, Assembly assembly) {

         string location = assembly.Location;
         
         if (location.HasValue()
            && !compilerParams.ReferencedAssemblies.Contains(location)) {
            compilerParams.ReferencedAssemblies.Add(location);
         }
      }

      static CodeTypeDeclaration GenerateType(XPathModuleInfo module) {

         var type = new CodeTypeDeclaration {
            Name = module.Type.FullName.Replace('.', '_') + "_extobj",
            IsClass = true,
            TypeAttributes = TypeAttributes.Public,
            CustomAttributes = { 
               new CodeAttributeDeclaration(
                  new CodeTypeReference(typeof(DebuggerNonUserCodeAttribute))
               )
            }
         };

         CodeExpression moduleExpr;

         if (module.TypeIsStatic) {
            moduleExpr = new CodeTypeReferenceExpression(module.Type);
         } else {

            var moduleField = new CodeMemberField {
               Name = "module",
               Type = new CodeTypeReference(module.Type),
               InitExpression = new CodeObjectCreateExpression(module.Type)
            };

            type.Members.Add(moduleField);

            moduleExpr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), moduleField.Name);

            if (module.Dependencies.Count > 0) {
               type.Members.Add(GenerateInitialize(module, moduleExpr));
            }
         }

         foreach (XPathFunctionInfo function in module.Functions) {
            type.Members.Add(GenerateFunction(function, moduleExpr));
         }

         return type;
      }

      static CodeMemberMethod GenerateInitialize(XPathModuleInfo module, CodeExpression moduleExpr) {

         var initializeMethod = new CodeMemberMethod { 
            Name = "Initialize",
            Attributes = MemberAttributes.Public | MemberAttributes.Final
         };

         for (int i = 0; i < module.Dependencies.Count; i++) {
            XPathDependencyInfo dependency = module.Dependencies[i];

            initializeMethod.Parameters.Add(new CodeParameterDeclarationExpression {
               Type = new CodeTypeReference(dependency.Type),
               Name = "d" + (i + 1).ToStringInvariant()
            });

            var paramRef = new CodeVariableReferenceExpression(initializeMethod.Parameters[i].Name);

            initializeMethod.Statements.Add(new CodeAssignStatement { 
               Left = new CodePropertyReferenceExpression(moduleExpr, dependency.Property.Name),
               Right = paramRef
            });
         }

         return initializeMethod;
      }

      static CodeMemberMethod GenerateFunction(XPathFunctionInfo function, CodeExpression moduleExpr) {

         string name = function.Name;

         MemberAttributes methodAttributes = MemberAttributes.Public;

         if (name.Contains('-')) {
            name = name.Replace('-', '_');
            methodAttributes = MemberAttributes.Family;
         }

         methodAttributes |= MemberAttributes.Final;

         Type procReturnType = GetReturnType(function.ReturnType);

         var codeMethod = new CodeMemberMethod {
            Name = name,
            Attributes = methodAttributes,
            ReturnType = new CodeTypeReference(procReturnType),
         };

         var methodInvoke = new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = function.Method.Name,
               TargetObject = moduleExpr
            }
         };

         for (int i = 0; i < function.Parameters.Count; i++) {
            
            XPathSequenceType paramTypeInfo = function.Parameters[i].Type;

            Type procParamType = GetParameterType(paramTypeInfo);

            var paramDecl = new CodeParameterDeclarationExpression { 
               Name = "p" + i.ToStringInvariant(),
               Type = new CodeTypeReference(procParamType)
            };

            codeMethod.Parameters.Add(paramDecl);

            var paramVarExpr = new CodeVariableReferenceExpression(paramDecl.Name);
            CodeExpression argExpr = GetArgumentExpression(paramTypeInfo, procParamType, paramVarExpr);

            methodInvoke.Parameters.Add(argExpr);
         }

         CodeVariableReferenceExpression resultVarExpr = null;

         if (function.ReturnType.ClrType == typeof(void)) {
            codeMethod.Statements.Add(new CodeExpressionStatement(methodInvoke));

         } else {

            var resultVarDecl = new CodeVariableDeclarationStatement {
               Name = "result",
               InitExpression = methodInvoke,
               Type = new CodeTypeReference(function.Method.ReturnType)
            };

            codeMethod.Statements.Add(resultVarDecl);

            resultVarExpr = new CodeVariableReferenceExpression(resultVarDecl.Name);
         }

         CodeExpression returnExpr = GetReturnExpression(function.ReturnType, resultVarExpr);
         
         codeMethod.Statements.Add(new CodeMethodReturnStatement(returnExpr));

         return codeMethod;
      }

      static Type GetReturnType(XPathSequenceType returnTypeInfo) {

         switch (returnTypeInfo.Cardinality) {
            default:
            case XPathSequenceCardinality.One:

               if (returnTypeInfo.ItemType.KindIsNode) {
                  return typeof(XPathNavigator);
               }
               
               return typeof(object);
            
            case XPathSequenceCardinality.ZeroOrOne:
            case XPathSequenceCardinality.ZeroOrMore:
               return typeof(object);
               
            case XPathSequenceCardinality.OneOrMore:
               return typeof(XPathNavigator[]);
         }
      }

      static CodeExpression GetReturnExpression(XPathSequenceType sequenceType, CodeVariableReferenceExpression varExpr) {

         if (sequenceType.ClrType == typeof(void)) {

            return new CodePropertyReferenceExpression {
               PropertyName = "EmptyIterator",
               TargetObject = new CodeTypeReferenceExpression(typeof(ExtensionObjectConvert))
            };
         }

         string convertMethod = "ToInput";

         if (sequenceType.ItemType.KindIsNode) {
            convertMethod += "Node";
         }

         if (sequenceType.Cardinality == XPathSequenceCardinality.ZeroOrMore
            || sequenceType.Cardinality == XPathSequenceCardinality.ZeroOrOne) {
            
            convertMethod += "OrEmpty";
         }

         CodeExpression returnExpr = new CodeMethodInvokeExpression {
            Method = new CodeMethodReferenceExpression {
               MethodName = convertMethod,
               TargetObject = new CodeTypeReferenceExpression(typeof(ExtensionObjectConvert))
            },
            Parameters = { varExpr }
         };

         if (sequenceType.ItemType.Kind == XPathItemKind.Element
            || sequenceType.ItemType.Kind == XPathItemKind.SchemaElement) {

            returnExpr = new CodeMethodInvokeExpression {
               Method = new CodeMethodReferenceExpression {
                  MethodName = "FirstElementOrSelf",
                  TargetObject = new CodeTypeReferenceExpression(typeof(ExtensionObjectConvert))
               },
               Parameters = { returnExpr }
            };
         }

         return returnExpr;
      }

      static Type GetParameterType(XPathSequenceType paramTypeInfo) {

         switch (paramTypeInfo.Cardinality) {
            case XPathSequenceCardinality.One:

               if (paramTypeInfo.ItemType.Kind == XPathItemKind.AnyItem) {
                  return typeof(object);
               }

               if (paramTypeInfo.ItemType.KindIsNode) {
                  return typeof(XPathNavigator);
               }

               switch (Type.GetTypeCode(paramTypeInfo.ItemType.ClrType)) {

                  default:
                  case TypeCode.String:
                  case TypeCode.Char:
                  case TypeCode.Object:
                     return typeof(string);

                  case TypeCode.Boolean:
                     return typeof(bool);
                     
                  case TypeCode.Byte:
                     return typeof(byte);

                  case TypeCode.DateTime:
                     return typeof(DateTime);
                     
                  case TypeCode.Decimal:
                     return typeof(decimal);
                     
                  case TypeCode.Double:
                     return typeof(double);
                     
                  case TypeCode.Int16:
                     return typeof(Int16);
                     
                  case TypeCode.Int32:
                     return typeof(Int32);
                     
                  case TypeCode.Int64:
                     return typeof(Int64);
                     
                  case TypeCode.SByte:
                     return typeof(SByte);
                     
                  case TypeCode.Single:
                     return typeof(Single);
                  
                  case TypeCode.UInt16:
                     return typeof(UInt16);
                     
                  case TypeCode.UInt32:
                     return typeof(UInt32);

                  case TypeCode.UInt64:
                     return typeof(UInt64);
	            }

            default:
               return typeof(XPathNodeIterator);
         }
      }

      static CodeExpression GetArgumentExpression(XPathSequenceType paramTypeInfo, Type varType, CodeVariableReferenceExpression varExpr) {

         var convertTypeExpr = new CodeTypeReferenceExpression(typeof(ExtensionObjectConvert));

         CodeExpression argExpr = varExpr;

         if (paramTypeInfo.ClrType.IsAssignableFrom(varType)) {
            return argExpr;
         }

         MethodInfo convertMethod = typeof(ExtensionObjectConvert)
            .GetMethod("To" + paramTypeInfo.ClrType.Name, BindingFlags.Public | BindingFlags.Static, null, new[] { varType }, null);

         if (convertMethod != null) {

            return new CodeMethodInvokeExpression {
               Method = new CodeMethodReferenceExpression {
                  MethodName = convertMethod.Name,
                  TargetObject = convertTypeExpr,
               },
               Parameters = { argExpr }
            };
         } 

         MethodInfo convertItemMethod = typeof(ExtensionObjectConvert)
            .GetMethod("To" + paramTypeInfo.ItemType.ClrType.Name, BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(XPathItem) }, null);

         CodeExpression convertItemExpr = (convertItemMethod != null) ?
            new CodeMethodReferenceExpression(convertTypeExpr, convertItemMethod.Name)
            : null;

         if (paramTypeInfo.ClrTypeIsEnumerable) {

            CodeMethodInvokeExpression methodExpr;

            argExpr = methodExpr = new CodeMethodInvokeExpression {
               Method = new CodeMethodReferenceExpression {
                  MethodName = "ToEnumerable",
                  TypeArguments = { paramTypeInfo.ItemType.ClrType },
                  TargetObject = convertTypeExpr
               },
               Parameters = { argExpr }
            };

            if (convertItemExpr != null) {
               methodExpr.Parameters.Add(convertItemExpr);
            }

            if (paramTypeInfo.ClrType.IsArray) {

               argExpr = new CodeMethodInvokeExpression {
                  Method = new CodeMethodReferenceExpression {
                     MethodName = "ToArray",
                     TypeArguments = { paramTypeInfo.ItemType.ClrType },
                     TargetObject = new CodeTypeReferenceExpression(typeof(Enumerable))
                  },
                  Parameters = { argExpr }
               };
            }

         } else if (paramTypeInfo.ClrTypeIsNullableValueType) {

            CodeMethodInvokeExpression methodExpr;

            argExpr = methodExpr = new CodeMethodInvokeExpression {
               Method = new CodeMethodReferenceExpression {
                  MethodName = "ToNullableValueType",
                  TypeArguments = { paramTypeInfo.ItemType.ClrType },
                  TargetObject = convertTypeExpr
               },
               Parameters = { argExpr }
            };

            if (convertItemExpr != null) {
               methodExpr.Parameters.Add(convertItemExpr);
            }

         } else {

            CodeMethodInvokeExpression methodExpr;

            argExpr = methodExpr = new CodeMethodInvokeExpression {
               Method = new CodeMethodReferenceExpression {
                  MethodName = "ToOutput",
                  TypeArguments = { paramTypeInfo.ClrType },
                  TargetObject = convertTypeExpr
               },
               Parameters = { argExpr }
            };

            if (convertItemExpr != null
               && paramTypeInfo.ClrType == paramTypeInfo.ItemType.ClrType) {

               methodExpr.Parameters.Add(convertItemExpr);
            }
         }
         
         return argExpr;
      }

      internal static Type RenameMethodsIfNecessary(Type extensionObjectType) {
         return RenameMethodsIfNecessary(new[] { extensionObjectType }).Single();
      }

      internal static IEnumerable<Type> RenameMethodsIfNecessary(IEnumerable<Type> extensionObjectTypes) {

         AssemblyBuilder asmb = null;
         ModuleBuilder moduleb = null;

         foreach (Type extensionObjectType in extensionObjectTypes) {

            MethodInfo[] methodsToRename = extensionObjectType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance)
               .Where(m => m.Name.Contains('_'))
               .ToArray();

            if (methodsToRename.Length == 0) {
               yield return extensionObjectType;
               continue;
            }

            if (moduleb == null) {
               var name = new AssemblyName(typeof(ExtensionObjectGenerator).Namespace + ".modules.runtime." + (++dynamicAssemblyIndex).ToStringInvariant());
               asmb = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
               moduleb = asmb.DefineDynamicModule(name.FullName); 
            }

            TypeBuilder typeb = moduleb.DefineType("Runtime" + extensionObjectType.Name, TypeAttributes.Class | TypeAttributes.Public, extensionObjectType);

            for (int i = 0; i < methodsToRename.Length; i++)
               RenameMethod(typeb, methodsToRename[i]);

            Type generatedType = typeb.CreateType();

            yield return generatedType;
         }
      }

      static MethodBuilder RenameMethod(TypeBuilder typeb, MethodInfo method) {

         ParameterInfo[] parameters = method.GetParameters();

         MethodBuilder methodb = typeb.DefineMethod(method.Name.Replace('_', '-'), MethodAttributes.Public | MethodAttributes.HideBySig);
         methodb.SetReturnType(method.ReturnType);
         methodb.SetParameters(parameters.Select(p => p.ParameterType).ToArray());

         for (int j = 1; j <= parameters.Length; j++)
            methodb.DefineParameter(j, ParameterAttributes.None, parameters[j - 1].Name);

         ILGenerator ilgen = methodb.GetILGenerator();

         ilgen.DeclareLocal(method.ReturnType);
         Label label11 = ilgen.DefineLabel();

         ilgen.Emit(OpCodes.Nop);
         ilgen.Emit(OpCodes.Ldarg_0);

         for (int j = 1; j <= parameters.Length; j++)
            ilgen.Emit(OpCodes.Ldarg, j);

         ilgen.Emit(OpCodes.Call, method);
         ilgen.Emit(OpCodes.Stloc_0);
         ilgen.Emit(OpCodes.Br_S, label11);
         ilgen.MarkLabel(label11);
         ilgen.Emit(OpCodes.Ldloc_0);
         ilgen.Emit(OpCodes.Ret);

         return methodb;
      }
   }
}
