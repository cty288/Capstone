using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EntityPropertyDependencyCache
{
    private static Dictionary<string, List<PropertyName>> initializationOrderCache = new Dictionary<string, List<PropertyName>>();

    public static void ClearCache() {
        initializationOrderCache.Clear();
    }
    public static List<PropertyName> GetInitializationOrder(string EntityName,
        Dictionary<PropertyName, IPropertyBase> Properties) {
        if (initializationOrderCache.TryGetValue(EntityName, out var cachedOrder)) {
            return cachedOrder;
        }

        var dependencies = new Dictionary<PropertyName, PropertyName[]>();
        foreach (var property in Properties.Values) {
            PropertyName[] dependentProperties = property.GetDependentProperties();
            dependencies[property.PropertyName] =
                dependentProperties ?? Array.Empty<PropertyName>();
        }

        var order = TopologicalSort(dependencies);
        initializationOrderCache[EntityName] = order;
        return order;
    }

    private static List<PropertyName> TopologicalSort(Dictionary<PropertyName, PropertyName[]> dependencies)
    {
        List<PropertyName> result = new List<PropertyName>();
        HashSet<PropertyName> visited = new HashSet<PropertyName>();
        HashSet<PropertyName> recursionStack = new HashSet<PropertyName>();

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

        bool IsCyclic(PropertyName node, Dictionary<PropertyName, PropertyName[]> graph,
            HashSet<PropertyName> visited, HashSet<PropertyName> recursionStack, List<PropertyName> result)
        {
            visited.Add(node);
            recursionStack.Add(node);

            if (graph.TryGetValue(node, out PropertyName[] dependents))
            {
                foreach (var dependent in dependents)
                {
                    if (!visited.Contains(dependent) && IsCyclic(dependent, graph, visited, recursionStack, result))
                    {
                        return true;
                    }
                    else if (recursionStack.Contains(dependent))
                    {
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
