using System;
using System.Linq;
namespace AutoSalon {
public class CatalogModule:IModule {
 public int Order{get{return 1;}} public string Name{get{return "Каталог автомобилей";}}
 public static int Add(Store s,string vin,string model,decimal price) {
  vin=Store.Text(vin,"VIN").ToUpperInvariant();model=Store.Text(model,"модель");int id=0;
  s.Update(d=>{id=d.Cars.Count==0?1:d.Cars.Max(c=>c.Id)+1;d.Cars.Add(new Car{Id=id,Vin=vin,Model=model,Price=price});});
  return id;
 }
 public static void Price(Store s,int id,decimal value){s.Update(d=>{
  var c=d.Cars.SingleOrDefault(x=>x.Id==id);if(c==null)throw new ArgumentException("Автомобиль не найден.");
  if(c.Status=="Продано")throw new ArgumentException("Цена проданного автомобиля зафиксирована.");c.Price=value;
 });}
 public static void Reserve(Store s,int id,bool reserve){s.Update(d=>{
  var c=d.Cars.SingleOrDefault(x=>x.Id==id);if(c==null)throw new ArgumentException("Автомобиль не найден.");
  if(c.Status!=(reserve?"В наличии":"Зарезервировано"))throw new ArgumentException("Недопустимый переход статуса.");
  c.Status=reserve?"Зарезервировано":"В наличии";
 });}
 public void Run(Store s) {
  int n=Menu.Choose(Name,new[]{"Добавить автомобиль","Каталог и поиск","Изменить цену","Зарезервировать","Снять резерв","Назад"});
  if(n<0||n==5)return;
  if(n==0){int id=Add(s,Menu.Input("VIN 17 знаков"),Menu.Input("Марка и модель"),Menu.Money("Цена, руб."));Console.WriteLine("Создан автомобиль №"+id);}
  if(n==1){Console.Write("Поиск по VIN/модели (Enter — все): ");string q=(Console.ReadLine()??"").Trim();
   foreach(var c in s.Snapshot().Cars.Where(c=>c.Vin.IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0||c.Model.IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0))
    Console.WriteLine("{0} | {1} | {2} | {3:N2} руб. | {4}",c.Id,c.Vin,c.Model,c.Price,c.Status);
  }
  if(n==2)Price(s,Menu.Number("ID автомобиля"),Menu.Money("Новая цена"));
  if(n==3||n==4)Reserve(s,Menu.Number("ID автомобиля"),n==3);
  Menu.Pause();
 }
}
}