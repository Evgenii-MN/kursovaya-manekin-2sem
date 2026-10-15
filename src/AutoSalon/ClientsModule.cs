using System;
using System.Linq;
using System.Text.RegularExpressions;
namespace AutoSalon {
public class ClientsModule:IModule {
 public int Order{get{return 2;}}public string Name{get{return "Клиенты";}}
 public static int Add(Store s,string name,string phone,string email){
  name=Store.Text(name,"имя");phone=Store.Text(phone,"телефон");
  phone=Regex.Replace(phone,@"[\s()\-]","");email=Store.Text(email,"email").ToLowerInvariant();int id=0;
  s.Update(d=>{id=d.Clients.Count==0?1:d.Clients.Max(c=>c.Id)+1;d.Clients.Add(new Client{Id=id,Name=name,Phone=phone,Email=email});});return id;
 }
 public void Run(Store s){
  int n=Menu.Choose(Name,new[]{"Добавить клиента","Список клиентов","История покупок","Назад"});if(n<0||n==3)return;
  if(n==0){int id=Add(s,Menu.Input("ФИО"),Menu.Input("Телефон"),Menu.Input("Email"));Console.WriteLine("Создан клиент №"+id);}
  if(n==1)foreach(var c in s.Snapshot().Clients)Console.WriteLine("{0} | {1} | {2} | {3}",c.Id,c.Name,c.Phone,c.Email);
  if(n==2){int id=Menu.Number("ID клиента");var d=s.Snapshot();if(!d.Clients.Any(c=>c.Id==id))throw new ArgumentException("Клиент не найден.");
   foreach(var sale in d.Sales.Where(x=>x.ClientId==id))Console.WriteLine("{0:dd.MM.yyyy} | {1} | {2:N2} руб.",sale.Date,d.Cars.Single(c=>c.Id==sale.CarId).Model,sale.Amount);
  }Menu.Pause();
 }
}
}