using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class KeyValue
{
    public enum ValueTypes { String, Float, Int, Obj}
    
    public string key;
    public ValueTypes valueType;
    public string stringValue;
    public float floatValue;
    public float intValue;
    public object objValue;

    public object GetValue()
    {
        if (valueType == ValueTypes.String) { return stringValue; }
        if (valueType == ValueTypes.Float)  { return floatValue; }
        if (valueType == ValueTypes.Int)    { return intValue; }
        if (valueType == ValueTypes.Obj)    { return objValue; }
          
        return null;
    }

    public static ExitGames.Client.Photon.Hashtable GetHashtableFromKeyValueList(List<KeyValue> keyValueList)
    {
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        for (int i = 0; i < keyValueList.Count; i++)
        {
            hashtable.Add(keyValueList[i].key, keyValueList[i].GetValue());
        }
        return hashtable;
    }


    public static object GetValueFromList(string key, List<KeyValue> keyValueList)
    {
        for (int i = 0; i < keyValueList.Count; i++)
        {
            if (keyValueList[i].key == key)
            {
                if (keyValueList[i].valueType == ValueTypes.String) { return keyValueList[i].stringValue; }
                if (keyValueList[i].valueType == ValueTypes.Float)  { return keyValueList[i].floatValue; }
                if (keyValueList[i].valueType == ValueTypes.Int)    { return keyValueList[i].intValue; }
                if (keyValueList[i].valueType == ValueTypes.Obj)    { return keyValueList[i].objValue; }
            }
        }
        return null;
    }

    public static void AddValueToList(string key, ValueTypes valueType, object value, List<KeyValue> keyValueList)
    {
        KeyValue keyValue = new KeyValue() { key = key, valueType = valueType };

        if (valueType == ValueTypes.String) { keyValue.stringValue = (string)value; }
        if (valueType == ValueTypes.Float)  { keyValue.floatValue = (float)value; }
        if (valueType == ValueTypes.Int)    { keyValue.intValue = (int)value; }
        if (valueType == ValueTypes.Obj)    { keyValue.objValue = value; }

        keyValueList.Add(keyValue);
    }
}
