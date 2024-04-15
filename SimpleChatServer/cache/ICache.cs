namespace SimpleChatServer.cache;

public interface ICache<TKey, TValue>
{
    bool Initialize();

    TValue? Get(TKey key);

    bool Add(TKey key, TValue value);

    bool Save(TValue value);

    bool Remove(TKey key);

    bool Contains(TKey key);

    void ClearCache();
}