using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class ClientsClass
{

    public string nameClient;
    public int indexAnswerChoose;
    public string answerChoose;

    public ClientsClass(string _name, int _indexAnswerChoose, string _answerStringText)
    {
        nameClient = _name;
        indexAnswerChoose = _indexAnswerChoose;
        answerChoose = _answerStringText;
    }
}
