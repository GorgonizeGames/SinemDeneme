using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Interfaces;

namespace Game.Runtime.Bootstrap
{
    public class AutoServiceRegistration : MonoBehaviour
    {
        [Header("Auto Registration Settings")]
        [SerializeField] private bool enableAutoRegistration = true;
        [SerializeField] private bool logRegistrations = true;
        [SerializeField] private string[] namespaceFilters = { "Game.Runtime" };

        // Manual service registration for when auto-registration isn't suitable
        [Header("Manual Service Registration")]
        [SerializeField] private ServiceReference[] manualServices;

        private readonly Dictionary<Type, object> _registeredServices = new Dictionary<Type, object>();

        [System.Serializable]
        public class ServiceReference
        {
            public string interfaceName;
            public MonoBehaviour implementation;
            public bool required = true;
        }

        void Awake()
        {
            try
            {
                if (enableAutoRegistration)
                {
                    RegisterServicesAutomatically();
                }
                
                RegisterManualServices();
                
                if (logRegistrations)
                {
                    LogRegistrationSummary();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during service registration: {e.Message}", this);
            }
        }

        private void RegisterServicesAutomatically()
        {
            // Use the non-obsolete method
            var serviceComponents = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var component in serviceComponents)
            {
                if (component == null) continue;

                try
                {
                    RegisterServiceInterfaces(component);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error registering service {component.GetType().Name}: {e.Message}", this);
                }
            }
        }

        private void RegisterServiceInterfaces(MonoBehaviour component)
        {
            var componentType = component.GetType();
            
            // Skip if not in filtered namespaces
            if (!IsInAllowedNamespace(componentType.Namespace))
                return;

            var interfaces = componentType.GetInterfaces();
            
            foreach (var interfaceType in interfaces)
            {
                // Skip Unity interfaces and system interfaces
                if (ShouldSkipInterface(interfaceType))
                    continue;

                // Check if it's a service interface (conventionally starts with 'I' and ends with 'Service')
                if (IsServiceInterface(interfaceType))
                {
                    RegisterService(interfaceType, component);
                }
            }
        }

        private bool IsInAllowedNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName)) return false;
            
            foreach (var filter in namespaceFilters)
            {
                if (namespaceName.StartsWith(filter))
                    return true;
            }
            
            return false;
        }

        private bool ShouldSkipInterface(Type interfaceType)
        {
            // Skip Unity interfaces
            if (interfaceType.Namespace != null && 
                (interfaceType.Namespace.StartsWith("UnityEngine") || 
                 interfaceType.Namespace.StartsWith("Unity.")))
                return true;

            // Skip system interfaces
            if (interfaceType.Namespace != null && interfaceType.Namespace.StartsWith("System"))
                return true;

            return false;
        }

        private bool IsServiceInterface(Type interfaceType)
        {
            var name = interfaceType.Name;
            
            // Check for service interface patterns
            return name.StartsWith("I") && 
                   (name.EndsWith("Service") || 
                    name.EndsWith("Manager") || 
                    name.EndsWith("Controller") ||
                    name.EndsWith("Provider"));
        }

        private void RegisterService(Type interfaceType, MonoBehaviour implementation)
        {
            try
            {
                if (!_registeredServices.ContainsKey(interfaceType))
                {
                    Dependencies.Container.Register(interfaceType, implementation);
                    _registeredServices[interfaceType] = implementation;
                    
                    if (logRegistrations)
                    {
                        Debug.Log($"Auto-registered {interfaceType.Name} -> {implementation.GetType().Name}");
                    }
                }
                else if (logRegistrations)
                {
                    Debug.LogWarning($"Service {interfaceType.Name} already registered, skipping duplicate");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to register {interfaceType.Name}: {e.Message}", this);
            }
        }

        private void RegisterManualServices()
        {
            if (manualServices == null) return;

            foreach (var serviceRef in manualServices)
            {
                try
                {
                    RegisterManualService(serviceRef);
                }
                catch (Exception e)
                {
                    if (serviceRef.required)
                    {
                        Debug.LogError($"Failed to register required service {serviceRef.interfaceName}: {e.Message}", this);
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to register optional service {serviceRef.interfaceName}: {e.Message}", this);
                    }
                }
            }
        }

        private void RegisterManualService(ServiceReference serviceRef)
        {
            if (string.IsNullOrEmpty(serviceRef.interfaceName))
            {
                Debug.LogWarning("Empty interface name in manual service registration");
                return;
            }

            if (serviceRef.implementation == null)
            {
                if (serviceRef.required)
                {
                    Debug.LogError($"Missing implementation for required service {serviceRef.interfaceName}");
                }
                return;
            }

            // Find the interface type
            Type interfaceType = FindInterfaceType(serviceRef.interfaceName);
            if (interfaceType == null)
            {
                Debug.LogError($"Interface type {serviceRef.interfaceName} not found");
                return;
            }

            // Verify implementation
            if (!interfaceType.IsAssignableFrom(serviceRef.implementation.GetType()))
            {
                Debug.LogError($"Implementation {serviceRef.implementation.GetType().Name} does not implement {serviceRef.interfaceName}");
                return;
            }

            // Register the service
            Dependencies.Container.Register(interfaceType, serviceRef.implementation);
            _registeredServices[interfaceType] = serviceRef.implementation;

            if (logRegistrations)
            {
                Debug.Log($"Manually registered {interfaceType.Name} -> {serviceRef.implementation.GetType().Name}");
            }
        }

        private Type FindInterfaceType(string interfaceName)
        {
            // First try to find in current assembly
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    var type = assembly.GetType(interfaceName);
                    if (type != null && type.IsInterface)
                        return type;
                        
                    // Also try searching by simple name
                    var types = assembly.GetTypes();
                    foreach (var t in types)
                    {
                        if (t.IsInterface && t.Name == interfaceName)
                            return t;
                    }
                }
                catch (Exception)
                {
                    // Skip assemblies that can't be loaded
                    continue;
                }
            }
            
            return null;
        }

        private void LogRegistrationSummary()
        {
            Debug.Log($"Service registration complete. Registered {_registeredServices.Count} services:");
            
            foreach (var kvp in _registeredServices)
            {
                var interfaceType = kvp.Key;
                var implementation = kvp.Value;
                Debug.Log($"  {interfaceType.Name} -> {implementation.GetType().Name}");
            }
        }

        // Validation method for editor
        [ContextMenu("Validate All Services")]
        private void ValidateServices()
        {
            int validServices = 0;
            int invalidServices = 0;

            foreach (var kvp in _registeredServices)
            {
                var interfaceType = kvp.Key;
                var implementation = kvp.Value as MonoBehaviour;

                if (implementation != null && implementation.gameObject != null)
                {
                    validServices++;
                }
                else
                {
                    invalidServices++;
                    Debug.LogWarning($"Invalid service registration: {interfaceType.Name}");
                }
            }

            Debug.Log($"Service validation complete: {validServices} valid, {invalidServices} invalid");
        }
    }

    // Extension for DIContainer to support Type registration
    public static class DIContainerExtensions
    {
        public static void Register(this DIContainer container, Type interfaceType, object implementation)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (interfaceType == null) throw new ArgumentNullException(nameof(interfaceType));
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));

            // Use reflection to call the generic Register method
            var registerMethod = typeof(DIContainer).GetMethod("Register", new Type[] { typeof(object) });
            if (registerMethod != null)
            {
                var genericRegisterMethod = registerMethod.MakeGenericMethod(interfaceType);
                genericRegisterMethod.Invoke(container, new object[] { implementation });
            }
        }
    }
}