using System;

namespace CarServiceApp
{
    public class ServiceOrder
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string CarModel { get; set; }
        public string CarNumber { get; set; }
        public string ServiceType { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
    }
}