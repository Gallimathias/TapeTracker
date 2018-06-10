using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TestConverter
{
    public class DbTypeBuilder
    {
        public Type CreateType(string typeName, Type parent, params Field[] fields)
        {
            TypeBuilder tb = GetTypeBuilder(typeName, parent);
            //var baseConstructor = parent?.GetConstructors()[0];
            ConstructorBuilder constructor;

            //if (baseConstructor == null || baseConstructor?.GetParameters().Length < 1)
            //{
            constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            //}
            //else
            //{
            //    constructor = tb.DefineConstructor(
            //         MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            //         CallingConventions.Standard,
            //         baseConstructor.GetParameters().Select(p => p.ParameterType).ToArray());
            //    var generator = constructor.GetILGenerator();
            //    generator.Emit(OpCodes.Ldarg_0);
            //    generator.Emit(OpCodes.Ldarg_1);
            //    generator.Emit(OpCodes.Call, baseConstructor);
            //    generator.Emit(OpCodes.Nop);
            //    generator.Emit(OpCodes.Nop);
            //    generator.Emit(OpCodes.Ret);
            //}


            // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
            foreach (var field in fields)
                CreateProperty(tb, field.FieldName, field.FieldType);

            return tb.CreateType();
        }
        public Type CreateType(string typeName, params Field[] fields)
            => CreateType(typeName, null, fields);
        public Type CreateType(string name, Type baseType, params Type[] types)
            => CreateType(name, baseType, types.Select(t => new Field(t.Name, t)).ToArray());
        public Type CreateType(SimpleTable table)
            => CreateType(table.Name, table.Columns.Select(c => new Field(c.Key, c.Value)).ToArray());

        private TypeBuilder GetTypeBuilder(string signature, Type parent = null)
        {
            var typeSignature = signature;
            var an = new AssemblyName(typeSignature);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    parent);
            return tb;
        }

        private void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        public class Field
        {

            public Field(string name, Type type)
            {
                FieldName = name;
                FieldType = type;
            }

            public string FieldName { get; set; }
            public Type FieldType { get; set; }
        }
    }
}
