using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AnswerClass 
{
    public bool isRightAnswer;
    public string answerText;

    public AnswerClass(bool _isRight, string _answerStringText)
    {
        isRightAnswer = _isRight;
        answerText = _answerStringText;
    }
}