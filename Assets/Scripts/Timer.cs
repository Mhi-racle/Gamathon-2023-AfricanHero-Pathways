using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public int timeLeft = 50;
    public bool isCounting = false;
  
    // Start is called before the first frame update
    void Start()
    {

        //timerText.gameObject.SetActive(false);
        isCounting = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!DialogManager.isActive)
        {
            timerText.gameObject.SetActive(true);
            if (!isCounting && timeLeft > 0)
            {
                StartCoroutine(count());
            }
            else if (timeLeft == 0)
            {
                timerText.text = "Time is up";
                GameManager.Instance.TimeUp();
            }

        }
    }
    IEnumerator count()
    {
        isCounting = true;
        yield return new WaitForSeconds(1f);
        timeLeft--;
        timerText.text = timeLeft + "s";
        isCounting = false;
    }
}
