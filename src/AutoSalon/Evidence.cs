using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace AutoSalon {
[DataContract] public class TestResult{[DataMember]public string Id;[DataMember]public string Name;[DataMember]public bool Passed;[DataMember]public string Detail;}
public static class Evidence {
 static List<TestResult> results=new List<TestResult>();static string folder;
 static Store Fresh(string tag){return new Store(Path.Combine(folder,tag,"salon.json"));}
 static void Assert(bool b){if(!b)throw new Exception("Результат не совпал с ожидаемым.");}
 static void Reject(Action a){try{a();}catch(ArgumentException){return;}throw new Exception("Недопустимое действие принято.");}
 static void Check(string id,string name,Action test){try{test();results.Add(new TestResult{Id=id,Name=name,Passed=true,Detail="Passed"});}
 catch(Exception e){results.Add(new TestResult{Id=id,Name=name,Passed=false,Detail=e.Message});}Console.WriteLine(id+" "+name+": "+(results.Last().Passed?"Passed":"Failed "+results.Last().Detail));}
 static int Car(Store s,string vin="XTA219010P0123456",decimal price=1200000.50m){return CatalogModule.Add(s,vin,"Lada Granta",price);}
 static int Buyer(Store s){return ClientsModule.Add(s,"Иванов Тест","+7 (900) 000-00-01","client01@example.test");}
 public static int Run(string[] args){
  if(args[0]!="--test"&&args[0]!="--demo"){Console.WriteLine("Параметры: --test папка | --demo папка");return 2;}
  folder=Path.GetFullPath(args.Length>1?args[1]:Path.Combine(Path.GetTempPath(),"AutoSalon-"+Guid.NewGuid()));
  if(Directory.Exists(folder)&&Directory.EnumerateFileSystemEntries(folder).Any())throw new ArgumentException("Для проверки нужна пустая папка.");
  Directory.CreateDirectory(folder);if(args[0]=="--demo"){Demo();return 0;}
  Check("CAT-01","Добавление VIN и нормализация",()=>{var s=Fresh("c1");Car(s,"xta219010p0123456");Assert(s.Snapshot().Cars.Single().Vin=="XTA219010P0123456");});
  Check("CAT-02","Неверная длина VIN",()=>{var s=Fresh("c2");Reject(()=>Car(s,"123"));Assert(s.Snapshot().Cars.Count==0);});
  Check("CAT-03","Недопустимая буква VIN",()=>{var s=Fresh("c3");Reject(()=>Car(s,"ITA219010P0123456"));});
  Check("CAT-04","Уникальность VIN без учёта регистра",()=>{var s=Fresh("c4");Car(s);Reject(()=>Car(s,"xta219010p0123456"));Assert(s.Snapshot().Cars.Count==1);});
  Check("CAT-05","Неверная цена и лишние десятичные знаки",()=>{var s=Fresh("c5");Reject(()=>Car(s,price:0));Reject(()=>Car(s,price:-0.01m));Reject(()=>Car(s,price:1.001m));});
  Check("CAT-06","Точная цена с копейками",()=>{var s=Fresh("c6");int id=Car(s);CatalogModule.Price(s,id,999999.99m);Assert(s.Snapshot().Cars.Single().Price==999999.99m);});
  Check("CAT-07","Неверная правка не изменяет файл",()=>{var s=Fresh("c7");int id=Car(s);Reject(()=>CatalogModule.Price(s,id,-1));Assert(s.Snapshot().Cars.Single().Price==1200000.50m);});
  Check("CAT-08","Резерв и снятие резерва",()=>{var s=Fresh("c8");int id=Car(s);CatalogModule.Reserve(s,id,true);Assert(s.Snapshot().Cars.Single().Status=="Зарезервировано");CatalogModule.Reserve(s,id,false);Assert(s.Snapshot().Cars.Single().Status=="В наличии");});
  Check("CAT-09","Повторное резервирование отклонено",()=>{var s=Fresh("c9");int id=Car(s);CatalogModule.Reserve(s,id,true);Reject(()=>CatalogModule.Reserve(s,id,true));});
  Check("CAT-10","Несуществующий автомобиль",()=>{var s=Fresh("c10");Reject(()=>CatalogModule.Price(s,999,1));});
  Check("CRM-01","Клиент и нормализация контактов",()=>{var s=Fresh("b1");Buyer(s);var c=s.Snapshot().Clients.Single();Assert(c.Phone=="+79000000001"&&c.Email=="client01@example.test");});
  Check("CRM-02","Пустое имя",()=>{var s=Fresh("b2");Reject(()=>ClientsModule.Add(s," ","+79000000001","a@example.test"));});
  Check("CRM-03","Неверный телефон",()=>{var s=Fresh("b3");Reject(()=>ClientsModule.Add(s,"Тест","abc","a@example.test"));});
  Check("CRM-04","Неверный email",()=>{var s=Fresh("b4");Reject(()=>ClientsModule.Add(s,"Тест","+79000000001","не-email"));});
  Check("CRM-05","Email уникален без учёта регистра",()=>{var s=Fresh("b5");Buyer(s);Reject(()=>ClientsModule.Add(s,"Другой","+79000000002","CLIENT01@EXAMPLE.TEST"));Assert(s.Snapshot().Clients.Count==1);});
  Check("CRM-06","Разные клиенты получают разные ID",()=>{var s=Fresh("b6");Buyer(s);int id=ClientsModule.Add(s,"Другой","+79000000002","b@example.test");Assert(id==2);});
  Check("SALE-01","Продажа сохраняет статус и сумму",()=>{var s=Fresh("s1");int c=Car(s),b=Buyer(s);SalesModule.Sell(s,c,b,"op-1");var d=s.Snapshot();Assert(d.Cars.Single().Status=="Продано"&&d.Sales.Single().Amount==1200000.50m);});
  Check("SALE-02","Повторная продажа запрещена",()=>{var s=Fresh("s2");int c=Car(s),b=Buyer(s);SalesModule.Sell(s,c,b,"op-1");Reject(()=>SalesModule.Sell(s,c,b,"op-2"));Assert(s.Snapshot().Sales.Count==1);});
  Check("SALE-03","Продажа резерва запрещена",()=>{var s=Fresh("s3");int c=Car(s),b=Buyer(s);CatalogModule.Reserve(s,c,true);Reject(()=>SalesModule.Sell(s,c,b,"op-1"));Assert(s.Snapshot().Sales.Count==0);});
  Check("SALE-04","Несуществующий клиент",()=>{var s=Fresh("s4");int c=Car(s);Reject(()=>SalesModule.Sell(s,c,999,"op-1"));Assert(s.Snapshot().Cars.Single().Status=="В наличии");});
  Check("SALE-05","Несуществующий автомобиль",()=>{var s=Fresh("s5");int b=Buyer(s);Reject(()=>SalesModule.Sell(s,999,b,"op-1"));});
  Check("SALE-06","Повторный номер операции",()=>{var s=Fresh("s6");int c=Car(s),b=Buyer(s);int c2=Car(s,"XTA219010P0123457");SalesModule.Sell(s,c,b,"op-1");Reject(()=>SalesModule.Sell(s,c2,b,"OP-1"));Assert(s.Snapshot().Cars.Single(x=>x.Id==c2).Status=="В наличии");});
  Check("SALE-07","Цена проданного авто неизменна",()=>{var s=Fresh("s7");int c=Car(s),b=Buyer(s);SalesModule.Sell(s,c,b,"op-1");Reject(()=>CatalogModule.Price(s,c,5));});
  Check("SALE-08","Карточка сделки содержит VIN и сумму",()=>{var s=Fresh("s8");int c=Car(s),b=Buyer(s);int id=SalesModule.Sell(s,c,b,"op-1");string text=SalesModule.Preview(s,id);Assert(text.Contains("XTA219010P0123456")&&text.Replace(" ","").Replace("\u00a0","").Replace("\u202f","").Contains("1200000,50")&&text.Contains("Иванов Тест"));});
  Check("SALE-09","Сумма журнала двух продаж",()=>{var s=Fresh("s9");int c=Car(s),b=Buyer(s);int c2=Car(s,"XTA219010P0123457",100.25m);SalesModule.Sell(s,c,b,"op-1");SalesModule.Sell(s,c2,b,"op-2");Assert(s.Snapshot().Sales.Sum(x=>x.Amount)==1200100.75m);});
  Check("SALE-10","Два хранилища одновременно продают одно авто",()=>{var s=Fresh("s10");int c=Car(s),b=Buyer(s);var t=new Store(Path.Combine(folder,"s10","salon.json"));int ok=0,bad=0;
   Parallel.Invoke(()=>{try{SalesModule.Sell(s,c,b,"op-1");System.Threading.Interlocked.Increment(ref ok);}catch(ArgumentException){System.Threading.Interlocked.Increment(ref bad);}},
    ()=>{try{SalesModule.Sell(t,c,b,"op-2");System.Threading.Interlocked.Increment(ref ok);}catch(ArgumentException){System.Threading.Interlocked.Increment(ref bad);}});
   Assert(ok==1&&bad==1&&s.Snapshot().Sales.Count==1);
  });
  Check("DATA-01","Повторное открытие JSON",()=>{var s=Fresh("d1");Car(s);Buyer(s);var t=new Store(Path.Combine(folder,"d1","salon.json"));Assert(t.Snapshot().Cars.Count==1&&t.Snapshot().Clients.Count==1);});
  Check("DATA-02","Повреждение основной копии",()=>{var s=Fresh("d2");Car(s);Buyer(s);File.WriteAllText(Path.Combine(folder,"d2","salon.json"),"{broken");var d=s.Snapshot();Assert(s.Recovered&&d.Cars.Count==1);});
  Check("DATA-03","Повреждение без резервной копии",()=>{var s=Fresh("d3");File.WriteAllText(Path.Combine(folder,"d3","salon.json"),"{broken");try{s.Snapshot();}catch(IOException){return;}throw new Exception("Повреждение не обнаружено.");});
  Check("DATA-04","Нарушение связи сделки",()=>{var s=Fresh("d4");Reject(()=>s.Update(d=>d.Sales.Add(new Sale{Id=1,CarId=99,ClientId=99,Amount=1,Date=DateTime.Now,Operation="bad"})));Assert(s.Snapshot().Sales.Count==0);});
  Check("DATA-05","Проданный автомобиль без сделки",()=>{var s=Fresh("d5");Car(s);Reject(()=>s.Update(d=>d.Cars[0].Status="Продано"));});
  Check("DATA-06","Ошибка изменения не оставляет часть операции",()=>{var s=Fresh("d6");Car(s);try{s.Update(d=>{d.Cars[0].Price=5;throw new InvalidOperationException();});}catch(InvalidOperationException){}Assert(s.Snapshot().Cars[0].Price==1200000.50m);});
  Check("DATA-07","Сохранение после восстановления сохраняет исправную копию",()=>{var s=Fresh("d7");Car(s);Buyer(s);string file=Path.Combine(folder,"d7","salon.json");File.WriteAllText(file,"bad");ClientsModule.Add(s,"Тест2","+79000000002","two@example.test");Assert(s.Snapshot().Cars.Count==1&&s.Snapshot().Clients.Count==1);});
  Check("MENU-01","Переход вверх из первого пункта",()=>Assert(Menu.Move(0,-1,4)==3));
  Check("MENU-02","Переход вниз из последнего пункта",()=>Assert(Menu.Move(3,1,4)==0));
  Check("MENU-03","Пустой список не принимается",()=>Reject(()=>Menu.Move(0,1,0)));
  using(var f=File.Create(Path.Combine(folder,"results.json")))new DataContractJsonSerializer(typeof(List<TestResult>)).WriteObject(f,results);
  Console.WriteLine("Итого: "+results.Count+", пройдено: "+results.Count(x=>x.Passed));
  return results.All(x=>x.Passed)?0:1;
 }
 static void Demo(){
  var s=Fresh("demo");int c=Car(s),b=Buyer(s);CatalogModule.Reserve(s,c,true);
  Console.WriteLine("1. Созданы автомобиль и клиент; статус: "+s.Snapshot().Cars[0].Status);
  CatalogModule.Reserve(s,c,false);int sale=SalesModule.Sell(s,c,b,"demo-sale-001");
  Console.WriteLine("2. Резерв снят, продажа завершена.\n"+SalesModule.Preview(s,sale));
  try{SalesModule.Sell(s,c,b,"demo-sale-002");}catch(ArgumentException e){Console.WriteLine("3. Повторная продажа отклонена: "+e.Message);}
  Console.WriteLine("4. После повторного открытия: "+new Store(Path.Combine(folder,"demo","salon.json")).Snapshot().Sales.Count+" сделка.");
 }
}
}