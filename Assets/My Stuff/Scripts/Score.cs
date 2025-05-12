using UnityEngine;
using UnityEngine.UI;
public class Score : MonoBehaviour
{
    Text score;
    public GameObject target;
    public Target tg_script;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       score = GetComponent<Text>();
       

    }

    // Update is called once per frame
    void Update()
    {
        var tg_script = target.GetComponent<Target>(); 
        score.text = "Current Score\n\n "+ tg_script.hits.ToString(); 
    }
}
