namespace Covenant.Core;

internal class ShouldSerializeContractResolver : DefaultContractResolver
{
    public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.PropertyType != typeof(string))
        {
            var propertyType = property.PropertyType;
            if (propertyType != null)
            {
                if (propertyType.GetInterface(nameof(IEnumerable)) != null)
                {
                    property.ShouldSerialize =
                        instance =>
                        {
                            if (property.PropertyName != null)
                            {
                                return (instance?
                                    .GetType()
                                    .GetProperty(property.PropertyName)?
                                    .GetValue(instance) as IEnumerable<object>)?.Count() > 0;
                            }

                            return true;
                        };
                }
            }
        }

        return property;
    }
}