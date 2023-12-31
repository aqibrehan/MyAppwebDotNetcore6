﻿using MyApp.DataAccessLayer.Data;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.DataAccessLayer.Migrations;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void PaymentStatus(int Id, string SessionId, string PaymanetIntentId)
        {
           
            var OrderHeader = _context.OrderHeaders.FirstOrDefault(x => x.Id == Id  );
            OrderHeader.DateofPayment=DateTime.Now;
            OrderHeader.PaymentIntentId=PaymanetIntentId;
            OrderHeader.SessionId=SessionId;
        }

        public void Update(OrderHeader orderHeader)
        {
          _context.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int Id, string orderstatus, string? paymentStatus=null)
        {
            var order = _context.OrderHeaders.FirstOrDefault(x => x.Id == Id);
            if (order != null) 
            {
            order.Orderstatus = orderstatus;
            }
            if(paymentStatus!=null)
            {
                order.PaymentStatus=paymentStatus;
            }

        }
    }
}
