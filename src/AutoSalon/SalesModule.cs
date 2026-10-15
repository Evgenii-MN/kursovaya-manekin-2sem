using System;
using System.Linq;
using System.Globalization;
namespace AutoSalon {
public class SalesModule:IModule {
 public int Order{get{return 3;}}public string Name{get{return "Продажи";}}
 public static int Sell(Store s,int car,int client,string operation) {
  operation=Store.Text(operation,"номер операции");int id=0;
  s.Update(d=>{
   if(d.Sales.Any(x=>String.Equals(x.Operation,operation,StringComparison.OrdinalIgnoreCase)))throw new ArgumentException("Номер операции уже учтён.");
   var c=d.Cars.SingleOrDefault(x=>x.Id==car);if(c==null)throw new ArgumentException("Автомобиль не найден.");
   if(!d.Clients.Any(x=>x.Id==client))throw new ArgumentException("Клиент не найден.");
   if(c.Status!="В наличии")throw new ArgumentException("Продать можно только автомобиль в наличии. Сначала снимите резерв.");
   id=d.Sales.Count==0?1:d.Sales.Max(x=>x.Id)+1;
   d.Sales.Add(new Sale{Id=id,CarId=car,ClientId=client,Amount=c.Price,Date=DateTime.Now,Operation=operation});c.Status="Продано";
  });return id;
 }
 public static string Preview(Store s,int id) {
  var d=s.Snapshot();var x=d.Sales.SingleOrDefault(a=>a.Id==id);if(x==null)throw new ArgumentException("Сделка не найдена.");
  var c=d.Cars.Single(a=>a.Id==x.CarId);var buyer=d.Clients.Single(a=>a.Id==x.ClientId);
  return String.Format(CultureInfo.GetCultureInfo("ru-RU"),
   "Учебная карточка сделки №{0}\nДата: {1:dd.MM.yyyy HH:mm}\nКлиент: {2}\nАвтомобиль: {3}\nVIN: {4}\nСумма: {5:N2} руб.\nНе является договором или платёжным документом.",
   x.Id,x.Date,buyer.Name,c.Model,c.Vin,x.Amount);
 }
 public void Run(Store s) {
  int n=Menu.Choose(Name,new[]{"Оформить продажу","Журнал и итог продаж","Карточка сделки","Назад"});if(n<0||n==3)return;
  if(n==0){int id=Sell(s,Menu.Number("ID автомобиля"),Menu.Number("ID клиента"),Menu.Input("Уникальный номер операции"));Console.WriteLine(Preview(s,id));}
  if(n==1){var d=s.Snapshot();foreach(var x in d.Sales)Console.WriteLine("Сделка {0} | авто {1} | клиент {2} | {3:N2} руб.",x.Id,x.CarId,x.ClientId,x.Amount);
   Console.WriteLine("Всего: {0}, выручка: {1:N2} руб.",d.Sales.Count,d.Sales.Sum(x=>x.Amount));}
  if(n==2)Console.WriteLine(Preview(s,Menu.Number("ID сделки")));Menu.Pause();
 }
}
}