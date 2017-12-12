using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class AnswerClass 
{
    public bool isRightAnswer;
    public string answerChoose;

    public AnswerClass(bool _isRight, string _answerStringText)
    {
        isRightAnswer = _isRight;
        answerChoose = _answerStringText;
    }
}