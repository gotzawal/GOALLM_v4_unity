using System;
using System.Collections.Generic;

[Serializable]
public class WorldStatus
{
    public Dictionary<string, SerializablePlace> Places { get; set; }
    public Dictionary<string, SerializableItem> Items { get; set; }

    public WorldStatus(WorldState worldState)
    {
        Places = new Dictionary<string, SerializablePlace>();
        foreach (var kvp in worldState.Places)
        {
            Places[kvp.Key] = new SerializablePlace(kvp.Value);
        }

        Items = new Dictionary<string, SerializableItem>();
        foreach (var kvp in worldState.Items)
        {
            Items[kvp.Key] = new SerializableItem(kvp.Value);
        }
    }
}

[Serializable]
public class SerializablePlace
{
    public string Name { get; set; }
    public List<string> Inventory { get; set; }
    public Dictionary<string, object> State { get; set; }

    public SerializablePlace(Place place)
    {
        Name = place.Name;
        Inventory = new List<string>(place.Inventory);
        State = new Dictionary<string, object>(place.State);
    }
}

[Serializable]
public class SerializableItem
{
    public string Name { get; set; }
    // 필요한 다른 직렬화 가능한 필드를 추가하세요.

    public SerializableItem(Item item)
    {
        Name = item.Name;
        // 다른 필드 초기화
    }
}
