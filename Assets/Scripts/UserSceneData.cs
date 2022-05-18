using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSceneData : MonoBehaviour
{
    public static UserSceneData ins;
    void Awake() { ins = this; }

    public string selectedPet = "Cat";
    public string selectedCity;
    public float bet = 10000;

    public bool playingChallenge;
    public ChallengeInfo challengeInfo;

    [System.Serializable]
    public class ChallengeInfo 
    {
        public string roomName;
        public string selectedCity = "Dublin";
        public float bet = 10000;
        public bool challengeSendingUser;
        public bool challengeRecievingUser;
    }

    public string GetSelectedCity()
    {
        string selectedCity = this.selectedCity;
        if (challengeInfo != null) { selectedCity = challengeInfo.selectedCity; }
        return selectedCity;
    }
}
