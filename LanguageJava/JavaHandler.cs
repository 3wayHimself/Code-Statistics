﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeStatisticsCore.Collections;
using CodeStatisticsCore.Handling;
using CodeStatisticsCore.Handling.Files;
using CodeStatisticsCore.Handling.Utils;
using LanguageJava.Elements;
using LanguageJava.Handling;
using LanguageJava.Utils;
using File = CodeStatisticsCore.Input.File;
using Type = LanguageJava.Elements.Type;

namespace LanguageJava{
    public class JavaHandler : AbstractLanguageFileHandler{
        public override int Weight{
            get { return 50; }
        }

        protected override string Key{
            get { return "java"; }
        }

        public override void SetupProject(Variables.Root variables){
            base.SetupProject(variables);
            variables.AddStateObject(this, new JavaState());
        }

        public override void Process(File file, Variables.Root variables){
            base.Process(file, variables);

            JavaState state = variables.GetStateObject<JavaState>(this);
            JavaFileInfo info = state.Process(file);

            variables.Increment("javaImportsTotal", info.Imports.Count);
            variables.Maximum("javaImportsMax", info.Imports.Count);

            foreach(Type type in info.Types){
                ProcessType(info.Package, type, variables, false);
            }
        }

        private void ProcessType(string prefix, Type type, Variables.Root variables, bool isNested){
            foreach(Type nestedType in type.NestedTypes){
                ProcessType(prefix+"."+type.Identifier, nestedType, variables, true);
            }

            // declaration type
            variables.Increment("javaTypesTotal");

            string declPrefix = "javaUnknown";

            switch(type.Declaration){
                case Type.DeclarationType.Class: variables.Increment(declPrefix = "javaClasses"); break;
                case Type.DeclarationType.Interface: variables.Increment(declPrefix = "javaInterfaces"); break;
                case Type.DeclarationType.Enum: variables.Increment(declPrefix = "javaEnums"); break;
                case Type.DeclarationType.Annotation: variables.Increment(declPrefix = "javaAnnotations"); break;
            }

            if (isNested){
                variables.Increment(declPrefix+(type.Modifiers.HasFlag(Modifiers.Static) ? "NestedStatic" : "NestedInner"));
            }

            foreach(Modifiers modifier in JavaModifiers.Values.Where(modifier => type.Modifiers.HasFlag(modifier))){
                variables.Increment(declPrefix+JavaModifiers.ToString(modifier).CapitalizeFirst());
            }
            
            // identifier
            TypeIdentifier identifier = new TypeIdentifier(prefix, type.Identifier);

            int simpleNameLength = identifier.Name.Length;
            variables.Increment("javaNamesSimpleTotal", simpleNameLength);
            variables.Minimum("javaNamesSimpleMin", simpleNameLength);
            variables.Maximum("javaNamesSimpleMax", simpleNameLength);

            int fullLength = identifier.FullName.Length;
            variables.Increment("javaNamesFullTotal", fullLength);
            variables.Minimum("javaNamesFullMin", fullLength);
            variables.Maximum("javaNamesFullMax", fullLength);

            JavaGlobalInfo global = variables.GetStateObject<JavaState>(this).GlobalInfo;
            global.IdentifiersSimpleTop.Add(identifier);
            global.IdentifiersSimpleBottom.Add(identifier);
            global.IdentifiersFullTop.Add(identifier);
            global.IdentifiersFullBottom.Add(identifier);

            if (type.Declaration == Type.DeclarationType.Annotation){
                // annotation elements
                int methodCount = type.GetData().Methods.Count;
                variables.Increment("javaAnnotationsElementsTotal", methodCount);
                variables.Minimum("javaAnnotationsElementsMin", methodCount);
                variables.Maximum("javaAnnotationsElementsMax", methodCount);
            }
            else{
                // fields
                List<Field> fields = type.GetData().Fields;
                int fieldsDefault = fields.Count;

                variables.Increment(declPrefix+"FieldsTotal", fields.Count);
                variables.Minimum(declPrefix+"FieldsMin", fields.Count);
                variables.Maximum(declPrefix+"FieldsMax", fields.Count);

                foreach(Modifiers modifier in JavaModifiers.Values){
                    int count = fields.Count(field => field.Modifiers.HasFlag(modifier));
                    if (modifier == Modifiers.Public || modifier == Modifiers.Protected || modifier == Modifiers.Private)fieldsDefault -= count;

                    variables.Increment(declPrefix+"Fields"+JavaModifiers.ToString(modifier).CapitalizeFirst(), count);
                }

                variables.Increment(declPrefix+"FieldsDefaultVisibility", fieldsDefault);

                // methods
                List<Method> constructorMethods = type.GetData().Methods.Where(method => method.IsConstructor).ToList();
                List<Method> classMethods = type.GetData().Methods.Where(method => !method.IsConstructor).ToList();

                int methodsDefault = classMethods.Count;
            
                if (type.GetData().CanHaveConstructors){
                    variables.Increment(declPrefix+"ConstructorsTotal", Math.Max(1, constructorMethods.Count)); // if 0, count an implicit constructor
                }

                variables.Increment(declPrefix+"MethodsTotal", classMethods.Count);
                variables.Minimum(declPrefix+"MethodsMin", classMethods.Count);
                variables.Maximum(declPrefix+"MethodsMax", classMethods.Count);

                foreach(Modifiers modifier in JavaModifiers.Values){
                    int count = classMethods.Count(method => method.Modifiers.HasFlag(modifier));
                    if (modifier == Modifiers.Public || modifier == Modifiers.Protected || modifier == Modifiers.Protected)methodsDefault -= count;

                    variables.Increment(declPrefix+"Methods"+JavaModifiers.ToString(modifier).CapitalizeFirst(), count);
                }

                variables.Increment(declPrefix+"MethodsDefaultVisibility", methodsDefault);
            }

            // enums
            if (type.Declaration == Type.DeclarationType.Enum){
                Type.DataEnum enumData = type.GetData<Type.DataEnum>();
                
                variables.Increment("javaEnumsValuesTotal", enumData.EnumValues.Count);
                variables.Minimum("javaEnumsValuesMin", enumData.EnumValues.Count);
                variables.Maximum("javaEnumsValuesMax", enumData.EnumValues.Count);
            }

            // annotations
            variables.Increment("javaAnnotationsUsedClasses", type.Annotations.Count);

            foreach(Field field in type.GetData().Fields){
                variables.Increment("javaAnnotationsUsedFields", field.Annotations.Count);
            }

            foreach(Method method in type.GetData().Methods){
                variables.Increment("javaAnnotationsUsedMethods", method.Annotations.Count);
            }

            // field and method counting
            variables.Increment("javaFieldsTotal", type.GetData().Fields.Count);
            variables.Increment("javaMethodsTotal", type.GetData().Methods.Count);

            foreach(Method method in type.GetData().Methods){
                variables.Increment("javaMethodParametersTotal", method.ParameterTypes.Count);
                variables.Minimum("javaMethodParametersMin", method.ParameterTypes.Count);
                variables.Maximum("javaMethodParametersMax", method.ParameterTypes.Count);
            }
        }

        public override void FinalizeProject(Variables.Root variables){
            base.FinalizeProject(variables);

            JavaState state = variables.GetStateObject<JavaState>(this);
            JavaGlobalInfo global = state.GlobalInfo;

            // general
            variables.SetVariable("javaPackages", state.PackageCount);

            // imports
            variables.Average("javaImportsAvg", "javaImportsTotal", "javaCodeFiles");
            variables.Average("javaNamesSimpleAvg", "javaNamesSimpleTotal", "javaTypesTotal");
            variables.Average("javaNamesFullAvg", "javaNamesFullTotal", "javaTypesTotal");

            // identifiers
            foreach(TypeIdentifier identifier in global.IdentifiersSimpleTop){
                variables.AddToArray("javaNamesSimpleTop", new { package = identifier.Prefix, name = identifier.Name });
            }

            foreach(TypeIdentifier identifier in global.IdentifiersSimpleBottom.Reverse()){
                variables.AddToArray("javaNamesSimpleBottom", new { package = identifier.Prefix, name = identifier.Name });
            }

            foreach(TypeIdentifier identifier in global.IdentifiersFullTop){
                variables.AddToArray("javaNamesFullTop", new { package = identifier.Prefix, name = identifier.Name });
            }

            foreach(TypeIdentifier identifier in global.IdentifiersFullBottom.Reverse()){
                variables.AddToArray("javaNamesFullBottom", new { package = identifier.Prefix, name = identifier.Name });
            }

            // fields and methods
            variables.Average("javaClassesFieldsAvg", "javaClassesFieldsTotal", "javaClasses");
            variables.Average("javaClassesConstructorsAvg", "javaClassesConstructorsTotal", "javaClasses");
            variables.Average("javaClassesMethodsAvg", "javaClassesMethodsTotal", "javaClasses");
            
            variables.Average("javaInterfacesFieldsAvg", "javaInterfacesFieldsTotal", "javaInterfaces");
            variables.Average("javaInterfacesMethodsAvg", "javaInterfacesMethodsTotal", "javaInterfaces");

            variables.Average("javaAnnotationsElementsAvg", "javaAnnotationsElementsTotal", "javaAnnotations");

            variables.Average("javaMethodParametersAvg", "javaMethodParametersTotal", "javaMethodsTotal");

            variables.SetVariable("javaMethodsReturnsTotal", state.GlobalInfo.Statements[FlowStatement.Return]);

            foreach(KeyValuePair<string, int> fieldType in global.FieldTypes.ListFromTop()){
                variables.AddToArray("javaFieldTypes", new { type = fieldType.Key, amount = fieldType.Value });
            }

            foreach(KeyValuePair<string, int> methodReturnType in global.MethodReturnTypes.ListFromTop()){
                variables.AddToArray("javaMethodReturnTypes", new { type = methodReturnType.Key, amount = methodReturnType.Value });
            }

            foreach(KeyValuePair<string, int> methodParameterType in global.MethodParameterTypes.ListFromTop()){
                variables.AddToArray("javaMethodParameterTypes", new { type = methodParameterType.Key, amount = methodParameterType.Value });
            }

            // enums
            variables.Average("javaEnumsValuesAvg", "javaEnumsValuesTotal", "javaEnums");

            // annotations
            List<KeyValuePair<string, int>> annotationUses = global.AnnotationUses.ListFromTop();
            int totalAnnotationsUsed = annotationUses.Sum(kvp => kvp.Value);

            for(int annotationIndex = 0; annotationIndex < Math.Min(annotationUses.Count, 10); annotationIndex++){
                variables.AddToArray("javaAnnotationsUsedTop", new { name = annotationUses[annotationIndex].Key, amount = annotationUses[annotationIndex].Value });
            }

            variables.SetVariable("javaAnnotationsUsedTotal", totalAnnotationsUsed);
            variables.SetVariable("javaAnnotationsUsedOther", totalAnnotationsUsed-variables.GetVariable("javaAnnotationsUsedClasses", 0)-variables.GetVariable("javaAnnotationsUsedFields", 0)-variables.GetVariable("javaAnnotationsUsedMethods", 0));

            // control flow
            variables.SetVariable("javaControlFlowIf", state.GlobalInfo.Statements[FlowStatement.If]+state.GlobalInfo.Statements[FlowStatement.Else]);

            variables.SetVariable("javaControlFlowFor", state.GlobalInfo.Statements[FlowStatement.For]);
            variables.SetVariable("javaControlFlowEnhFor", state.GlobalInfo.Statements[FlowStatement.EnhancedFor]);
            variables.SetVariable("javaControlFlowWhile", state.GlobalInfo.Statements[FlowStatement.While]);
            variables.SetVariable("javaControlFlowDoWhile", state.GlobalInfo.Statements[FlowStatement.DoWhile]);

            variables.SetVariable("javaControlFlowSwitch", state.GlobalInfo.Statements[FlowStatement.Switch]);
            variables.SetVariable("javaControlFlowCaseTotal", state.GlobalInfo.Statements[FlowStatement.SwitchCase]);
            variables.SetVariable("javaControlFlowCaseDefaultTotal", state.GlobalInfo.Statements[FlowStatement.SwitchDefault]);

            variables.SetVariable("javaControlFlowCaseMin", state.GlobalInfo.MinSwitchCases);
            variables.SetVariable("javaControlFlowCaseMax", state.GlobalInfo.MaxSwitchCases);
            variables.Average("javaControlFlowCaseAvg", "javaControlFlowCaseTotal", "javaControlFlowSwitch");
            variables.Percent("javaControlFlowCaseDefaultPerc", "javaControlFlowCaseDefaultTotal", "javaControlFlowSwitch");
            
            variables.SetVariable("javaControlFlowTry", state.GlobalInfo.Statements[FlowStatement.Try]);
            variables.SetVariable("javaControlFlowTryCatch", state.GlobalInfo.Statements[FlowStatement.TryCatch]);
            variables.SetVariable("javaControlFlowTryWithResources", state.GlobalInfo.Statements[FlowStatement.TryWithResources]);
            variables.SetVariable("javaControlFlowTryCatchFinally", state.GlobalInfo.TryCatchWithFinally);
            variables.SetVariable("javaControlFlowTryWithResourcesFinally", state.GlobalInfo.TryWithResourcesWithFinally);
            variables.Percent("javaControlFlowTryCatchFinallyPerc", "javaControlFlowTryCatchFinally", "javaControlFlowTry");
            variables.Percent("javaControlFlowTryWithResourcesFinallyPerc", "javaControlFlowTryWithResourcesFinally", "javaControlFlowTry");

            variables.SetVariable("javaControlFlowCatchTotal", state.GlobalInfo.Statements[FlowStatement.Catch]);
            variables.SetVariable("javaControlFlowCatchMin", state.GlobalInfo.MinCatchBlocks);
            variables.SetVariable("javaControlFlowCatchMax", state.GlobalInfo.MaxCatchBlocks);
            variables.Average("javaControlFlowCatchAvg", "javaControlFlowCatchTotal", "javaControlFlowTry");
        }

        protected override object GetFileObject(FileIntValue fi, Variables.Root variables){
            JavaState state = variables.GetStateObject<JavaState>(this);
            return new { package = state.GetFile(fi.File).Package.Replace('.', '/')+'/', file = Path.GetFileName(fi.File.FullPath), amount = fi.Value };
        }

        public override string PrepareFileContents(string contents){
            return JavaParseUtils.PrepareCodeFile(contents);
        }

        public override IEnumerable<Node> GenerateTreeViewData(Variables.Root variables, File file){
            JavaFileInfo info = variables.GetStateObject<JavaState>(this).GetFile(file);

            foreach(Type type in info.Types){
                yield return GenerateTreeViewDataForType(info, type);
            }
        }

        private static Node GenerateTreeViewDataForType(JavaFileInfo fileInfo, Type type){
            Type.TypeData data = type.GetData();
            Type.DataEnum dataEnum = data as Type.DataEnum;

            Node node = new Node(type.ToString());

            if (fileInfo != null){
                Node packageNode = new Node("[Package]");
                packageNode.Add(fileInfo.Package);
                node.Add(packageNode);

                Node importNode = new Node("[Imports]");
                importNode.AddRangeAsStrings(fileInfo.Imports);
                node.Add(importNode);
            }

            if (data.Fields.Count > 0){
                Node fieldNode = new Node("[Fields]");
                fieldNode.AddRangeAsStrings(data.Fields);
                node.Add(fieldNode);
            }

            if (data.Methods.Count > 0){
                Node methodNode = new Node("[Methods]");
                methodNode.AddRangeAsStrings(data.Methods);
                node.Add(methodNode);
            }
            
            if (type.NestedTypes.Count > 0){
                Node nestedNode = new Node("[Nested Types]");
                nestedNode.AddRange(type.NestedTypes.Select(nestedType => GenerateTreeViewDataForType(null, nestedType)));
                node.Add(nestedNode);
            }

            if (dataEnum != null){
                Node enumNode = new Node("[Enum Values]");
                enumNode.AddRange(dataEnum.EnumValues);
                node.Add(enumNode);
            }

            return node;
        }
    }
}
