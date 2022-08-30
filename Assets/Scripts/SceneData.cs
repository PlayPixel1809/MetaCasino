using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : MonoBehaviour
{
    public static SceneData ins;
    void Awake() { ins = this; }

    public string selectedPet = "Cat";
    public string selectedCity;
    public float bet = 10000;

    public bool playingChallenge;
    public ChallengeInfo challengeInfo;

    [System.Serializable]
    public class ChallengeInfo 
    {
        public string challengeSendingUser;
        public string challengeRecievingUser;
    }

    public string GetSelectedCity()
    {
        return selectedCity;
    }
}
