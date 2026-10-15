using System;
using System.Globalization;
namespace AutoSalon {
public static class Menu {
 public static int Move(int current,int step,int count){if(count<=0)throw new ArgumentException("Нет пунктов.");return (current+step%count+count)%count;}
 public static int Choose(string title,string[] items) {
  int selected=0;while(true){
   Console.Clear();Console.WriteLine("АС БАС / "+title);
   Console.WriteLine("Раскрытое меню: ↑ ↓ выбор, Enter открыть, Esc назад");
   for(int i=0;i<items.Length;i++)Console.WriteLine((selected==i?" > ":"   ")+items[i]);
   var key=Console.ReadKey(true).Key;
   if(key==ConsoleKey.UpArrow)selected=Move(selected,-1,items.Length);
   if(key==ConsoleKey.DownArrow)selected=Move(selected,1,items.Length);
   if(key==ConsoleKey.Enter)return selected;if(key==ConsoleKey.Escape)return -1;
  }
 }
 public static string Input(string title){Console.Write(title+": ");return Store.Text(Console.ReadLine(),title);}
 public static int Number(string title){int n;if(!Int32.TryParse(Input(title),out n)||n<1)throw new ArgumentException("Нужен положительный ID.");return n;}
 public static decimal Money(string title){
  decimal n;string s=Input(title);if(!Decimal.TryParse(s,NumberStyles.AllowDecimalPoint|NumberStyles.AllowLeadingSign,CultureInfo.GetCultureInfo("ru-RU"),out n)||n<=0)
   throw new ArgumentException("Нужна положительная цена, например 1200000,50.");return n;
 }
 public static void Pause(){Console.WriteLine("Нажмите любую клавишу.");Console.ReadKey(true);}
}
}