public interface ISettingsSetter
{
    /// <summary>
    /// Prepare listeners
    /// </summary>
    void Prepare();

    /// <summary>
    /// Load saved values, if any
    /// </summary>
    void Load();
}