using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class KeyValue 
{
    public string key;
    public string value;
    public object obj;

    public static string GetKey(string key, List<KeyValue> data)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].key == key)
            {
                return data[i].value;
            }
        }
        return string.Empty;
    }

    public object GetVal()
    {
        if (!string.IsNullOrEmpty(value)) { return value; }
        if (obj != null) { return obj; }

        return null;
    }
}
