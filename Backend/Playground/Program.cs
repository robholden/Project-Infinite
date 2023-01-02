
Console.WriteLine("1 => {0} 2 => {1}", a.Test, a.Test2);
Console.ReadLine();

class A
{
    public string Test { get; set; }
    public string Test2 { get; set; }
}

class CustomPropertyCalls<TSource> where TSource : class
{
    private readonly CustomUpdater<TSource> _updater;

    public CustomPropertyCalls(TSource source)
    {
        _updater = new CustomUpdater<TSource>(source);
    }

    public CustomPropertyCalls<TSource> Apply<TProperty>(Expression<Func<CustomUpdater<TSource>, CustomUpdater<TSource>>> propertyExpression)
    {
        // Invoke the lambda expression to get the SetPropertyCalls object.
        propertyExpression.Compile().Invoke(_updater);

        return this;
    }
}

class CustomUpdater<TSource> where TSource : class
{
    private readonly TSource _source;
    private readonly Type _type;

    public CustomUpdater(TSource source)
    {
        _source = source;
        _type = source.GetType();
    }

    public CustomUpdater<TSource> SetProperty<TProperty>(Expression<Func<TSource, TProperty>> propertyExpression, TProperty valueExpression)
    {
        // Get the name of the property.
        string propertyName = GetPropertyName(propertyExpression);

        // Get the property information for the property with the specified name.
        PropertyInfo propertyInfo = _type.GetProperty(propertyName);

        // Set the value of the property.
        propertyInfo.SetValue(_source, valueExpression);

        return this;
    }

    private static string GetPropertyName<T, U>(Expression<Func<T, U>> propertySelector)
    {
        // Get the body of the lambda expression.
        MemberExpression body = propertySelector.Body as MemberExpression;

        // Get the name of the property.
        return body.Member.Name;
    }
}

public sealed class SetPropertyCalls<TSource>
{
    private SetPropertyCalls() { }

    public SetPropertyCalls<TSource> SetProperty<TProperty>(Func<TSource, TProperty> propertyExpression, Func<TSource, TProperty> valueExpression)
        => throw new InvalidOperationException(RelationalStrings.SetPropertyMethodInvoked);

    public SetPropertyCalls<TSource> SetProperty<TProperty>(Func<TSource, TProperty> propertyExpression, TProperty valueExpression)
        => throw new InvalidOperationException(RelationalStrings.SetPropertyMethodInvoked);
}