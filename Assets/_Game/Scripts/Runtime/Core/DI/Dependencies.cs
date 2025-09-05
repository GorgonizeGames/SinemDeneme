using System.Reflection;
using UnityEngine;

namespace Game.Runtime.Core.DI
{
    public static class Dependencies
    {
        private static DIContainer _container;
        public static DIContainer Container => _container ?? (_container = new DIContainer());

        public static void Inject(MonoBehaviour obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                if (injectAttr != null)
                {
                    var resolveMethod = typeof(DIContainer).GetMethod("Resolve").MakeGenericMethod(field.FieldType);
                    var service = resolveMethod.Invoke(Container, null);

                    field.SetValue(obj, service);
                }
            }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class InjectAttribute : System.Attribute { }
}
