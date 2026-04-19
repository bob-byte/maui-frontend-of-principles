using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Principles.Core.Extensions;

public static class ObjectExtensions
{
    public static void MergeFrom(this object target, object source)
    {
        if (target == null || source == null)
        {
            throw new ArgumentNullException("Target and source must not be null");
        }

        var visited = new HashSet<ObjectPair>( ObjectPairComparer.Instance );
        MergeInternal(target, source, visited);
    }

    private static void MergeInternal(object target, object source, HashSet<ObjectPair> visited)
    {
        if (!visited.Add(new ObjectPair( target, source )))
        {
            return;
        }

        Type targetType = target.GetType();
        Type sourceType = source.GetType();

        foreach (PropertyInfo sourceProp in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!sourceProp.CanRead)
            {
                continue;
            }

            PropertyInfo? targetProp = targetType.GetProperty(sourceProp.Name, BindingFlags.Public | BindingFlags.Instance);
            if (targetProp == null || !targetProp.CanWrite)
            {
                continue;
            }

            object? sourceValue = sourceProp.GetValue(source);
            if (sourceValue == null)
            {
                continue;
            }

            Type sourcePropType = sourceProp.PropertyType;
            Type targetPropType = targetProp.PropertyType;

            if (IsSimpleType(sourcePropType) && targetPropType.IsAssignableFrom(sourcePropType))
            {
                targetProp.SetValue(target, sourceValue);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(sourcePropType) && sourcePropType != typeof(string))
            {
                if (sourceProp.Name is "Progresses" or "ComputedProgresses")
                {
                    targetProp.SetValue(target, sourceValue);
                }
                else
                {
                    object? targetValue = targetProp.GetValue(target);
                    object? merged = MergeEnumerables(targetValue, sourceValue, targetPropType, visited);
                    targetProp.SetValue(target, merged);
                }
            }
            else
            {
                object? targetValue = targetProp.GetValue(target);
                if (targetValue == null)
                {
                    try
                    {
                        targetValue = Activator.CreateInstance(targetPropType);
                        targetProp.SetValue(target, targetValue);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (targetValue == null)
                {
                    continue;
                }

                MergeInternal(targetValue, sourceValue, visited);
            }
        }
    }

    private static object? MergeEnumerables(object? targetValue, object? sourceValue, Type targetType, HashSet<ObjectPair> visited)
    {
        if (sourceValue == null)
        {
            return targetValue;
        }

        if (targetValue == null)
        {
            try
            {
                targetValue = Activator.CreateInstance(targetType);
            }
            catch
            {
                return sourceValue;
            }
        }

        var targetList = targetValue as IList;
        var sourceList = sourceValue as IEnumerable;

        if (targetList == null || sourceList == null)
        {
            return sourceValue;
        }

        int i = 0;
        foreach (object? sourceItem in sourceList)
        {
            if (i < targetList.Count)
            {
                object? targetItem = targetList[i];
                if (targetItem != null && sourceItem != null &&
                    !IsSimpleType(sourceItem.GetType()) &&
                    targetItem.GetType() == sourceItem.GetType())
                {
                    MergeInternal(targetItem, sourceItem, visited);
                }
                else if (sourceItem != null)
                {
                    targetList[i] = sourceItem;
                }
            }
            else
            {
                targetList.Add(sourceItem);
            }

            i++;
        }

        return targetList;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(Guid) ||
               type == typeof(TimeSpan);
    }

    private readonly record struct ObjectPair(object Target, object Source);

    private sealed class ObjectPairComparer : IEqualityComparer<ObjectPair>
    {
        public static ObjectPairComparer Instance { get; } = new();

        public bool Equals(ObjectPair x, ObjectPair y)
        {
            return ReferenceEquals(x.Target, y.Target) && ReferenceEquals(x.Source, y.Source);
        }

        public int GetHashCode(ObjectPair obj)
        {
            return HashCode.Combine(
                RuntimeHelpers.GetHashCode(obj.Target),
                RuntimeHelpers.GetHashCode(obj.Source)
            );
        }
    }
}
