using System;

namespace Common
{
    public static class CoreConstant
    {
        public static class OrderStatus
        {
            public const string CREATE_PENDING = "CREATE_PENDING";
            public const string CANCEL_RETRYLATER = "CANCEL_RETRYLATER";
            public const string CANCEL_STOCK_NOT_ENOUGH = "CANCEL_STOCK_NOT_ENOUGH";
            public const string CANCEL_RESTAURANT_ERROR = "CANCEL_RESTAURANT_ERROR";
            public const string CANCEL_BALANCE_NOT_ENOUGH = "CANCEL_BALANCE_NOT_ENOUGH";
            public const string CANCEL_PAYMENT_FAILED = "CANCEL_PAYMENT_FAILED";
            public const string CANCEL_SHIPPER_NOT_FOUND = "CANCEL_SHIPPER_NOT_FOUND";
            public const string PAID = "PAID";
            public const string READYTODELIVER = "READY_TO_DELIVER";
            public const string COMPLETED = "COMPLETED";
        }

        public static class PaymentStatus
        {
            public const string REFUNDED = "REFUNDED";
            public const string PAID = "PAID";
        }

        public static class TicketStatus
        {
            public const string CREATE_PENDING = "CREATE_PENDING";
            public const string APPROVED = "APPROVED";
            public const string CANCELED = "CANCELED";
            public const string DONE = "DONE";
        }

        public static class EventStatus
        {
            public const string NEW = "NEW";
            public const string PUBLISHED = "PUBLISHED";
        }

        public static class InventoryStatus
        {
            public const string AVAIABLE = "AVAIABLE";
            public const string LOCKING = "LOCKING";
        }

        public static class Topic
        {
            public const string ORDER_CREATE_PENDING = "ORDER_CREATE_PENDING";
            public const string TICKET_CREATE_PENDING = "TICKET_CREATE_PENDING";
            public const string RESTAURANT_RETRYLATER = "RESTAURANT_RETRYLATER";
            public const string RESTAURANT_STOCK_NOT_ENOUGH = "RESTAURANT_STOCK_NOT_ENOUGH";
            public const string RESTAURANT_ERROR = "RESTAURANT_ERROR";
            public const string PAYMENT_BALANCE_NOT_ENOUGH = "PAYMENT_BALANCE_NOT_ENOUGH";
            public const string TICKET_CANCEL_BALANCE_NOT_ENOUGH = "TICKET_CANCEL_BALANCE_NOT_ENOUGH";
            public const string PAYMENT_SUCCESS = "PAYMENT_SUCCESS";
            public const string PAYMENT_FAILED = "PAYMENT_FAILED";
            public const string TICKET_APPROVED = "TICKET_APPROVED";
            public const string TICKET_DONE = "TICKET_DONE";
            public const string SHIPPER_FOUND = "SHIPPER_FOUND";
            public const string SHIPPER_NOT_FOUND = "SHIPPER_NOT_FOUND";
        }
    }
}
