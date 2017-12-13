using System;

[Serializable]
public class FinalGraphClass 
{
    public string namePlayer;
    public int counterRightAnswer;

    public FinalGraphClass(string _name, int _counterRightAnswer)
    {
        namePlayer = _name;
        counterRightAnswer = _counterRightAnswer;
    }
}