using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SketchyFont : MonoBehaviour {
    private Font font1;
    private Font font2;
    private Text txt;
    void Start()
    {
        txt = GetComponent<Text>();
        // loads Font files from a folder in Assets named Resources
        font1 = (Font)Resources.Load("Fonts/GameText1", typeof(Font));
		font2 = (Font)Resources.Load("Fonts/GameText2", typeof(Font));
        StartCoroutine(FontChangeCoroutine());
    }

    IEnumerator FontChangeCoroutine()
    {
        // sketchy effect is on as long as GameObject is active
        //set up to easily add a third font when the third variant is fixed
        while (gameObject.activeSelf)
        {
            txt.font = font1;
            yield return new WaitForSeconds(.5f);
            txt.font = font2;
            yield return new WaitForSeconds(.5f);
        }
    }
}
