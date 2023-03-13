using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


public class Localization
{
    public static List<string> LanguageNames = new List<string>();

    public static Dictionary<string, List<string>> AllTextDatas = new Dictionary<string, List<string>>();
    public static string ServerLanguage { get; set; }

    public static void LoadData(string path)
    {
        string line;

        // Read the file and display it line by line.  
        var file = new StreamReader(path + "/SrvConfigs/ServerLocalization.csv");
        line = file.ReadLine();
        List<string> languages = new List<string>();
        string key = XuLyDong(line, languages);

        if (key != "KEY")
        {
            // config loi
            return;
        }

        LanguageNames = languages;

        while ((line = file.ReadLine()) != null)
        {
            var localization_of_key = new List<string>();
            key = XuLyDong(line, localization_of_key);
            if (key != "KEY")
            {
                AllTextDatas[key] = localization_of_key;
            }
        }

        file.Close();
    }

    /// <summary>
    /// Read a single line of Comma-Separated Values from the file.
    /// </summary>

    static List<string> ReadCSV(string line, List<string> mTemp)
    {
        mTemp.Clear();
        bool insideQuotes = false;
        int wordStart = 0;

        line = line.Trim();
        if (string.IsNullOrEmpty(line)) return null;
        line = line.Replace("\\n", "\n");

        for (int i = wordStart, imax = line.Length; i < imax; ++i)
        {
            char ch = line[i];

            if (ch == ',')
            {
                if (!insideQuotes)
                {
                    mTemp.Add(line.Substring(wordStart, i - wordStart));
                    wordStart = i + 1;
                }
            }
            else if (ch == '"')
            {
                if (insideQuotes)
                {
                    if (i + 1 >= imax)
                    {
                        mTemp.Add(line.Substring(wordStart, i - wordStart).Replace("\"\"", "\""));
                        return mTemp;
                    }

                    if (line[i + 1] != '"')
                    {
                        mTemp.Add(line.Substring(wordStart, i - wordStart).Replace("\"\"", "\""));
                        insideQuotes = false;

                        if (line[i + 1] == ',')
                        {
                            ++i;
                            wordStart = i + 1;
                        }
                    }
                    else ++i;
                }
                else
                {
                    wordStart = i + 1;
                    insideQuotes = true;
                }
            }
        }

        if (wordStart < line.Length)
        {
            mTemp.Add(line.Substring(wordStart, line.Length - wordStart));
        }
        return mTemp;
    }

    static string XuLyDong(string line, List<string> localizations)
    {
        var result = ReadCSV(line, localizations);
        if (result == null || result.Count < 2) return string.Empty;
        else
        {
            string key = result[0];
            localizations.RemoveAt(0);

            return key;
        }
    }

    public static void LoadThemNgonNgu(string path, string config)
    {
        string line;

        // Read the file and display it line by line.  
        var file = new StreamReader(path + config);
        line = file.ReadLine();

        List<string> languages = new List<string>();
        string key = XuLyDong(line, languages);

        if (key != "KEY")
        {
            // config loi
            return;
        }

        while ((line = file.ReadLine()) != null)
        {
            var localization_of_key = new List<string>();
            key = XuLyDong(line, localization_of_key);

            if (key != "KEY")
            {
                if (AllTextDatas.ContainsKey(key) == false)
                {
                    AllTextDatas.Add(key, new List<string>());
                }

                for (int i = 0; i < localization_of_key.Count; ++i)
                {
                    if (i < languages.Count)
                    {
                        var lang = languages[i];

                        bool haveLang = false;
                        for (int l = 0; l < LanguageNames.Count; ++l)
                        {
                            if (LanguageNames[l] == lang)
                            {
                                var list = AllTextDatas[key];
                                if (list.Count > l)
                                {
                                    if (string.IsNullOrEmpty(list[l]))
                                        list[l] = localization_of_key[i];
                                }
                                else
                                {
                                    for (int c = list.Count; c < l; ++c)
                                    {
                                        list.Add(string.Empty);
                                    }
                                    list.Add(localization_of_key[i]);
                                }
                                haveLang = true;
                                break;
                            }
                        }
                        if (haveLang == false)
                        {
                            var langID = LanguageNames.Count;
                            LanguageNames.Add(lang);

                            var list = AllTextDatas[key];
                            if (list.Count > langID)
                            {
                                if (string.IsNullOrEmpty(list[langID]))
                                    list[langID] = localization_of_key[i];
                            }
                            else
                            {
                                for (int c = list.Count; c < langID; ++c)
                                {
                                    list.Add(string.Empty);
                                }
                                list.Add(localization_of_key[i]);
                            }
                        }
                    }
                }
            }
        }

        file.Close();
    }

    public static string Get(string key, string lang = "")
    {
        if (string.IsNullOrEmpty(lang))
        {
            lang = ServerLanguage;
        }
        string result = string.Empty;
        if (AllTextDatas.ContainsKey(key))
        {
            var localization_of_key = AllTextDatas[key];
            var language_index = LanguageNames.IndexOf(lang);
            if (language_index >= 0 && localization_of_key.Count > language_index)
            {
                result = localization_of_key[language_index];
            }
            else if (localization_of_key.Count > 0)
            {
                result = localization_of_key[0];
            }
        }

        if (string.IsNullOrEmpty(result))
        {
            result = key;
        }
        return result;
    }
}