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
        struct EndWord 
        {
            public EndWord(string _osnova, string endWords)
            {
                osnova = _osnova;
                endLetters = endWords;
                Console.WriteLine(endWords);
            }
            //Неизменяемая часть слова 
            public string osnova;
            //Окончания
            public string endLetters;
            

            

        }
        
        //Данные о букве добавляемых слов
        class LetterData 
        {
            //Позиция буквы в добавляемом слове
            public int position;
            //Буква
            public char letter;
            //Описание для неизменяемой части слова
            public string morfologData;
            //Списо окончаний для конечной буквы неизменяемой части слова
            List<EndWord> endWords  = new List<EndWord>();
            
            public int IDEndWord;
            public LetterData(char _letter, int _position,  Hashtable _endWordsTable = null, int _IDEndword = -1, string _morfologData = "", string _osnova = "") 
            {
                letter = _letter;
                position = _position;
                morfologData = _morfologData;
                if (_IDEndword != -1)
                {
                    endWords.Add(new EndWord(_osnova, _endWordsTable[_IDEndword].ToString()));
                }

                
            }

        }
        class WordAnalysis 
        {
            //Список хранимых букв
            List<LetterData> letters = new List<LetterData>();

            //Добавляем слово
            //Переменная _word это неизменяемая часть слова
            public void addWord(string _word, string _morfologData, int IDEndWord, Hashtable _endWordsTable) 
            {
                for (int i = 0; i < _word.Length; i++) 
                {
                    if (i == _word.Length - 1)
                    {
                        //Если буква неизменямой части последняя, тогда берём описание из первого словаря
                        this.letters.Add(new LetterData(_word[i], i, _endWordsTable, IDEndWord, _morfologData, _word));
                        
                    }
                    else
                    {
                        this.letters.Add(new LetterData(_word[i], i));
                    }
                }

            }
            //Поиск в словаре по входной строке
            public void findWord(string _searchWord)
            {
                
                for (int i = 0; i < _searchWord.Length; i++) 
                {
                    var tempList = letters.Where( let=> let.letter == _searchWord[i] && let.position == i);
                    if (tempList.Any())
                    {
                        foreach (LetterData resultletter in tempList)
                        {
                            //Console.WriteLine(resultletter.letter.ToString() + ' ' + resultletter.position + ' ' + resultletter.morfologData + '\n');
                        }
                    }
                    else 
                    { //Сбросить состояние поиска слова, т.к. слово не найдено
                        break; 
                    }
                }
                
            }
        }
        static void Main(string[] args)
        {
            //Выгрузка окончаний в хеш таблицу
            Hashtable endWordsTable = new Hashtable();
            StreamReader endData = new StreamReader(@"D:\dict\flexia.txt",Encoding.Default);

            string str;
            while (!endData.EndOfStream)
            {
                str = endData.ReadLine();
                endWordsTable.Add(Int32.Parse(Regex.Match(str, @"[0-9]*").Value), Regex.Replace(str, @"^[0-9]|:|\s", ""));
            }
          

            

            WordAnalysis wa = new WordAnalysis();
            wa.addWord("asdf", "definition word", 2, endWordsTable);
            wa.findWord("asdf");
           

            Console.ReadLine();
        }
    }
}
