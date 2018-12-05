using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Quotes
{
    List<string> quotes;

    public string RandomQuote
    {
        get
        {
            return quotes[UnityEngine.Random.Range(0, quotes.Count)];
        }
    }

    public Quotes(string filename)
    {
        TextAsset txtAsset = Resources.Load(filename) as TextAsset;
        string[] quotations = txtAsset.text.Split("%".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        quotes = new List<string>();
        quotes.AddRange(quotations);
    }
}
