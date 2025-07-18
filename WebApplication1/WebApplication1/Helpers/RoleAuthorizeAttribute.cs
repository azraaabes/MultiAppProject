using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Helpers
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string _requiredRoleId;

        public RoleAuthorizeAttribute(string roleId)
        {
            _requiredRoleId = roleId;
        }

        protected override bool AuthorizeCore(HttpContextBase ctx)
        {
            // 1) Önce Session’a bak
            var roleId = ctx.Session["RoleId"] as string;

            // 2) Session boşsa Redis’ten getir
            if (string.IsNullOrEmpty(roleId) && ctx.Session["Username"] != null)
            {
                roleId = RedisCacheHelper.Cache.GetString(
                             $"user:{ctx.Session["Username"]}:role");
                ctx.Session["RoleId"] = roleId;     // geri yaz
            }

            return roleId == _requiredRoleId;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterCtx)
        {
            // İstersen login sayfasına da yönlendirebilirsin
            filterCtx.Result = new RedirectResult("/Login/Index");
        }
    }
}