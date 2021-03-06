﻿using AutoMapper;
using Newtonsoft.Json;
using ResourceMetadata.Model;
using ResourceMetadata.Service;
using ResourceMetadata.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using owin = Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;

namespace ResourceMetadata.Web.Controllers
{
    public class UserController : ApiController
    {
        private readonly IUserService userService;

        private readonly UserManager<ApplicationUser> userManager;
        public UserController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            this.userService = userService;
            this.userManager = userManager;

            //Todo: This needs to be moved from here.
            this.userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                {
                    AllowOnlyAlphanumericUserNames = false
                };
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().Authentication;
            }
        }

        public IHttpActionResult Get()
        {
            if (System.Threading.Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                return Ok();
            }

            return Unauthorized();
        } 
  
        [HttpPut]
        public IHttpActionResult LogOut()
        {
            AuthenticationManager.SignOut();
            return Ok();
        }

       
        [OverrideAuthorization]
        public async Task<IHttpActionResult> Post(RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                switch (viewModel.Action)
                {
                    case Enums.LoginActions.Login:
                        {
                            var user = userManager.FindByName(viewModel.Email);

                            if (user == null)
                            {
                                return new ResourceMetadata.Web.Helpers.InvalidUserResult(Request);
                            }

                            await SignInAsync(user, isPersistent: false);
                            return Ok();
                        }
                    case Enums.LoginActions.Register:
                        {
                            try
                            {
                                ApplicationUser user = new ApplicationUser();
                                Mapper.Map(viewModel, user);
                                var identityResult = await userManager.CreateAsync(user);

                                if (identityResult.Succeeded)
                                {
                                    await SignInAsync(user, isPersistent: false);
                                }
                                else
                                {
                                    foreach (var error in identityResult.Errors)
                                    {

                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }


                            //userService.RegisterUser(user);
                            //var ticket = new FormsAuthenticationTicket(viewModel.Email, true, 3);
                            //var jsonString = JsonConvert.SerializeObject(ticket);
                            //HttpContext.Current.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)));
                            return Ok();
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return InternalServerError();
        }

        #region Private methods
        #region SignInAsync
        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }
        #endregion SignInAsync 
        #endregion SignInAsync
    }
}