using UnityEngine;
using Game.Runtime.Core.DI;

namespace Game.Runtime.Core.Extensions
{
    public static class MonoBehaviourExtensions
    {
        public static void InjectDependencies(this MonoBehaviour obj)
        {
            Dependencies.Inject(obj);
        }
    }
}