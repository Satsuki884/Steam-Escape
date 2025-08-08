namespace LinkModule.Scripts.Config
{
    public interface IConfigurable<T>
    {
        void SetConfig(T config);
    }
}