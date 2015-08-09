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
            //Списо окончаний для конечной буквы неизменяемой части слова
            List<Suffix> suffixes  = new List<Suffix>();
            
            public LetterData(char _letter, int _position, bool _final = false, string _osnova = "", Hashtable _suffixTable = null, int _IdSuffix = -1, string _morfologData = "") 
            {
                letter = _letter;
                position = _position;
                morfologData = _morfologData;
                final = _final;
                if (_IdSuffix != -1)
                {
                   suffixes.Add(new Suffix(_osnova, _suffixTable[_IdSuffix].ToString()));
                }
            }

            public IEnumerable checkSuffix(string _osnova, string _endWord) 
            {

                var resSuffix = this.suffixes.Where(suf => suf.osnova == _osnova && suf.suffix.ContainsKey(_endWord));
                if (resSuffix.Any()) Console.WriteLine("Done!");
                return resSuffix;
            }
        }
        
        class WordAnalysis 
        {
            //Правильно было бы сделать поля статическими, но у нас один объект класса
            //Далее необходимо реализовать Singleton
            //Список хранимых букв
            List<LetterData> letters = new List<LetterData>();
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
            public void addWord(string _word, string _morfologData, int IdSuffix) 
            {
                for (int i = 0; i < _word.Length; i++) 
                {
                    if (i == _word.Length - 1)
                    {
                        //Если буква неизменямой части последняя, тогда берём описание из первого словаря
                        this.letters.Add(new LetterData(_word[i], i, true , _word, this.suffixTable, IdSuffix, _morfologData));
                    }
                    else
                    {
                        this.letters.Add(new LetterData(_word[i], i));
                    }
                }
            }
            #endregion

            #region Поиск слова
            //Поиск в словаре по входной строке
            public void findWord(string _searchWord)
            {
                
                for (int i = 0; i < _searchWord.Length; i++) 
                {
                    var resLetter = letters.Where( let=> let.letter == _searchWord[i] && let.position == i);
                    if (resLetter.Any())
                    {
                        foreach (LetterData let in resLetter)
                        {
                            Console.WriteLine(let.letter.ToString() + ' ' + let.position + ' ' + let.morfologData + ' '+let.final+ '\n');
                            if (let.final) 
                            {
                                Console.WriteLine(_searchWord.Substring(0, i + 1) + ' ' + _searchWord.Substring(i+1));
                                //С помощью LINQ найти окончания которые равны оставшемуся слову для данной буквы
                                let.checkSuffix(_searchWord.Substring(0, i + 1), _searchWord.Substring(i+1));
                            }
                        }
                    }
                    else 
                    { //Сбросить состояние поиска слова, т.к. слово не найдено
                        break; 
                    }
                }

            }
            #endregion
        
        }
        static void Main(string[] args)
        {
            WordAnalysis wa = new WordAnalysis();
            wa.addWord("asdf", "definition word", 2);
            wa.findWord("asdfыть");
            Console.ReadLine();
        }
    }
}
