using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MorfologAnalysis
{
    class Program
    {
        //Данные об окончаниях
        class Suffix 
        {
            //Неизменяемая часть слова
            public string osnova;
            //Таблица окончаний
            public Hashtable suffix = new Hashtable();
           
            public Suffix(string _osnova, string _suffix)
            {
                osnova = _osnova;
                string[] suffixData = _suffix.Split(';');
                string suffixKey, suffixVal;
                foreach (string sd in suffixData) 
                {
                    suffixKey = Regex.Match(sd, @"[а-яА-Я]*").Value;
                    suffixVal = Regex.Replace(sd, @"^[а-яА-Я]*,", "");
                    //Заполняем таблицу окончаний для добавленного  слова
                    if (!suffix.ContainsKey(suffixKey))
                    {
                        suffix.Add(Regex.Match(sd, @"[а-яА-Я]*").Value, suffixVal);
                    }
                    else 
                    {
                        suffix[suffixKey] += " или " + suffixVal;
                    }
                    
                }
            }
        }

        abstract class CharData 
        {   //Морфологический анализ неизменяемой части слова
            public string morfologData;
            //id окончания из второго файла
            public int IdSuffix;
            public List<Suffix> suffixes = new List<Suffix>();
            public abstract string getSuffixData(string _endWord, string _osnova = "");
        }

        class OnlySuffixData :CharData
        {
            public OnlySuffixData(Hashtable _suffixTable = null, int _IdSuffix = -1, string _morfologData = "") 
            {
                morfologData = _morfologData;
                IdSuffix = _IdSuffix;
                if (_suffixTable[_IdSuffix] != null)
                {
                    suffixes.Add(new Suffix("", _suffixTable[_IdSuffix].ToString()));
                }
            }
            public override string getSuffixData(string _endWord, string _osnova = "")
            {
                var resSuffix = this.suffixes.SingleOrDefault(suf => suf.suffix.ContainsKey(_endWord));
                if (resSuffix != null)
                {
                    return resSuffix.suffix[_endWord].ToString();
                }
                else
                {
                    return "";
                }
            }
        }

        //Данные о букве добавляемых слов
        class LetterData : CharData
        {
            //Является ли буква конечной для какого либо из добавленных слов
            public bool final;
            //Позиция буквы в добавляемом слове
            public int position;
            //Буква слова
            public char letter;
 
            public LetterData(char _letter, int _position, bool _final = false, string _osnova = "", Hashtable _suffixTable = null, int _IdSuffix = -1, string _morfologData = "") 
            {
                
                letter = _letter;
                position = _position;
                morfologData = _morfologData;
                final = _final;
                IdSuffix = _IdSuffix;
                if (IdSuffix != -1 && _suffixTable[_IdSuffix] != null)
                {
                   this.suffixes.Add(new Suffix(_osnova, _suffixTable[_IdSuffix].ToString()));
                }
            }

            public override string getSuffixData(string _endWord, string _osnova) 
            {
                var resSuffix = this.suffixes.SingleOrDefault(suf => suf.osnova == _osnova && suf.suffix.ContainsKey(_endWord));
                if (resSuffix != null)
                {
                    return resSuffix.suffix[_endWord].ToString();
                }
                else 
                {
                    return "";
                }
            }
        }
        
        class WordAnalysis 
        {
            //Список хранимых букв
            List<LetterData> letters = new List<LetterData>();
            //Список окнчаний без неизменяемой части
            List<OnlySuffixData> onlySuffix = new List<OnlySuffixData>(); 
            //Хеш таблица окончаний
            Hashtable suffixTable = new Hashtable();

            public WordAnalysis() 
            {
                //Загрузка окончаний в хеш таблицу
                StreamReader endData = new StreamReader(@"dict\flexia.txt", Encoding.Default);
                string str;
                while (!endData.EndOfStream)
                {
                    str = endData.ReadLine();
                    suffixTable.Add(Int32.Parse(Regex.Match(str, @"[0-9]*").Value), Regex.Replace(str, @"^[0-9]|:|\s", ""));
                }
                endData.Close();

                //Загрузка неизменяемых частей
                StreamReader osnovaData = new StreamReader(@"dict\word.txt", Encoding.Default);
                string[] wordDict;
                while (!osnovaData.EndOfStream)
                {
                    str = osnovaData.ReadLine();
                    wordDict = str.Split(':');
                    if (wordDict.Count() == 3)
                    {
                        addWord(wordDict[0].Trim(' '), wordDict[1].Trim(' '), Convert.ToInt32(wordDict[2].Trim(' ')));
                    }
                    addWord(wordDict[0].Trim(' '), wordDict[1].Trim(' '));
                }
                osnovaData.Close();
            }

            #region Добавление слова
            //Добавление слова
            //Переменная _word это неизменяемая часть слова
            public void addWord(string _word, string _morfologData, int IdSuffix = -1) 
            { 
                if (_word.Length == 0)
                {
                    this.onlySuffix.Add(new OnlySuffixData(this.suffixTable, IdSuffix, _morfologData));
                }
                else 
                {
                    for (int i = 0; i < _word.Length; i++)
                    {
                        if (i == _word.Length - 1)
                        {
                            //Если буква неизменямой части последняя, тогда берём описание из первого словаря
                            this.letters.Add(new LetterData(_word[i], i, true, _word, this.suffixTable, IdSuffix, _morfologData));
                        }
                        else
                        {
                            this.letters.Add(new LetterData(_word[i], i));
                        }
                    }
                }
            }
            #endregion

            #region Поиск слова
            //Поиск в словаре по входной строке
            public string findWord(string _searchWord)
            {
                string resultAnalysis = "";
                string suffixData;
                for (int i = 0; i < _searchWord.Length; i++) 
                {
                    var resLetter = letters.Where( let=> let.letter == _searchWord[i] && let.position == i);
                    if (resLetter.Any())
                    {
                        foreach (LetterData let in resLetter)
                        {
                           // Console.WriteLine(let.letter.ToString() + ' ' + let.position + ' ' + let.morfologData + ' '+let.final+ '\n');
                            if (let.final) 
                            {
                               // Console.WriteLine(_searchWord.Substring(0, i + 1) + ' ' + _searchWord.Substring(i+1));
                                if (_searchWord.Substring(i + 1) != "")
                                {
                                    //Если ещё остались буквы в слове, проверяем привязанные окончания
                                    suffixData = let.getSuffixData(_searchWord.Substring(i + 1), _searchWord.Substring(0, i + 1));
                                    if (suffixData != "")
                                    {
                                        
                                        return resultAnalysis = let.morfologData + ' ' + suffixData;
                                    }
                                }
                                else 
                                {
                                    //Если букв не осталось и слово не имеет неизменяемой части, 
                                    //то возвращаем морфологическое описание из первого документа
                                    if (let.IdSuffix == -1)
                                    {
                                        return let.morfologData;
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                        string onlySufMorfolog;
                        foreach (OnlySuffixData os in onlySuffix)
                        {
                            onlySufMorfolog = os.getSuffixData(_searchWord); 
                            if (onlySufMorfolog != "") 
                            {
                                return resultAnalysis = os.morfologData + ' ' + onlySufMorfolog;
                            }
                        }
                        //Сбросить состояние поиска слова, т.к. слово не найдено
                        resultAnalysis = "";
                        break; 
                    }
                }
                return resultAnalysis;

            }
            #endregion

            #region Вывод результата
            public void stringAnalysis(string _inputStr) 
            {
                string strResult;
                MatchCollection allWords = Regex.Matches(_inputStr.ToLower(), "[а-яА-Я]+");
                foreach (Match wordInput in allWords)
                {
                    strResult = findWord(wordInput.Value);
                    if (strResult != "")
                    {
                        Console.WriteLine(wordInput + " - " + strResult );
                    }
                    else
                    {
                        Console.WriteLine(wordInput + " - " + "слово не найдено");
                    }
                }
            }
            #endregion
        }


        static void Main(string[] args)
        {
            WordAnalysis wa = new WordAnalysis();
            string text = Console.ReadLine();
            wa.stringAnalysis(text);

            Console.ReadLine();
        }
    }
}
