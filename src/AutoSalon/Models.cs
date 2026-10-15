using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
namespace AutoSalon {
[DataContract] public class Car {
 [DataMember] public int Id; [DataMember] public string Vin;
 [DataMember] public string Model; [DataMember] public decimal Price;
 [DataMember] public string Status="В наличии";
}
[DataContract] public class Client {
 [DataMember] public int Id; [DataMember] public string Name;
 [DataMember] public string Phone; [DataMember] public string Email;
}
[DataContract] public class Sale {
 [DataMember] public int Id; [DataMember] public int CarId;
 [DataMember] public int ClientId; [DataMember] public decimal Amount;
 [DataMember] public DateTime Date; [DataMember] public string Operation;
}
[DataContract] public class Database {
 [DataMember] public List<Car> Cars=new List<Car>();
 [DataMember] public List<Client> Clients=new List<Client>();
 [DataMember] public List<Sale> Sales=new List<Sale>();
}
public interface IModule {int Order{get;} string Name{get;} void Run(Store store);}
}