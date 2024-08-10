using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class GreenArrowsScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;
    public KMColorblindMode Colorblind;
    public KMRuleSeedable RuleSeedable;

    public KMSelectable[] buttons;
    public GameObject numDisplay;
    public GameObject colorblindText;

    private int number = 0;
    private int streak = 0;
    private string nextMove;

    private bool isanimating = true;
    private bool colorblindActive = false;

    private List<int>[] solutions = new List<int>[4] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
    private static readonly int[] _seed1Table = new int[100] { 2, 3, 0, 1, 2, 0, 1, 2, 0, 1, 0, 0, 2, 3, 0, 2, 3, 0, 2, 3, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 3, 1, 2, 0, 1, 2, 3, 0, 1, 0, 1, 3, 0, 1, 0, 0, 1, 2, 3, 2, 0, 2, 2, 0, 1, 2, 3, 0, 1, 3, 1, 3, 0, 1, 2, 0, 2, 1, 2, 2, 3, 1, 0, 1, 0, 3, 2, 3, 1, 0, 1, 0, 1, 0, 3, 2, 0, 1, 3, 2, 0, 2, 2, 3, 0, 2, 3, 0, 2, 3 };

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        colorblindActive = Colorblind.ColorblindModeActive;
        foreach (KMSelectable obj in buttons){
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void Start () {
        var rnd = RuleSeedable.GetRNG();
        if (rnd.Seed == 1)
            for (int i = 0; i < 100; i++)
                solutions[_seed1Table[i]].Add(i);
        else
        {
            Debug.LogFormat("[Green Arrows #{0}] Using rule seed {1}.", moduleId, rnd.Seed);
            var list = new List<int>();
            for (int i = 0; i < 100; i++)
                list.Add(i % 4);
            rnd.ShuffleFisherYates(list);
            for (int i = 0; i < 100; i++)
                solutions[list[i]].Add(i);
        }

        numDisplay.GetComponent<TextMesh>().text = " ";
        number = Rnd.Range(0, 100);
        getNextMove(number);
    }

    void OnActivate()
    {
        StartCoroutine(showNewNum());
        if (colorblindActive)
            colorblindText.SetActive(true);
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true && isanimating != true)
        {
            pressed.AddInteractionPunch(0.25f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
            if (nextMove.Equals("UP") && pressed != buttons[0])
            {
                GetComponent<KMBombModule>().HandleStrike();
                streak = 0;
                Debug.LogFormat("[Green Arrows #{0}] 'UP' was not pressed and was expected! Streak set back to 0!", moduleId);
                StartCoroutine(genAndRemoveNum());
            }
            else if (nextMove.Equals("DOWN") && pressed != buttons[1])
            {
                GetComponent<KMBombModule>().HandleStrike();
                streak = 0;
                Debug.LogFormat("[Green Arrows #{0}] 'DOWN' was not pressed and was expected! Streak set back to 0!", moduleId);
                StartCoroutine(genAndRemoveNum());
            }
            else if (nextMove.Equals("LEFT") && pressed != buttons[2])
            {
                GetComponent<KMBombModule>().HandleStrike();
                streak = 0;
                Debug.LogFormat("[Green Arrows #{0}] 'LEFT' was not pressed and was expected! Streak set back to 0!", moduleId);
                StartCoroutine(genAndRemoveNum());
            }
            else if (nextMove.Equals("RIGHT") && pressed != buttons[3])
            {
                GetComponent<KMBombModule>().HandleStrike();
                streak = 0;
                Debug.LogFormat("[Green Arrows #{0}] 'RIGHT' was not pressed and was expected! Streak set back to 0!", moduleId);
                StartCoroutine(genAndRemoveNum());
            }
            else
            {
                streak++;
                if (streak == 7)
                {
                    moduleSolved = true;
                    StartCoroutine(victory());
                    Debug.LogFormat("[Green Arrows #{0}] '{1}' pressed successfully! Streak is now {2}!", moduleId, nextMove, streak);
                    Debug.LogFormat("[Green Arrows #{0}] Streak of 7 reached! Module Disarmed!", moduleId);
                }
                else
                {
                    Debug.LogFormat("[Green Arrows #{0}] '{1}' pressed successfully! Streak is now {2}!", moduleId, nextMove, streak);
                    StartCoroutine(genAndRemoveNum());
                }
            }
        }
    }

    private IEnumerator showNewNum()
    {
        if (number <= 9)
        {
            yield return new WaitForSeconds(0.5f);
            numDisplay.GetComponent<TextMesh>().text = "0";
            yield return new WaitForSeconds(0.5f);
            numDisplay.GetComponent<TextMesh>().text += number;
        }
        else
        {
            string num = ""+ number;
            yield return new WaitForSeconds(0.5f);
            numDisplay.GetComponent<TextMesh>().text = num.Substring(0,1);
            yield return new WaitForSeconds(0.5f);
            numDisplay.GetComponent<TextMesh>().text += num.Substring(1,1);
        }
        isanimating = false;
    }

    private IEnumerator genAndRemoveNum()
    {
        isanimating = true;
        number = Rnd.Range(0, 100);
        getNextMove(number);
        yield return new WaitForSeconds(0.5f);
        string num = numDisplay.GetComponent<TextMesh>().text;
        numDisplay.GetComponent<TextMesh>().text = num.Substring(0,1);
        yield return new WaitForSeconds(0.5f);
        numDisplay.GetComponent<TextMesh>().text = " ";
        StartCoroutine(showNewNum());
    }

    private void getNextMove(int i)
    {
        if (solutions[0].Contains(i))
            nextMove = "UP";
        else if (solutions[1].Contains(i))
            nextMove = "RIGHT";
        else if (solutions[2].Contains(i))
            nextMove = "DOWN";
        else if (solutions[3].Contains(i))
            nextMove = "LEFT";
        Debug.LogFormat("[Green Arrows #{0}] The number displayed is {1}, the next move should be '{2}'", moduleId, i, nextMove);
    }

    private IEnumerator victory()
    {
        isanimating = true;
        for (int i = 0; i < 100; i++)
        {
            int rand1 = Rnd.Range(0, 10);
            int rand2 = Rnd.Range(0, 10);
            if (i < 50)
            {
                numDisplay.GetComponent<TextMesh>().text = rand1 + "" + rand2;
            }
            else
            {
                numDisplay.GetComponent<TextMesh>().text = "G" + rand2;
            }
            yield return new WaitForSeconds(0.025f);
        }
        numDisplay.GetComponent<TextMesh>().text = "GG";
        isanimating = false;
        GetComponent<KMBombModule>().HandlePass();
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} up/right/down/left [Presses the specified arrow button] | Words can be substituted as one letter (Ex. right as r)";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*up\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*u\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            buttons[0].OnInteract();
        }
        if (Regex.IsMatch(command, @"^\s*down\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*d\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            buttons[1].OnInteract();
        }
        if (Regex.IsMatch(command, @"^\s*left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*l\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            buttons[2].OnInteract();
        }
        if (Regex.IsMatch(command, @"^\s*right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(command, @"^\s*r\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            buttons[3].OnInteract();
        }
        if (moduleSolved) { yield return "solve"; }
        yield break;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (streak <= 6)
        {
            while (isanimating) { yield return true; };
            if (nextMove.Equals("UP"))
            {
                yield return ProcessTwitchCommand("up");
            }
            else if (nextMove.Equals("DOWN"))
            {
                yield return ProcessTwitchCommand("down");
            }
            else if (nextMove.Equals("LEFT"))
            {
                yield return ProcessTwitchCommand("left");
            }
            else if (nextMove.Equals("RIGHT"))
            {
                yield return ProcessTwitchCommand("right");
            }
            if (streak == 7)
            {
                break;
            }
        }
        while (isanimating) { yield return true; };
    }
}
