using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class swipe_menu : MonoBehaviour
{
    public GameObject scrollbar;
    float scrollPos = 0;
    float [] pos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pos = new float[transform.childCount];
        float distance = 1f/(pos.Length-1f); // since the items are 3, distance == 1/3-2 === 1/2
        for(int i=0; i< pos.Length; i++){
            pos[i] = distance * i;
            //since I have two items, the positions would be
            //pos[0] = 0 * 0.5 = 0
            //pos[1] = 1 * 0.5 = 0.5
            //pos[2] = 2 * 0.5 = 1
        }
        if(Input.GetMouseButton(0)){
            scrollPos = scrollbar.GetComponent<Scrollbar>().value;
            //get the position of the scrollbar and stores it if the mouse is moving
            
         
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit cardInfo;
            if(Physics.Raycast(ray, out cardInfo)){
                Debug.Log(cardInfo.transform.gameObject.name);
            }

        }
        else
        {
            for(int i=0; i < pos.Length; i++){
                if(scrollPos < pos[i] + (distance/2) && scrollPos > pos[i] - (distance /2)){
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value , pos[i], 0.1f);
                }
            }
        }

         for(int i=0; i < pos.Length; i++){
                if(scrollPos < pos[i] + (distance/2) && scrollPos > pos[i] - (distance /2)){
                    transform.GetChild(i).localScale =  Vector2.Lerp( transform.GetChild(i).localScale, new Vector2(1.2f, 1.2f), 0.1f);

                     for(int a=0; a < pos.Length; a++){
                         transform.GetChild(a).localScale = Vector2.Lerp( transform.GetChild(a).localScale, new Vector2(.8f, 0.8f), 0.1f);
                     }
                }
    }   
        }

    public void QuickPlay(){
        if(scrollPos == 0){
            Debug.Log("Already in focus");
        }
        else{
            scrollPos = 0f;
        }
    }
}
