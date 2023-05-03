using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataUtils : MonoBehaviour
{
    public List<string> dataToSync = new List<string>();
    public List<object> dataToSyncVals = new List<object>();

    public List<string> dataForClients = new List<string>();
    public List<object> dataForClientsVals = new List<object>();

    


    public void AddDataToSync(List<string> data, List<object> vals)
    {
        AddData(dataToSync, dataToSyncVals, data, vals);
    }
    public void AddDataToSync(string data, object val)
    {
        AddDataToSync(new List<string>() { data }, new List<object>() { val });
    }


    public void AddDataForClients(List<string> data, List<object> vals)
    {
        AddData(dataForClients, dataForClientsVals, data, vals);
    }
    public void AddDataForClients(string data, object val)
    {
        AddDataForClients(new List<string>() { data }, new List<object>() { val });
    }

    public ExitGames.Client.Photon.Hashtable GetDataToSyncHashtable()
    {
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        for (int i = 0; i < dataToSync.Count; i++) { hashtable.Add(dataToSync[i], dataToSyncVals[i]); }
        return hashtable;
    }

    public ExitGames.Client.Photon.Hashtable GetDataForClientsHashtable()
    {
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
        for (int i = 0; i < dataForClients.Count; i++) { hashtable.Add(dataForClients[i], dataForClientsVals[i]); }
        return hashtable;
    }

    public void MergeData(DataUtils mergeWith)
    {
        AddDataToSync(mergeWith.dataToSync, mergeWith.dataToSyncVals);
        AddDataForClients(mergeWith.dataForClients, mergeWith.dataForClientsVals);
    }

    void AddData(List<string> addTo, List<object> addToVals, List<string> data, List<object> vals)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (addTo.Contains(data[i]))
            {
                addToVals[addTo.IndexOf(data[i])] = vals[i];
            }
            else
            {
                addTo.Add(data[i]);
                addToVals.Add(vals[i]);
            }
        }
    }

    
}
