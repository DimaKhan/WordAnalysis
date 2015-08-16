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

    /// <summary>
    /// WordAnalysis - основной класс программы, который
    /// добавляет слова в словарь
    /// определяет морфологические характеристики введённых слов
    /// склоняет слово согласно введённым морфологическим характеристикам
    /// </summary>
    public sealed class WordAnalysis
    {
        //Список букв, которые есть в словах указанных в словаре
        List<LetterData> letters = new List<LetterData>();
        //Список окнчаний без неизменяемой части
        List<OnlySuffixData> onlySuffix = new List<OnlySuffixData>();
        //Хеш таблица неизменяемых частей
        Hashtable suffixTable = new Hashtable();


        #region Конструтор класса
        //В конструкторе загружаем данные из файлов
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
        /// <summary>
        /// Функция предназанчена для добавления слова в словарь, который находится в оперативной памяти
        /// </summary>
        /// <param name="_word">Неизменяемая часть слова</param>
        /// <param name="_morfologData">Морфологические описания из файла words.txt</param>
        /// <param name="IdSuffix">id неизменямой части слова из файла flexia.txt, если нет неизменямой части, то значение -1</param>
        private void addWord(string _word, string _morfologData, int IdSuffix = -1)
        {
            if (_word.Length == 0)
            {
                this.onlySuffix.Add(new OnlySuffixData(_morfologData,  this.suffixTable, IdSuffix));
            }
            else
            {
                for (int i = 0; i < _word.Length; i++)
                {
                    if (i == _word.Length - 1)
                    {
                        //Если буква неизменямой части последняя, тогда берём описание из первого словаря
                        this.letters.Add(new LetterData(_word[i], i, _morfologData, true, _word, this.suffixTable, IdSuffix));
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
        /// <summary>
        /// Осуществляет поиск слова по заданным параметрам
        /// </summary>
        /// <param name="_searchWord">Введённое пользователем слово</param>
        /// <param name="_changeFrom">Атрибут указывает на режим работы
        /// true - склоняет слово
        /// false - находит морфологические признаки по каждому слову</param>
        /// <param name="_morfologInput">Желаемые морфлогические характеристики для склоняемого слова</param>
        /// <returns>Возвращает либо строку с морфологическими характеристиками, 
        /// либо слово с заданными характеристиками</returns>
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
        /// <summary>
        /// Принмиает входные данные по режиму работы программы и выводит результаты работы
        /// </summary>
        /// <param name="_inputStr">Входная строка для обработки</param>
        /// <param name="_changeForm">Выбрать сценраий работы программы
        /// true - изменить слово по заданным морфологическим признакам
        /// false - получить морфлогические данные по каждому введённому слову</param>
        /// <param name="_morfologInput">Желаемые морфологические характиристики для склоняемого слова</param>
        /// <returns>Возвращает строку с морфологическими характеристиками по каждому слову или изменённое слово 
        /// согласно морфологичесикм харатеристикам</returns>
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

    /// <summary>
    /// Класс для хранения изменяемой части слова
    /// </summary>
    class Suffix
    {
        //Неизменяемая часть слова
        public string osnova;
        //Таблица окончаний
        public Hashtable suffix = new Hashtable();

        #region Конструктор класса Suffix
        /// <summary>
        /// Конструктор класса создаёт объект содержащий основу слова и соответствующие ей 
        /// возможные варианты неизменяемой части слова с морфологическими характеристиками
        /// </summary>
        /// <param name="_osnova">Неизменяемая часть слова</param>
        /// <param name="_suffix">Строка содержащая все варианты окончаний для данного слова</param>
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
        #endregion
    }

    //Авбстрактный класс содержащий общую информацию для хранимых символов
    abstract class CharData
    {   //Морфологические данные неизменяемой части слова
        public string morfologData;
        //id неизменяемой части из файла flexia
        public int IdSuffix;
        public List<Suffix> suffixes = new List<Suffix>();
        public abstract string getSuffixData(string _endWord, string _osnova = "");
    }

    //Класс содержащий данные о словах у которых нет неизменяемой части
    class OnlySuffixData : CharData
    {
        /// <summary>
        /// Конструктор класса создаёт объект в котором хранится описание из файла words и соответствующей части
        /// из файла flexia
        /// </summary>
        /// <param name="_morfologData">морфологические признаки из файла words.txt</param>
        /// <param name="_suffixTable">Хеш-таблица с данными по неизменяемой части</param>
        /// <param name="_IdSuffix">id необходмой нам изменяемой части</param>
        public OnlySuffixData(string _morfologData,  Hashtable _suffixTable, int _IdSuffix = -1)
        {
            morfologData = _morfologData;
            IdSuffix = _IdSuffix;
            if (_suffixTable[_IdSuffix] != null)
            {
                suffixes.Add(new Suffix("", _suffixTable[_IdSuffix].ToString()));
            }
        }
        //Получить морфологический анализ для слова без неизменяемой части
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

    //Данные о буквах в словах неизменяемой части
    class LetterData : CharData
    {
        //Является ли буква конечной для какого либо из добавленных слов
        public bool final;
        //Позиция буквы в добавляемом слове
        public int position;
        //Буква слова
        public char letter;
        /// <summary>
        /// Конструктор создаёт объект содержащий данные по букве
        /// </summary>
        /// <param name="_letter">Буква</param>
        /// <param name="_position">Позиция буквы в слове</param>
        /// <param name="_morfologData">Морфологические признаки неизменяемой части слова</param>
        /// <param name="_final">Является ли буква последней в неизменяемой части</param>
        /// <param name="_osnova">Неизменяемая часть слова</param>
        /// <param name="_suffixTable">Таблица окончаний</param>
        /// <param name="_IdSuffix">id изменяемой части слова</param>
        public LetterData(char _letter, int _position, string _morfologData = "", bool _final = false, string _osnova = "", Hashtable _suffixTable = null, int _IdSuffix = -1)
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
