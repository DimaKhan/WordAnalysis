using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MorfologAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            //Для класса WordAnalysis реализован шаблон Singleton с целью предотвратить повторную загрузку данных из файлов
            WordAnalysis wa = WordAnalysis.GetInstance();
          
            Console.Write("Выберите действие:\n1 - Морфологический анализ предложения\n2 - Морфологический анализ слова\nУкажите номер действия:");
            string inputText;
            string actionType = Console.ReadLine();
            Console.Clear();
            
            switch (actionType) 
            {
                case "1" :
                    Console.WriteLine("Морфологический анализ предложения\nВведите предложение:");
                    inputText = Console.ReadLine();
                    Console.WriteLine(wa.stringAnalysis(inputText, false));
                    break;
                case "2" :
                    Console.WriteLine("Морфологический анализ слова\nВведите слово:");
                    inputText = Console.ReadLine();
                    Console.WriteLine("Шаблоны ввода морфологических признаков\nСуществительные - [число],[падеж]\nГлаголы - [инф]\nГлаголы - [время],[число],[лицо]\nМестоимения - [падеж]");
                    Console.WriteLine("Введите морфологические характеристики согласно соответствующему шаблону:");
                    string morgfologChars = Console.ReadLine();
                    Console.WriteLine("Результат: " + wa.stringAnalysis(inputText, true, morgfologChars));
                    break;
                default: Console.WriteLine("Ошибка ввода"); break;
            }
            Console.ReadKey();
        }
    }
}
