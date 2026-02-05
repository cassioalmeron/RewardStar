using RewardStar.Api.DTOs;
using RewardStart.Core;
using RewardStart.Core.Models;
using System.Reflection;

namespace RewardStar.Api.Extensions;

public static class EntityBaseExtension
{
    public static T CopyTo<T>(this EntityBase source)
        where T : DtoBase
    {
        ArgumentNullExceptionHelper.ThrowIfNull(source, nameof(source));

        var dto = Activator.CreateInstance<T>() ?? throw new InvalidOperationException($"Cannot create instance of {typeof(T).Name}");
        source.CopyTo(dto);
        return dto;
    }

    public static void CopyTo(this EntityBase source, DtoBase target)
    {
        ArgumentNullExceptionHelper.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionHelper.ThrowIfNull(target, nameof(target));

        var sourceType = source.GetType();
        var targetType = target.GetType();

        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetPropertyDict = targetProperties.ToDictionary(p => p.Name, StringComparer.Ordinal);

        foreach (var sourceProperty in sourceProperties)
        {
            if (!sourceProperty.CanRead)
                continue;

            // For navigation properties (EntityBase derived), skip them - we only want to copy simple properties
            if (sourceProperty.PropertyType.BaseType == typeof(EntityBase))
                continue;

            if (!targetPropertyDict.TryGetValue(sourceProperty.Name, out var targetProperty))
                continue;

            if (!targetProperty.CanWrite)
                continue;

            var sourceValue = sourceProperty.GetValue(source);

            if (sourceValue != null && sourceProperty.PropertyType.IsEnum && targetProperty.PropertyType == typeof(int))
                sourceValue = Convert.ToInt32(sourceValue);

            try
            {
                targetProperty.SetValue(target, sourceValue);
            }
            catch
            {
                continue;
            }
        }
    }
}
