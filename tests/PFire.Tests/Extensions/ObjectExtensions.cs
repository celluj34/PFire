using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PFire.Tests.Extensions
{
    internal static class ObjectExtensions
    {
        public static T InvokePrivateMethod<T>(this object objectToInvoke, string methodName, params object[] parameters)
        {
            return objectToInvoke.InvokePrivateMethod<T>(methodName, parameters, parameters.Select(x => x.GetType()));
        }

        public static T InvokePrivateMethod<T>(this object objectToInvoke, string methodName, IEnumerable<object> parameters, IEnumerable<Type> parameterTypes)
        {
            var privateMethod = objectToInvoke.GetPrivateMethod(methodName, parameterTypes);
            try
            {
                return (T)privateMethod.Invoke(objectToInvoke, parameters.ToArray());
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public static void InvokePrivateMethod(this object objectToInvoke, string methodName, params object[] parameters)
        {
            objectToInvoke.InvokePrivateMethod<object>(methodName, parameters);
        }

        public static void InvokePrivateMethod(this object objectToInvoke, string methodName, IEnumerable<object> parameters, IEnumerable<Type> parameterTypes)
        {
            objectToInvoke.InvokePrivateMethod<object>(methodName, parameters, parameterTypes);
        }

        public static T GetPrivatePropertyValue<T>(this object objectToInvoke, string propertyName)
        {
            var property = objectToInvoke.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null)
            {
                throw new MissingMemberException($"Type {objectToInvoke.GetType().Name} does not have private property '{propertyName}'.");
            }

            return (T)property.GetValue(objectToInvoke, null);
        }

        public static T GetPrivateFieldValue<T>(this object objectToInvoke, string fieldName)
        {
            var type = objectToInvoke.GetType();
            FieldInfo fieldInfo;
            for (fieldInfo = null; fieldInfo == null && type != null; type = type.BaseType)
            {
                fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (fieldInfo == null)
            {
                throw new MissingMemberException($"Type {objectToInvoke.GetType().Name} does not have private field '{fieldName}'.");
            }

            return (T)fieldInfo.GetValue(objectToInvoke);
        }

        private static MethodInfo GetPrivateMethod(this object objectToInvoke, string methodName, IEnumerable<Type> parameters)
        {
            var method = objectToInvoke.GetType()
                                       .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic, Type.DefaultBinder, parameters.ToArray(), null);

            if (method == null)
            {
                throw new MissingMemberException($"Type {objectToInvoke.GetType().Name} does not have private method '{methodName}'.");
            }

            return method;
        }
    }
}
