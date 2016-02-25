﻿using CodeStatistics.Handling.Languages.Java.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CodeStatistics.Handling.Languages.Java.Elements{
    public struct TypeOf{
        private static readonly ReadOnlyCollection<TypeOf> EmptyCollection = new List<TypeOf>().AsReadOnly();

        public static TypeOf Primitive(Primitives primitive){
            return new TypeOf(primitive);
        }

        public static TypeOf Object(string identifier){
            return new TypeOf(identifier);
        }

        public static TypeOf Void(){
            return new TypeOf(null);
        }

        public static ReadOnlyCollection<TypeOf> EmptyList(){
            return EmptyCollection;
        }

        public bool IsVoid { get { return obj == null; } }
        public bool IsPrimitive { get { return obj is Primitives; } }
        public bool IsTypeObject { get { return !IsVoid && !IsPrimitive; } }

        private readonly object obj;

        private TypeOf(object obj){
            this.obj = obj;
        }

        public Primitives? AsPrimitive(){
            if (IsPrimitive)return (Primitives)obj;
            else return null;
        }

        public string AsSimpleType(){
            if (IsTypeObject)return JavaParseUtils.FullToSimpleName((string)obj);
            else return null;
        }

        public override string ToString(){
            if (IsVoid)return "void";
            else if (IsPrimitive)return JavaPrimitives.ToString((Primitives)obj);
            else return obj.ToString();
        }
    }
}