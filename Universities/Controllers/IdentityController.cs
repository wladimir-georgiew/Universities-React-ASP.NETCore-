﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Universities.Data.Models;
using Universities.Models.Identity;
using Universities.Services.Identity;

namespace Universities.Controllers
{
    public class IdentityController : ApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IIdentityServices _identityService;

        public IdentityController(
            UserManager<User> userManager,
            IOptions<AppSettings> appSettings,
            IIdentityServices identityService)
        {
            this._userManager = userManager;
            this._appSettings = appSettings.Value;
            this._identityService = identityService;
        }

        [Route(nameof(Register))]
        public async Task<JsonResult> Register(RegisterRequestModel model)
        {
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await this._userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return new JsonResult(Ok("Successfully created user"));
            }

            var errors = result.Errors.Select(x => x.Description);
            return new JsonResult(BadRequest(errors));
        }

        [Route(nameof(Login))]
        public async Task<JsonResult> Login(LoginRequestModel model)
        {
            var user = await this._userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                return new JsonResult(BadRequest("User not found"));
            }

            var isPasswordValid = await this._userManager.CheckPasswordAsync(user, model.Password);

            if (!isPasswordValid)
            {
                return new JsonResult(Unauthorized("Wrong password"));
            }

            var encryptedToken = this._identityService.GetEncryptedJWT(user, this._appSettings.Secret);
            return new JsonResult(Ok(encryptedToken));
        }
    }
}
