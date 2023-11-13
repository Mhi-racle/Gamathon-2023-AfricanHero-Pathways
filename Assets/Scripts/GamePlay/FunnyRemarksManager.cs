using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FunnyRemarksManager : MonoBehaviour
{
    public static FunnyRemarksManager Instance { set; get; }
    public TextMeshProUGUI pauseText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI loseText;

    string[] pauseComments =
       {"Taking a break I see...\nHurry up, the path won't create itself!",
        "Tired?\nCan't keep up with the mighty Ananse?",
        "Catch your breath.\nI understand. It isn't easy keeping up\nwith the great Ananse.",
        "Rest a bit.\nYou'll need strength to guide me.",
        "Taking a break I see...\nHurry up, the wisdom won't steal\nitself.",
        "Taking a break I see...\nYe mortals are really weak.",
        "Resting a bit..\nWell you need the rest, take your\ntime.",
        "Stop slacking.\nThe gourd is waiting to be found."
    };

    string[] winComments =
       {"You found the gourd.\nYou served the master craftsman well.",
        "Thank you for helping me.\nI might consider sharing my wisdom\nwith you.",
        "How does it feel to help the great\nAnanse? Must feel great right?",
        "Good work getting to\nthe gourd.",
        "Gathering wisdom sure is fun.\nDon't worry, I will share it with you.\n",
        "Very soon all the knowledge in the\nworld would be mine.",
        "Good work.\nSeems Oyankopon sent his best to\nspy on me.",
        "Good work getting to the gourd.\nSeems my wisdom is rubbing off\non you.",
        "More wisdom for me, ehem, \nsorry, us.",
        "Great job, you succeeded.\nI'm impressed.",
        "Well done\nI will tell Oyankopon you served well."



    };

    string[] loseComments =
       {"Hahaha, you failed.\nWanna give it another shot?\nOr you are too scared?",
        "You had just one job..\nSigh",
        "Riddle riddle, \nwhat can't solve a path puzzle?\n\"You\"",
        "Don't feel bad.\nTake a shot at another chance to \nhelp me. You're welcome.",
        "Sighh..\nLet's try again, shall we?\n",
        "Oh Onyankopon.\nIs this the best person \nyou could send?\n",
        "Don't feel bad.\nWhen we get all the wisdom I'll make\nyou smarter.",
        "Don't give up.\nLet's try again. \nWe have alot at stake, me getting\nwiser.",
        "Let's try again.\nYou are helping the great Ananse,\nsurely you can't falter.",
        ".................\nI'm tired of talking.\n",
        ".................\nSIgh, let's try again\n",
        "I will forgive your failure if \nyou try again."

    };

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ShowPauseText()
    {
        pauseText.text = pauseComments[(Random.Range(0, pauseComments.Length))];
    }
    public void ShowWinText()
    {
        winText.text = winComments[(Random.Range(0, winComments.Length))];
    }
    public void ShowLoseText()
    {
        loseText.text = loseComments[(Random.Range(0, loseComments.Length))];
    }
}
