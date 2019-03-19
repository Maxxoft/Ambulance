using System;
namespace Ambulance.ObjectModel
{
    public enum OrderStatus
    {
        New = 0,
        Assigned = 1,
        OnWayToPatient = 2,
        OnPatientPlace = 3,
        OnWayToClinique = 4,
        Cancelled = 5,
        Done = 6
    }

    public class Order 
    {
		public int OrderId { get; set; }
        public string AddressFrom { get; set; }
        public double AddressFromLat { get; set; }
        public double AddressFromLng { get; set; }
        public string AddressTo { get; set; }
        public double AddressToLat { get; set; }
        public double AddressToLng { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string SickName { get; set; }
        public string SickPhone { get; set; }
        public string AutoData { get; set; }
        public string Comment { get; set; }
        public OrderStatus Status { get; set; }
        public double Distance { get; set; }
        public DateTime CalcDistanceDate { get; set; }
    }

    public static class OrderHelper
    {
        public static string EligibleActionName(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.New:
                    return "Взять заявку на исполнение";
                case OrderStatus.Assigned:
                    return "Выехать к пациенту";
                case OrderStatus.OnWayToPatient:
                    return "Прибытие к пациенту";
                case OrderStatus.OnPatientPlace:
                    return "Выехать в медучреждение";
                case OrderStatus.OnWayToClinique:
                    return "Заявка выполнена";
                default:
                    return null;
            }
        }

        public static string EligibleActionConfirmation(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.New:
                    return "Взять заявку на исполнение?";
                case OrderStatus.Assigned:
                    return "Установить отметку о выезде к пациенту?";
                case OrderStatus.OnWayToPatient:
                    return "Установить отметку о прибытии к пациенту?";
                case OrderStatus.OnPatientPlace:
                    return "Установить отметку о выезде в медучреждение?";
                case OrderStatus.OnWayToClinique:
                    return "Установить отметку о выполнении заявки?";
                default:
                    return null;
            }
        }
    }
}
