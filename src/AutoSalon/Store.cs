using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.Serialization.Json;
namespace AutoSalon {
public class Store {
 readonly string path; public bool Recovered {get;private set;}
 public Store(string file) {path=Path.GetFullPath(file);Directory.CreateDirectory(Path.GetDirectoryName(path));}
 FileStream Lock() {
  for(int i=0;i<100;i++) {
   try{return new FileStream(path+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);}
   catch(IOException){Thread.Sleep(20);}
  }
  throw new IOException("Хранилище занято. Повторите действие.");
 }
 static Database Read(string file) {
  using(var f=File.OpenRead(file)){return (Database)new DataContractJsonSerializer(typeof(Database)).ReadObject(f);}
 }
 Database Load() {
  Recovered=false;
  if(!File.Exists(path))return new Database();
  try{var d=Read(path);Validate(d);return d;}
  catch(Exception e) {
   if(!(e is IOException||e is System.Runtime.Serialization.SerializationException||e is ArgumentException))throw;
   if(!File.Exists(path+".bak"))throw new IOException("Повреждены данные, резервной копии нет.",e);
   var d=Read(path+".bak");Validate(d);Recovered=true;return d;
  }
 }
 public Database Snapshot(){using(Lock()){return Load();}}
 public void Update(Action<Database> change) {
  using(Lock()) {
   var d=Load();change(d);Validate(d);string tmp=path+".tmp";
   try {
    using(var f=new FileStream(tmp,FileMode.Create,FileAccess.Write,FileShare.None)) {
     new DataContractJsonSerializer(typeof(Database)).WriteObject(f,d);f.Flush(true);
    }
    if(File.Exists(path))File.Replace(tmp,path,Recovered?null:path+".bak");
    else File.Move(tmp,path);
   }finally{if(File.Exists(tmp))File.Delete(tmp);}
  }
 }
 public static string Text(string s,string label) {
  if(String.IsNullOrWhiteSpace(s))throw new ArgumentException("Заполните поле: "+label);
  return s.Trim();
 }
 public static void Validate(Database d) {
  if(d==null||d.Cars==null||d.Clients==null||d.Sales==null)throw new ArgumentException("Неполное хранилище.");
  if(d.Cars.Any(x=>x==null)||d.Clients.Any(x=>x==null)||d.Sales.Any(x=>x==null))throw new ArgumentException("Пустая запись.");
  if(d.Cars.Any(x=>x.Id<=0)||d.Clients.Any(x=>x.Id<=0)||d.Sales.Any(x=>x.Id<=0)||
   d.Cars.Select(x=>x.Id).Distinct().Count()!=d.Cars.Count||
   d.Clients.Select(x=>x.Id).Distinct().Count()!=d.Clients.Count||
   d.Sales.Select(x=>x.Id).Distinct().Count()!=d.Sales.Count)throw new ArgumentException("Повторный или неверный ID.");
  foreach(var c in d.Cars) {
   if(!System.Text.RegularExpressions.Regex.IsMatch(c.Vin??"","^[A-HJ-NPR-Z0-9]{17}$")||
    String.IsNullOrWhiteSpace(c.Model)||c.Price<=0||Decimal.Round(c.Price,2)!=c.Price||
    !new[]{"В наличии","Зарезервировано","Продано"}.Contains(c.Status))throw new ArgumentException("Неверная карточка автомобиля.");
  }
  if(d.Cars.Select(x=>x.Vin).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=d.Cars.Count)throw new ArgumentException("Повторный VIN.");
  foreach(var c in d.Clients) {
   if(String.IsNullOrWhiteSpace(c.Name)||!System.Text.RegularExpressions.Regex.IsMatch(c.Phone??"","^[+]?[0-9]{10,15}$")||
    !System.Text.RegularExpressions.Regex.IsMatch(c.Email??"",@"^[^@\s]+@[^@\s]+\.[^@\s]+$"))throw new ArgumentException("Неверная карточка клиента.");
  }
  if(d.Clients.Select(x=>x.Email).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=d.Clients.Count)throw new ArgumentException("Повторный email.");
  if(d.Sales.Select(x=>x.CarId).Distinct().Count()!=d.Sales.Count||
   d.Sales.Select(x=>x.Operation).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=d.Sales.Count)throw new ArgumentException("Повторная продажа или операция.");
  foreach(var s in d.Sales) {
   if(!d.Cars.Any(x=>x.Id==s.CarId&&x.Status=="Продано")||!d.Clients.Any(x=>x.Id==s.ClientId)||
    s.Amount<=0||Decimal.Round(s.Amount,2)!=s.Amount||s.Date==default(DateTime)||String.IsNullOrWhiteSpace(s.Operation))throw new ArgumentException("Неверная связь продажи.");
  }
  if(d.Cars.Any(c=>c.Status=="Продано"&&!d.Sales.Any(x=>x.CarId==c.Id)))throw new ArgumentException("Нет сделки проданного автомобиля.");
 }
}
}