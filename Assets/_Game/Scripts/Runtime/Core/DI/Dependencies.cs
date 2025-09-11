using System;
using System.Reflection;
using UnityEngine;

namespace Game.Runtime.Core.DI
{
public static class Dependencies
    {
        private static readonly object _lock = new object();
        private static DIContainer _container;
        
        public static DIContainer Container
        {
            get
            {
                if (_container == null)
                {
                    lock (_lock)
                    {
                        if (_container == null)
                        {
                            _container = new DIContainer();
                        }
                    }
                }
                return _container;
            }
        }

        public static void Inject(UnityEngine.MonoBehaviour obj)
        {
            if (obj == null)
            {
                UnityEngine.Debug.LogError("Cannot inject dependencies into null object");
                return;
            }

            try
            {
                var fields = obj.GetType().GetFields(
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);

                foreach (var field in fields)
                {
                    var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr != null)
                    {
                        try
                        {
                            var resolveMethod = typeof(DIContainer).GetMethod("Resolve")?.MakeGenericMethod(field.FieldType);
                            if (resolveMethod != null)
                            {
                                var service = resolveMethod.Invoke(Container, null);
                                field.SetValue(obj, service);
                            }
                        }
                        catch (System.Reflection.TargetInvocationException e)
                        {
                            // Extract the inner exception for better error reporting
                            var innerException = e.InnerException ?? e;
                            UnityEngine.Debug.LogError($"Failed to inject dependency for field '{field.Name}' in '{obj.GetType().Name}': {innerException.Message}", obj);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogError($"Failed to inject dependency for field '{field.Name}' in '{obj.GetType().Name}': {e.Message}", obj);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to inject dependencies into '{obj.GetType().Name}': {e.Message}", obj);
            }
        }

        // Convenience method for safe dependency resolution
        public static bool TryInject(UnityEngine.MonoBehaviour obj)
        {
            try
            {
                Inject(obj);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class InjectAttribute : System.Attribute 
    {
        public bool Required { get; set; } = true;
        
        public InjectAttribute(bool required = true)
        {
            Required = required;
        }
    }
}

