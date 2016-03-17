﻿using CodeStatistics.Input;
using System.Collections.Generic;
using CodeStatistics.Handling.Languages.Java.Elements;
using CodeStatistics.Handling.Languages.Java.Utils;

namespace CodeStatistics.Handling.Languages.Java{
    class JavaState{
        private readonly Dictionary<File,JavaFileInfo> fileInfo = new Dictionary<File,JavaFileInfo>();
        private readonly HashSet<string> packages = new HashSet<string>();

        public readonly JavaGlobalInfo GlobalInfo = new JavaGlobalInfo();

        public int PackageCount { get { return packages.Count; } }

        public JavaFileInfo GetFile(File file){
            return fileInfo[file];
        }

        public JavaFileInfo Process(File file){
            JavaFileInfo info = new JavaFileInfo();
            fileInfo.Add(file,info);

            JavaCodeParser parser = new JavaCodeParser(JavaParseUtils.PrepareCodeFile(file.Contents));
            parser.AnnotationCallback += IncrementAnnotation;
            parser.CodeBlockCallback += blockParser => ReadCodeBlock(blockParser,GlobalInfo);

            ReadPackage(parser,info);
            ReadImportList(parser,info);
            ReadTopLevelTypes(parser,info);

            UpdateLocalData(info);

            return info;
        }

        private void UpdateLocalData(JavaFileInfo info){
            packages.Add(info.Package);

            foreach(Type type in info.Types){
                UpdateLocalDataForType(type);
            }
        }

        private void UpdateLocalDataForType(Type type){
            foreach(Type nestedType in type.NestedTypes){
                UpdateLocalDataForType(nestedType);
            }

            foreach(Field field in type.GetData().Fields){
                GlobalInfo.FieldTypes.Increment(field.Type.ToStringGeneral());
            }

            foreach(Method method in type.GetData().Methods){
                GlobalInfo.MethodReturnTypes.Increment(method.ReturnType.ToStringGeneral());

                foreach(TypeOf parameterType in method.ParameterTypes){
                    GlobalInfo.MethodParameterTypes.Increment(parameterType.ToStringGeneral());
                }
            }
        }

        private void IncrementAnnotation(Annotation annotation){
            GlobalInfo.AnnotationUses.Increment(annotation.SimpleName);
        }

        private static void ReadPackage(JavaCodeParser parser, JavaFileInfo info){
            parser.SkipSpaces();
            parser.SkipReadAnnotationList();
            parser.SkipSpaces();

            info.Package = parser.ReadPackageDeclaration();
        }

        private static void ReadImportList(JavaCodeParser parser, JavaFileInfo info){
            while(true){
                parser.SkipSpaces();

                Import? import = parser.ReadImportDeclaration();
                if (!import.HasValue)break;

                info.Imports.Add(import.Value);
            }
        }

        private static void ReadTopLevelTypes(JavaCodeParser parser, JavaFileInfo info){
            while(true){
                parser.SkipSpaces();
                if (parser.IsEOF)break;

                Type type = parser.ReadType();

                if (type != null)info.Types.Add(type);
                else break;
            }
        }

        private static void ReadCodeBlock(JavaCodeBlockParser blockParser, JavaGlobalInfo info){
            string keyword;
            
            while((keyword = blockParser.ReadNextKeywordSkip()).Length > 0){
                switch(keyword){
                    case "do":
                        ReadCodeBlock((JavaCodeBlockParser)blockParser.ReadBlock('{','}'),info);

                        if (blockParser.ReadNextKeywordSkip() != "while"){
                            blockParser.RevertKeywordSkip(); // should not happen
                        }

                        ++info.Statements[FlowStatement.DoWhile];
                        break;

                    case "while":
                        ++info.Statements[FlowStatement.While];
                        break;

                    case "for":
                        JavaCodeBlockParser cycleDeclBlock = (JavaCodeBlockParser)blockParser.ReadBlock('(',')');
                        ReadCodeBlock(cycleDeclBlock,info);
                        
                        JavaCodeParser cycleDeclParser = new JavaCodeParser(cycleDeclBlock.Contents);
                        cycleDeclParser.ReadTypeOf(false);
                        cycleDeclParser.SkipSpaces().ReadIdentifier();

                        ++info.Statements[cycleDeclParser.SkipSpaces().Char == ':' ? FlowStatement.EnhancedFor : FlowStatement.For];
                        break;
                }
            }
        }
    }
}
