/// <summary>
/// SURGE FRAMEWORK
/// Author: Bob Berkebile
/// Email: bobb@pixelplacement.com
/// 
/// Utilizes script execution order to run before anything else to avoid order of operation failures so accessing things like singletons at any stage of application startup will never fail.
/// 
/// </summary>

using UnityEngine;
using System.Reflection;

namespace Architecture
{
    public class Initialization : MonoBehaviour
    {

        //Init:
        void Awake()
        {
            //singleton initialization:
            InitializeSingleton(); 
        }

        //Private Methods:
        void InitializeSingleton()
        {
            foreach (Component item in GetComponents<Component>())
            {
                string baseType;

#if NETFX_CORE
                baseType = item.GetType ().GetTypeInfo ().BaseType.ToString ();
#else
                baseType = item.GetType().BaseType.ToString();
#endif

                if (baseType.Contains("Singleton"))
                {
                    MethodInfo m;

#if NETFX_CORE
                    m = item.GetType ().GetTypeInfo ().BaseType.GetMethod ("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
#else
                    m = item.GetType().BaseType.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

                    m.Invoke(item, new Component[] { item });
                }
            }
        }
    }
}