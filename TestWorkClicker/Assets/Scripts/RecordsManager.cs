using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecordsManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _records;

    private List<Score> _list = new List<Score>();
    private int _unitsOfRecord = 10;
    private int _currentUnitsOfRecord;
    private int _maxScore = 0;

    private void Start() 
    {
        FillOutSheet();
        GetMaximumRecord();
        PrintScore();
    }

    public void PrintScore()
    {
        FillOutSheet();

        string tempString = $"Последние {_list.Count} рекордов:\n\n";

        for (int i = 0; i < _list.Count; i++)
            tempString += $"Дата установления рекорда: {_list[i].Date}, очков: {_list[i].Value}\n\n";
        
        _records.text = tempString;
    }

    private void GetMaximumRecord()
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (_maxScore <= _list[i].Value)
            {
                _maxScore = _list[i].Value;
            }
        }
    }

    private void SaveData(int score, DateTime date)
    {
            Score tempScore = new Score(score, date);

            if (_list.Count < _unitsOfRecord)
            {
                PlayerPrefs.SetInt($"Score{_list.Count}", tempScore.Value);
                PlayerPrefs.SetString($"Date{_list.Count}", tempScore.Date.ToString());
                PlayerPrefs.Save();
            }
            else
            {
                _list.Remove(_list[0]);
                _list.Add(tempScore);

                for (int i = 0; i < _list.Count; i++)
                {
                    PlayerPrefs.SetInt($"Score{i}", _list[i].Value);
                    PlayerPrefs.SetString($"Date{i}", _list[i].Date.ToString());
                    PlayerPrefs.Save();
                }
            }
    }

    private void FillOutSheet ()
    {
        _list.Clear();

        for (int i = 0; i < _unitsOfRecord; i++)
        {
            if (PresenceCheck($"Score{i}", $"Date{i}"))
            {
                _list.Add(new Score(PlayerPrefs.GetInt($"Score{i}"), PlayerPrefs.GetString($"Date{i}")));
            }
        }
    }

    private bool PresenceCheck(string score, string date)
    {
        if (PlayerPrefs.HasKey($"{score}") && PlayerPrefs.HasKey($"{date}"))
            return true;
        else
            return false;
    }

    public void SetScore(int score)
    {
        if (score > _maxScore)
        {
            SaveData(score, DateTime.Now);
        }
    }
}
public class Score
{
    private int _value;
    private string _date;

    public int Value => _value;
    public string Date => _date;

    public Score(int value, DateTime date)
    {
        _value = value;
        _date = date.ToString();
    }

    public Score(int value, string date)
    {
        _value = value;
        _date = date;
    }
}