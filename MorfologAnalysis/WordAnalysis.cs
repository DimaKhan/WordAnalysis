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

    public sealed class WordAnalysis
    {
        //Список хранимых букв
        List<LetterData> letters = new List<LetterData>();
        //Список окнчаний без неизменяемой части
        List<OnlySuffixData> onlySuffix = new List<OnlySuffixData>();
        //Хеш таблица окончаний
        Hashtable suffixTable = new Hashtable();


        #region Конструтор класса
        //Загружаем данные из файлов
        private WordAnalysis()
        {
            //Загрузка окончаний в хеш таблицу
            StreamReader endData = new StreamReader(@"flexia.txt", Encoding.Default);
            string str;
            while (!endData.EndOfStream)
            {
                str = endData.ReadLine();
                suffixTable.Add(Int32.Parse(Regex.Match(str, @"[0-9]*").Value), Regex.Replace(str, @"^[0-9]|:|\s", ""));
            }
            endData.Close();

            //Загрузка неизменяемых частей
            StreamReader osnovaData = new StreamReader(@"word.txt", Encoding.Default);
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
        #endregion

        //Инициализируем единственный объект класса
        private static WordAnalysis wordAnalysis = new WordAnalysis();

        //Метод для получениядоступа к экземпляру класса
        public static WordAnalysis GetInstance()
        {
            return wordAnalysis;
        }

        #region Добавление слова
        //Добавление слова в словарь
        //Переменная _word это неизменяемая часть слова
        private void addWord(string _word, string _morfologData, int IdSuffix = -1)
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
        private string findWord(string _searchWord, bool _changeFrom, string _morfologInput = "")
        {
            string resultAnalysis = "";
            string suffixData;
            for (int i = 0; i < _searchWord.Length; i++)
            {
                //Если есть хоть одна буква с текущей позицией
                var resLetter = letters.Where(let => let.letter == _searchWord[i] && let.position == i);
                if (resLetter.Any())
                {
                    //Перебираем все найденные буквы 
                    foreach (LetterData let in resLetter)
                    {
                        //Являтеся ли буква последней для неизменяемой части слова
                        if (let.final)
                        {
                            suffixData = let.getSuffixData(_searchWord.Substring(i + 1), _searchWord.Substring(0, i + 1));
                            if (suffixData != "")
                            {
                                if (_changeFrom)
                                {
                                    string sklonenie = Regex.Match(this.suffixTable[let.IdSuffix].ToString(), @"(\w+|)," + _morfologInput).Value;
                                    if (sklonenie != "")
                                    {
                                        return _searchWord.Substring(0, i + 1) + sklonenie.Split(',')[0];
                                    }
                                    else
                                    {
                                        return "Ошибка ввода морфологических характеристик";
                                    }
                                }
                                string beginForm = Regex.Match(suffixTable[let.IdSuffix].ToString(), @"^\w+").Value;
                                return resultAnalysis = "начальная форма \"" + _searchWord.Substring(0, i + 1) + beginForm + "\"," + let.morfologData + ',' + suffixData;
                            }
                            //Если букв не осталось и слово не имеет неизменяемой части, 
                            //то возвращаем морфологическое описание из первого документа
                            if (let.IdSuffix == -1 && _searchWord.Substring(i + 1) == "")
                            {
                                if (_changeFrom) return "";
                                return let.morfologData;
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
                            if (_changeFrom)
                            {
                                string sklonenie = Regex.Match(this.suffixTable[os.IdSuffix].ToString(), @"(\w+|)," + _morfologInput).Value;
                                if (sklonenie != "")
                                {
                                    return sklonenie.Split(',')[0];
                                }
                                else
                                {
                                    return "Ошибка ввода морфологических характеристик";
                                }


                            }
                            string beginForm = Regex.Match(this.suffixTable[os.IdSuffix].ToString(), @"^\w+").Value;
                            return resultAnalysis = "начальная форма \"" + beginForm + "\"," + os.morfologData + ',' + onlySufMorfolog;
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
        //Вывод на экран результата морфологического анализа введённой строки
        public string stringAnalysis(string _inputStr, bool _changeForm, string _morfologInput = "")
        {
            string strResult = "";
            if (_changeForm)
            {
                strResult = findWord(_inputStr.ToLower(), true, _morfologInput);
                if (strResult == "")
                {
                    strResult = _inputStr + " - слово не найдено\n";
                }
            }
            else
            {
                string wordFinded;
                MatchCollection allWords = Regex.Matches(_inputStr.ToLower(), "[а-яА-Я]+");
                //Поочерёдно для каждого слова выполняем морфологический анализ
                foreach (Match wordInput in allWords)
                {
                    wordFinded = findWord(wordInput.Value, false);
                    if (wordFinded != "")
                    {
                        strResult += wordInput + " - " + wordFinded + "\n";

                    }
                    else
                    {
                        strResult += wordInput + " - слово не найдено\n";
                    }
                }
            }
            return strResult;
        }
        #endregion
    }
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

    class OnlySuffixData : CharData
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
        //Получить морфологический анализ для всего слова
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
        //Получить морфологический анализ изменяемой части слова
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
}
