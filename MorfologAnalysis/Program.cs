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
            public Hashtable suffix = new Hashtable();
           
            public Suffix(string _osnova, string _suffix)
            {
                osnova = _osnova;
                string[] suffixData = _suffix.Split(';');
                foreach (string sd in suffixData) 
                {
                    //Заполняем таблицу окончаний для добавленного  слова
                    suffix.Add(Regex.Match(sd, @"[а-яА-Я]*").Value, Regex.Replace(sd, @"^[а-яА-Я]*,", ""));
                    
                }
            }
        }

        class OnlySuffixData 
        {
              //Описание для неизменяемой части слова
            public string morfologData;
            public int IdSuffix;
            public List<Suffix> suffixes  = new List<Suffix>();
            public OnlySuffixData(Hashtable _suffixTable = null, int _IdSuffix = -1, string _morfologData = "") 
            {
                morfologData = _morfologData;
                IdSuffix = _IdSuffix;
                
                suffixes.Add(new Suffix("", _suffixTable[_IdSuffix].ToString()));
            }
            public string getSuffixData(string _endWord)
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
        class LetterData 
        {
            //Является ли буква конечной для какого либо из добавленных слов
            public bool final;
            //Позиция буквы в добавляемом слове
            public int position;
            //Буква
            public char letter;
            //Описание для неизменяемой части слова
            public string morfologData;
            public int IdSuffix;
            //Список окончаний для конечной буквы неизменяемой части слова
            List<Suffix> suffixes  = new List<Suffix>();
            
            public LetterData(char _letter, int _position, bool _final = false, string _osnova = "", Hashtable _suffixTable = null, int _IdSuffix = -1, string _morfologData = "") 
            {
                letter = _letter;
                position = _position;
                morfologData = _morfologData;
                final = _final;
                IdSuffix = _IdSuffix;
                if (IdSuffix != -1)
                {
                   suffixes.Add(new Suffix(_osnova, _suffixTable[_IdSuffix].ToString()));
                }
            }

            public string getSuffixData(string _osnova, string _endWord) 
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
            //Правильно было бы сделать поля статическими, но у нас один объект класса
            //Далее необходимо реализовать Singleton
            //Список хранимых букв
            List<LetterData> letters = new List<LetterData>();
            //Список окнчаний без неизменяемой части
            List<OnlySuffixData> onlySuffix = new List<OnlySuffixData>(); 
            //Хеш таблица окончаний
            Hashtable suffixTable = new Hashtable();

            public WordAnalysis() 
            {
                //Выгрузка окончаний в хеш таблицу
                StreamReader endData = new StreamReader(@"D:\dict\flexia.txt", Encoding.Default);
                string str;
                while (!endData.EndOfStream)
                {
                    str = endData.ReadLine();
                    suffixTable.Add(Int32.Parse(Regex.Match(str, @"[0-9]*").Value), Regex.Replace(str, @"^[0-9]|:|\s", ""));
                }
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
                                    suffixData = let.getSuffixData(_searchWord.Substring(0, i + 1), _searchWord.Substring(i + 1));
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
        
        }
        static void Main(string[] args)
        {
            WordAnalysis wa = new WordAnalysis();
            wa.addWord("asdf", "definition word", 2);
            wa.addWord("azdf", "my word", 2);
            wa.addWord("тест", "описание");
            Console.WriteLine(wa.findWord("тест"));
            Console.ReadLine();
        }
    }
}
