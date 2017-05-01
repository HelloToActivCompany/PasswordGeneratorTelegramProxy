namespace PasswordGeneratorTelegramProxy.Models.Cache
{
    public interface ICache<TKey, TValue>
    {
        void Add(TKey key, TValue value);

        void Delete(TKey key);

        bool TryGet(TKey key, out TValue value);
    }
}
