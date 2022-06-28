﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Web.Http;

namespace WebApp.Helper
{
    public class AuditTrail : ApiController
    {
        public string EventType { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string ObjectType { get; set; }
        public string PageUrl { get; set; }
        public string ContentDetail { get; set; }
        public string Remarks { get; set; }

        public string Method { get; set; }
        public string AbsoluteUri { get; set; }
        public string CrfToken { get; set; }
        //public string StatusCode { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        public string ReasonPhrase { get; set; }

        public void SaveTransactionLogs()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cId = User.Identity.GetUserId();
                        // For Transaction Logs 
                        ChangeLog log = new ChangeLog(); // Change Log Table

                        log.EventType = this.EventType;
                        log.Description = this.Description;
                        log.vMenuID = db.AspNetUsersMenus.Where(x => x.nvPageUrl == this.PageUrl).FirstOrDefault().vMenuID;
                        log.ObjectType = this.ObjectType;
                        log.EventName = this.EventName;
                        log.ContentDetail = this.ContentDetail;
                        log.Remarks = this.Remarks;
                        log.IPAddress = this.GetIPAddress();
                        log.CreatedDate = DateTime.Now;
                        log.CreatedByPK = cId;

                        db.ChangeLogs.Add(log);
                        db.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        public void SaveHttpClientLogs(string method, string uri, string token, string statuscode, bool trigger, string reason, string objecttype)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cId = User.Identity.GetUserId();
                        // For Transaction Logs 
                        HttpClientLog log = new HttpClientLog(); // Change Log Table

                        log.Method = method;
                        log.AbsoluteUri = uri;
                        log.CrfToken = token;
                        log.StatusCode = statuscode;
                        log.IsSuccessStatusCode = trigger;
                        log.ReasonPhrase = reason;
                        log.CreatedDate = DateTime.Now;
                        log.ObjectType = objecttype;

                        db.HttpClientLogs.Add(log);
                        db.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    catch (Exception)
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        private string GetIPAddress()
        {
            string ip = "";

            HttpContext cont = HttpContext.Current;
            string ipAddress = cont.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    ip = addresses[0];
                }
            }

            return cont.Request.ServerVariables["REMOTE_ADDR"]; 
        }
    }
}