namespace YZXLogicEngine.Morse
{
  public class MorseConvert
  {
    public readonly static System.Collections.Hashtable morseCode = new System.Collections.Hashtable();
    static MorseConvert()
    {
      morseCode["A"] = ".-";
      morseCode["B"] = "-...";
      morseCode["C"] = "-.-.";
      morseCode["D"] = "-..";
      morseCode["E"] = ".";
      morseCode["F"] = "..-.";
      morseCode["G"] = "--.";
      morseCode["H"] = "....";
      morseCode["I"] = "..";
      morseCode["J"] = ".---";
      morseCode["K"] = "-.-";
      morseCode["L"] = ".-..";
      morseCode["M"] = "--";
      morseCode["N"] = "-.";
      morseCode["O"] = "---";
      morseCode["P"] = ".--.";
      morseCode["Q"] = "--.-";
      morseCode["R"] = ".-.";
      morseCode["S"] = "...";
      morseCode["T"] = "-";
      morseCode["U"] = "..-";
      morseCode["V"] = "...-";
      morseCode["W"] = ".--";
      morseCode["X"] = "-..-";
      morseCode["Y"] = "-.--";
      morseCode["Z"] = "--..";
      morseCode["1"] = ".----";
      morseCode["2"] = "..---";
      morseCode["3"] = "...--";
      morseCode["4"] = "....-";
      morseCode["5"] = ".....";
      morseCode["6"] = "-....";
      morseCode["7"] = "--...";
      morseCode["8"] = "---..";
      morseCode["9"] = "----.";
      morseCode["0"] = "-----";
      morseCode["?"] = "..--..";
      morseCode["/"] = "-..-.";
      morseCode["["] = "-.-..";
      morseCode["]"] = ".---.";
      morseCode["-"] = "-....-";
      morseCode["."] = ".-.-.-";
      morseCode["@"] = "--.-.";
      morseCode["*"] = "----";
      morseCode["$"] = "...-.";
      morseCode["#"] = "..--";
      //空格替换成七个空格
      morseCode[" "] = "       ";
    }

    public static string word2morse(string str)
    {
      string morse = string.Empty;
      str = str.ToUpper();
      foreach (char s in str)
      {
        if (morseCode.ContainsKey(s.ToString()))
        {
          morse += morseCode[s.ToString()] + " ";
          //每个字符间添加空格
          morse += " ";
        }
      }
      return morse;
    }

    public static string morse2word(string morse)
    {
      string word = string.Empty;
      string[] morseSplit = morse.Split(' ');
      foreach (string str in morseSplit)
      {
        foreach (System.Collections.DictionaryEntry item in morseCode)
        {
          if (item.Value.ToString() == str)
          {
            word += item.Key.ToString();
            break;
          }
        }
      }
      return word;
    }
  }
}
