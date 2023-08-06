using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Base.Property;
using UnityEngine;

public static class EntityPropertyDependencyCache
{
    //private static Dictionary<string, List<string>> initializationOrderCache = new Dictionary<string, List<string>>();

    public static void ClearCache() {
        //initializationOrderCache.Clear();
    }

    public static List<string> GetInitializationOrder(string EntityName,
        Dictionary<string, IPropertyBase> Properties) {
        /*if (initializationOrderCache.TryGetValue(EntityName, out var cachedOrder)) {
            return cachedOrder;
        }*/

        var dependencies = new Dictionary<string, string[]>();
        
        foreach (IPropertyBase property in Properties.Values) {
            string[] dependentProperties = property.GetDependentProperties()?
                .Select(dependentProperty => dependentProperty.GetFullName()).ToArray();
            
            dependencies[property.GetFullName()] =
                dependentProperties ?? Array.Empty<string>();
        }

        var order = TopologicalSort(dependencies);
       // initializationOrderCache[EntityName] = order;
        return order;
    }

    private static List<string> TopologicalSort(Dictionary<string, string[]> dependencies)
    {
        List<string> result = new List<string>();
        HashSet<string> visited = new HashSet<string>();
        HashSet<string> recursionStack = new HashSet<string>();

        foreach (var node in dependencies.Keys)
        {
            if (!visited.Contains(node))
            {
                if (IsCyclic(node, dependencies, visited, recursionStack, result))
                {
                    throw new Exception("Graph contains cycle.");
                }
            }
        }

        //result.Reverse(); // Since we want to get the nodes in a way that dependencies come before their dependents, we reverse the list.
        return result;

        bool IsCyclic(string node, Dictionary<string, string[]> graph,
            HashSet<string> visited, HashSet<string> recursionStack, List<string> result)
        {
            visited.Add(node);
            recursionStack.Add(node);

            if (graph.TryGetValue(node, out string[] dependents))
            {
                foreach (var dependent in dependents)
                {
                    if (!visited.Contains(dependent) && IsCyclic(dependent, graph, visited, recursionStack, result)) {
                        return true;
                    }
                    else if (recursionStack.Contains(dependent)) {
                        return true;
                    }
                }
            }

            recursionStack.Remove(node);
            result.Add(node); // Add node to result after visiting all dependencies
            return false;
        }
    }
}
