using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class UserBasicInfo 
{
    public UserBasicInfo() { }

    public UserBasicInfo(User user) 
    {
        id = user.playfabId;
        name = user.username;
    }


    public string id;
    public string name;
    public string avatar;
    public string country;
    public int petsCount;
}
