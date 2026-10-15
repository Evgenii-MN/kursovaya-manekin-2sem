using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.IO;
namespace AutoSalon {
class Program {
 static int Main(string[] args){
  Console.InputEncoding=Encoding.UTF8;Console.OutputEncoding=Encoding.UTF8;
  try {
   if(args.Length>0){
    var t=Assembly.GetExecutingAssembly().GetType("AutoSalon.Evidence");
    if(t==null){Console.WriteLine("Средства проверки добавляются при интеграции.");return 0;}
    return (int)t.GetMethod("Run").Invoke(null,new object[]{args});
   }
   var store=new Store(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"data","salon.json"));
   store.Snapshot();if(store.Recovered){Console.WriteLine("Восстановлено из резервной копии.");Menu.Pause();}
   var modules=Assembly.GetExecutingAssembly().GetTypes().Where(t=>typeof(IModule).IsAssignableFrom(t)&&!t.IsInterface)
    .Select(t=>(IModule)Activator.CreateInstance(t)).OrderBy(t=>t.Order).ToArray();
   while(true){
    int n=Menu.Choose("АвтоПлюс",modules.Select(t=>t.Name).Concat(new[]{"Выход"}).ToArray());
    if(n<0||n==modules.Length)return 0;
    try{modules[n].Run(store);}catch(Exception e){Console.WriteLine("Операция отклонена: "+e.Message);Menu.Pause();}
   }
  }catch(Exception e){Console.WriteLine("Ошибка: "+(e.InnerException??e).Message);return 1;}
 }
}
}