using System.Reflection;
public class ReflectableClass
{
    public object this[string key]
    {
        get
        {
            return this.GetType().GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(this);
        }
        set
        {
            this.GetType().GetField(key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(this, value);
        }
    }

}