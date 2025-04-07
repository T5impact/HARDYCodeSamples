using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public class String
{
    public string text;
    public AudioClip clip;
}

public class Introduction : MonoBehaviour
{
    [SerializeField] String[] dialogueText;
    [SerializeField] TMP_Text stringText;
    [SerializeField] float timeToDisplay;
    [SerializeField] float timeBefNextSentence;
    [SerializeField] AudioSource source;

    Queue<String> dialogueQueue = new Queue<String>();

    bool doneDisplaying = true;

    // Start is called before the first frame update
    void Start()
    {
        foreach(String s in dialogueText)
        {
            dialogueQueue.Enqueue(s);
        }

        doneDisplaying = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Goes through the dialogue queue until all pieces of dialogue finish
        if(dialogueQueue.Count > 0 && doneDisplaying)
        {
            StartCoroutine(SetText(dialogueQueue.Dequeue()));
        }

        //Once dialogue finishes, load the main menu
        if(dialogueQueue.Count == 0 && doneDisplaying)
        {
            SceneManager.LoadScene(1);
        }
    }

    //Display the provided string incrementally and play the provided audio
    IEnumerator SetText(String s)
    {
        float time = 0;

        doneDisplaying = false;

        stringText.text = null;
        char[] chars = s.text.ToCharArray();
        source.clip = s.clip;
        source.Play();

        for (int i = 0; i < chars.Length; i++)
        {
            stringText.text += chars[i];

            //Skip spaces when waiting inbetween characters
            if (char.IsWhiteSpace(chars[i]))
            {
                i++;
                if(i <= chars.Length)
                    stringText.text += chars[i];
            }
            time += Time.deltaTime;

            //Allow player to skip the dialogue by pressing Space
            if (Input.GetKey(KeyCode.Space))
            {
                doneDisplaying = true;
                break;
            }

            yield return new WaitForSeconds(timeToDisplay);
        }

        stringText.text = s.text;

        float timeBef = timeBefNextSentence;

        if (doneDisplaying == false)
            yield return new WaitForSeconds(timeBef);

        doneDisplaying = true;
    }
}
