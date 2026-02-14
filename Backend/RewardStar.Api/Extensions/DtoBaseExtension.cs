using RewardStar.Core.Models;

namespace RewardStar.Core.Extensions;

public static class DtoBaseExtension
{
    public static T CopyTo<T>(this RewardStar.Api.DTOs.DtoBase source)
        where T : EntityBase
    {
        var item = Activator.CreateInstance<T>();
        source.CopyTo(item);
        return item;
    }

    public static void CopyTo(this RewardStar.Api.DTOs.DtoBase source, EntityBase target)
    {
        ArgumentNullExceptionHelper.ThrowIfNull(source, nameof(source));
        ArgumentNullExceptionHelper.ThrowIfNull(target, nameof(target));

        var sourceType = source.GetType();
        var targetType = target.GetType();

        var sourceProperties = sourceType.GetProperties();
        var targetProperties = targetType.GetProperties();

        foreach (var sourceProperty in sourceProperties)
        {
            var targetProperty = targetProperties.SingleOrDefault(x => x.Name == sourceProperty.Name);

            if (targetProperty == null)
            {
                var name = sourceProperty.Name;
                if (name.EndsWith("Id") && name != "Id")
                {
                    name = name.Replace("Id", "");
                    targetProperty = targetProperties.SingleOrDefault(x => x.Name == name);
                }
            }

            if (targetProperty == null)
                continue;

            var sourceValue = sourceProperty.GetValue(source, null);

            if (targetProperty.PropertyType.BaseType == typeof(EntityBase))
            {
                if (sourceValue == null)
                    continue;

                var id = (int)sourceValue;
                if (id == 0)
                    continue;

                var subEntity = (EntityBase)Activator.CreateInstance(targetProperty.PropertyType);
                subEntity.Id = (int)sourceValue;
                sourceValue = subEntity;
            }

            targetProperty?.SetValue(target, sourceValue);
        }
    }
}